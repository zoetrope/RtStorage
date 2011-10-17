:: ドキュメントをインストールするためのFragmentを生成する
:: -dir   検索するディレクトリ
:: -gg    IDを自動生成する
:: -sfrag Fragmentとディレクトリの識別子を生成しない
:: -cg    ComponentGroupの名前
:: -dr    ディレクトリ名
:: -out   出力ファイル名
:: -var   ソースディレクトリ
"C:\Program Files\Windows Installer XML v3.5\bin\heat.exe" dir ..\doc\build\html\ -gg -sfrag -cg ComponentGroup.Document -dr Directory.Documents -out Document.wxs -var var.DocumentDir
