copy ..\..\..\..\Xna\bin\Debug\*.dll Xna\
::copy ..\..\..\..\Xna\bin\Debug\*.pdb Xna\
move Xna\fmodex.dll fmodex.dll

copy ..\..\..\..\EntryEngine\bin\CLIENT\*.dll Xna\
::copy ..\..\..\..\EntryEngine\bin\CLIENT\*.pdb Xna\

..\EntryBuilder.exe BuildLinkShell Xna\ 3.5 10 Xna.exe "" "2019-12-31"
del fmodex.dll