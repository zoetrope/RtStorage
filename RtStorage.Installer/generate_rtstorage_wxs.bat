:: 
:: -dir   検索するディレクトリ
:: -gg    IDを自動生成する
:: -sfrag Fragmentとディレクトリの識別子を生成しない
:: -cg    ComponentGroupの名前
:: -dr    ディレクトリ名
:: -out   出力ファイル名
:: -var   ソースディレクトリ
"C:\Program Files\Windows Installer XML v3.5\bin\heat.exe" dir "..\RtStorage\bin\Release" -gg -sfrag -cg ComponentGroup.RtStorage -dr Directory.Binaries -out RtStorage.wxs -var var.BinariesDir
pause
