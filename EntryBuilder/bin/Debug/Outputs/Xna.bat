copy ..\..\..\..\Xna\bin\CLIENT\*.dll Xna\
::copy ..\..\..\..\Xna\bin\Debug\*.pdb Xna\
move Xna\fmodex.dll fmodex.dll

copy ..\..\..\..\EntryEngine\bin\Debug\*.dll Xna\
::copy ..\..\..\..\EntryEngine\bin\Debug\*.pdb Xna\
..\EntryBuilder.exe BuildLinkShell Xna\ 3.5 10 Xna.exe "x86" "" true ""