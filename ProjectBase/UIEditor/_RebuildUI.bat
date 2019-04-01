:: 拷贝代码文件到项目
call _CopyToCode.bat
:: 重新编译项目
..\EntryBuilder.exe BuildDll ..\Code\Client\Client\ ..\Launch\Client\Client.dll 3.5 "..\Code\EntryEngine.dll" false
:: 拷贝可能新生成的翻译表到运行目录
copy ..\Design\LANGUAGE.csv ..\Launch\Client\Content\Tables\LANGUAGE.csv
:: 输出最终翻译表
..\EntryBuilder.exe BuildOutputCSV ..\Launch\Client\Content\Tables\LANGUAGE.csv ""
:: 切换到运行目录运行应用客户端程序
cd ..\Launch\Client
call Xna.exe