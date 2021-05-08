copy ..\..\..\..\EntryEditor\bin\Debug\EntryEditor.exe EntryEditor\
copy ..\..\..\..\EntryEditor\bin\Debug\*.dll EntryEditor\

::copy ..\..\..\..\EntryEditor\bin\Debug\*.pdb EntryEditor\
copy ..\..\..\..\Xna\bin\CLIENT\*.dll EntryEditor\

::copy ..\..\..\..\Xna\*.dll EntryEditor\

copy ..\..\..\..\EntryEngine\bin\Debug\*.dll EntryEditor\

del EntryEditor\Microsoft.Xna.Framework.dll
del EntryEditor\Microsoft.Xna.Framework.Game.dll

move EntryEditor\fmodex.dll fmodex.dll
..\EntryBuilder.exe BuildLinkShell EntryEditor\ 3.5 10 EntryEditor.exe "x86" "" true ""
del fmodex.dll