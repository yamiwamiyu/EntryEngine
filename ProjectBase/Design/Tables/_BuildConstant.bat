del /q ..\..\Launch\Client\Content\Tables\C.xml
del /q ..\..\Launch\Client\Content\Tables\CC.xml
del /q ..\..\Launch\Server\Tables\C.xml
del /q ..\..\Launch\Server\Tables\CS.xml
:: 构建常量表
copy _C.xlsx ..\Tables_Build\
copy _CC.xlsx ..\Tables_Build\
copy _CS.xlsx ..\Tables_Build\
..\..\EntryBuilder.exe BuildConstantTable ..\Tables_Build\_C.xlsx ..\..\Launch\Server\Tables\C.xml ..\..\Code\Protocol\Protocol\_C.design.cs true 12.0
..\..\EntryBuilder.exe BuildConstantTable ..\Tables_Build\_CC.xlsx ..\..\Launch\Client\Content\Tables\CC.xml ..\..\Code\Client\Client\_CC.design.cs true 12.0
..\..\EntryBuilder.exe BuildConstantTable ..\Tables_Build\_CS.xlsx ..\..\Launch\Server\Tables\CS.xml ..\..\Code\Server\Server\_CS.design.cs true 12.0
copy ..\..\Launch\Server\Tables\C.xml ..\..\Launch\Client\Content\Tables\