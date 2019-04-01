copy ..\..\..\..\Xna\bin\Debug\*.dll Xna\
::copy ..\..\..\..\Xna\bin\Debug\*.pdb Xna\
move Xna\fmodex.dll fmodex.dll

copy ..\..\..\..\EntryEngine\bin\SERVER\*.dll Xna\
::copy ..\..\..\..\EntryEngine\bin\SERVER\*.pdb Xna\

..\EntryBuilder.exe BuildLinkShell Xna\ 3.5 10 Xna.exe "" "2019-12-31"