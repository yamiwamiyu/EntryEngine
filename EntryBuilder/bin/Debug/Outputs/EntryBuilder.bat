copy ..\EntryBuilder.exe EntryBuilder\
copy ..\EntryEngine.dll EntryBuilder\
copy ..\ReflectionHelper.dll EntryBuilder\
::del EntryBuilder\CSharp.dll
::copy ..\*.pdb EntryBuilder\
::copy ..\EntryBuilder.pdb EntryBuilder\
..\EntryBuilder.exe BuildLinkShell EntryBuilder\ 4.0 0 EntryBuilder.exe ""  "" true ""