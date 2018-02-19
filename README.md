# CommadLineParser
コマンドライン引数の解析を補助するライブラリです。

---
使用方法
```
    //! 引数の解析結果を保持するクラス 
    public class TestArg  
    {
        // --filepath <arg>の argの部分が代入されます。  
        [CmdArgOption(CommandName = "filepath", Description = "ファイルパス")]  
        public string FilePath { get;set;}  
        
        // 引数を取らないOn/Offのコマンドも定義できます。
        [CmdFlagOption(CommandName = "log", ShortcutName = "l" , Description = "ログの出力を有効化します。")]
        public bool Flag { get; set; }  
    }
    
    public static void Main( string[] args)
    {
        var parser = new CommandLine.Parser();
        TestArg testArgs = parser.Parse<TestArg>( args );
        
        //メイン処理...
    }
```
例の場合以下の様なヘルプが作成されます。

```
使用可能な引数
-v,--version
説明	バージョンの表示
--filepath<arg>
説明	ファイルパス
-l,--log
説明	ログの出力を有効化します。
```
