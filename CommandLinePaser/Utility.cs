using System;

namespace CommandLine
{
    internal static class Utility
    {
        public static bool Parse(ref object value, string arg)
        {
            try
            {
                switch (value)
                {
                    case int _:
                        value = int.Parse(arg);
                        break;
                    case short _:
                        value = short.Parse(arg);
                        break;
                    case long _:
                        value = long.Parse(arg);
                        break;
                    case uint _:
                        value = uint.Parse(arg);
                        break;
                    case ushort _:
                        value = ushort.Parse(arg);
                        break;
                    case ulong _:
                        value = ulong.Parse(arg);
                        break;
                    case float _:
                        value = float.Parse(arg);
                        break;
                    case double _:
                        value = double.Parse(arg);
                        break;
                    case decimal _:
                        value = decimal.Parse(arg);
                        break;
                    case char _:
                        value = char.Parse(arg);
                        break;
                    case string _:
                        value = arg;
                        break;
                    case byte _:
                        value = byte.Parse(arg);
                        break;
                    case sbyte _:
                        value = sbyte.Parse(arg);
                        break;
                    case bool _:
                        value = bool.Parse(arg);
                        break;
                    case Enum _:
                        value = Enum.Parse(value.GetType(), arg);
                        break;
                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
