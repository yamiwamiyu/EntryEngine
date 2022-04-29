del /q ..\..\Launch\Client\Content\Tables\C.xml
:: 构建常量表
copy _C.xlsx ..\Tables_Build\
..\..\EntryBuilder.exe BuildConstantTable ..\Tables_Build\_C.xlsx ..\..\Launch\Client\Content\Tables\C.xml ..\..\Code\Client\Client\_C.design.cs false 12.0