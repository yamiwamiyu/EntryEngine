del /q ..\..\Launch\Server\Tables\*.csv
del /q ..\..\Launch\Client\Content\Tables\*.csv
del /q ..\Tables_Build\*.xlsx
:: 构建数据表
:: 构建数据表到客户端
copy *.xlsx ..\Tables_Build\
..\..\EntryBuilder.exe BuildCSVFromExcel ..\Tables_Build\ ..\..\Launch\Client\Content\Tables\ ..\LANGUAGE.csv 12.0 ..\..\Code\Protocol\_TABLE.design.cs true
:: 复制数据表到服务端
copy ..\..\Launch\Client\Content\Tables\*.csv ..\..\Launch\Server\Tables\