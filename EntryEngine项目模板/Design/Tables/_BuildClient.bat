del /q ..\..\Launch\Client\Content\Tables\*.csv
del /q ..\Tables_Build\*.xlsx
:: 构建数据表
:: 构建数据表到客户端
copy *.xlsx ..\Tables_Build\
..\..\EntryBuilder.exe BuildCSVFromExcel ..\Tables_Build\ ..\..\Launch\Client\Content\Tables\ ..\LANGUAGE.csv 12.0 ..\..\Code\Client\Client\_TABLE.design.cs true