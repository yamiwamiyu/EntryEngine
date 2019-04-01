copy ..\..\..\..\EntryEditor\bin\Debug\EntryEditor.exe EntryEditor\
copy ..\..\..\..\EntryEditor\bin\Debug\*.dll EntryEditor\

::copy ..\..\..\..\EntryEditor\bin\Debug\*.pdb EntryEditor\
copy ..\..\..\..\Xna\bin\CLIENT\*.dll EntryEditor\

copy ..\..\..\..\Xna\*.dll EntryEditor\

copy ..\..\..\..\EntryEngine\bin\CLIENT\*.dll EntryEditor\

move EntryEditor\fmodex.dll fmodex.dll
..\EntryBuilder.exe BuildLinkShell EntryEditor\ 3.5 10 EntryEditor.exe "" "2019-12-31"
del fmodex.dll