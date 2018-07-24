using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace CommandLine
{  
    public class Parser
    {
        internal class CommandObject
        {
            public string Command { get; set; } = string.Empty;
            public string Arg { get; set; } = string.Empty;
        }

        private static string Version => nameof(Version).ToLower();
        private static string Help => nameof(Help).ToLower();

        /// <summary>
        /// コマンドをパースする
        /// </summary>
        /// <typeparam name="TCommandLineArg"></typeparam>
        /// <param name="args"></param>
        /// <returns>nullなら失敗</returns>
        public TCommandLineArg Parse<TCommandLineArg>(string[] args) where TCommandLineArg : class
        {
            var type = typeof(TCommandLineArg);
            var result = Activator.CreateInstance(type);

            try
            {
                var commands = ArgsToCommands(args).ToArray();

                if (commands.Any(x => x.Command == Help || x.Command == "h"))
                {
                    ShowHelp(type);
                    return null;
                }

                if (commands.Any(x => x.Command == Version || x.Command == "v"))
                {
                    ShowVersion();
                    return null;
                }

                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var cmdOptions = properties.Where(x => x.GetCustomAttributes().Any(y => y is CmdOption)).ToArray();
                var cmdArgProps = properties.Where(x => x.GetCustomAttributes().Any(y => y is CmdArgOption)).ToArray();
                var cmdFlagProps = properties.Where(x => x.GetCustomAttributes().Any(y => y is CmdFlagOption)).ToArray();

                var reqDictionary = new Dictionary<string, bool>();
                foreach (var op in cmdOptions)
                {
                    var attr = op.GetCustomAttribute<CmdArgOption>();
                    reqDictionary.Add(attr.ShortcutName+attr.CommandName, attr.IsRequired);
                }

                foreach (var cmd in commands)
                {
                    foreach (var prop in cmdArgProps)
                    {
                        var attr = prop.GetCustomAttribute<CmdArgOption>();
                        if (attr.CommandName == cmd.Command ||
                            attr.ShortcutName == cmd.Command)
                        {
                            var value = (prop.GetValue(result));

                            if (value is null && prop.PropertyType == typeof(string))
                                value = string.Empty;

                            Utility.Parse(ref value, cmd.Arg);
                            prop.SetValue(result, value);
                            reqDictionary[attr.ShortcutName + attr.CommandName] = false;
                        }
                    }

                    foreach (var prop in cmdFlagProps)
                    {
                        var attr = prop.GetCustomAttribute<CmdArgOption>();
                        if (attr.CommandName == cmd.Command ||
                            attr.ShortcutName == cmd.Command)
                        {
                            prop.SetValue(result, true);
                            reqDictionary[attr.ShortcutName + attr.CommandName] = false;
                        }
                    }
                }

                if (reqDictionary.Any(req => req.Value))
                {
                    ShowHelp(type);
                    return null;
                }

            }
            //例外が発生した場合は--helpを表示させる
            catch (Exception)
            {
                ShowHelp(type);
                return null;
            }

            return (TCommandLineArg) result;
        }

        /// <summary>
        /// 引数からコマンドリストを取得する
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private IEnumerable<CommandObject> ArgsToCommands(string[] args)
        {
            var commands = new Stack<CommandObject>();
            foreach (var arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    commands.Push(arg.StartsWith("--")
                        ? new CommandObject() {Command = arg.Substring(2)}
                        : new CommandObject() {Command = arg.Substring(1)});
                }
                else if (commands.Any())
                    commands.Peek().Arg = arg;                    
            }
            return commands;
        }

        /// <summary>
        /// 型からヘルプ情報一覧を出力する
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private IEnumerable<string> TypeToCommandsInfo(Type type)
        {
            var commands = new List<string>();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var cmdArgProps = properties.Where(x => x.GetCustomAttributes().Any(y => y is CmdArgOption));
            var cmdFlagProps = properties.Where(x => x.GetCustomAttributes().Any(y => y is CmdFlagOption));

            commands.Add($"-v,--version{Environment.NewLine}説明\tバージョンの表示");

            foreach (var propertyInfo in cmdArgProps)
            {
                var attr = propertyInfo.GetCustomAttribute<CmdArgOption>();
                var info = string.Empty;

                if (attr.ShortcutName != string.Empty &&
                    attr.CommandName != string.Empty)
                    info = $"-{attr.ShortcutName},--{attr.CommandName}<arg>{Environment.NewLine}説明\t{attr.Description}";
                else if (attr.ShortcutName != string.Empty)
                    info = $"-{attr.ShortcutName}<arg>{Environment.NewLine}説明\t{attr.Description}";
                else if (attr.CommandName != string.Empty)
                    info = $"--{attr.CommandName}<arg>{Environment.NewLine}説明\t{attr.Description}";

                if (attr.IsRequired)
                    info += "(※必須)";
                commands.Add(info);
            }
            foreach (var propertyInfo in cmdFlagProps)
            {
                var attr = propertyInfo.GetCustomAttribute<CmdFlagOption>();
                var info = string.Empty;

                if (attr.ShortcutName != string.Empty &&
                    attr.CommandName != string.Empty)
                    info = $"-{attr.ShortcutName},--{attr.CommandName}{Environment.NewLine}説明\t{attr.Description}";
                else if (attr.ShortcutName != string.Empty)
                    info = $"-{attr.ShortcutName}{Environment.NewLine}説明\t{attr.Description}";
                else if (attr.CommandName != string.Empty)
                    info = $"--{attr.CommandName}{Environment.NewLine}説明\t{attr.Description}";

                if (attr.IsRequired)
                    info += "(※必須)";
                commands.Add(info);
            }
            return commands;
        }


        /// <summary>
        /// ヘルプの表示
        /// </summary>
        /// <returns></returns>
        private void ShowVersion()
        {
            var ver = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            Console.WriteLine("Version{0}", ver.ProductVersion);
        }

        private void ShowHelp(Type type)
        {
            Console.WriteLine($"{Assembly.GetEntryAssembly().FullName}:");
            Console.WriteLine($"");
            Console.WriteLine($"使用可能な引数");
            foreach (var info in TypeToCommandsInfo(type))
            {
                Console.WriteLine(info);
            }
        }
    }
}
