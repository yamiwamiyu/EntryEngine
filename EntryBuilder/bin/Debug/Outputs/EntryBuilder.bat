copy ..\EntryBuilder.exe EntryBuilder\
copy ..\*.dll EntryBuilder\
del EntryBuilder\CSharp.dll
::copy ..\*.pdb EntryBuilder\
::copy ..\EntryBuilder.pdb EntryBuilder\
..\EntryBuilder.exe BuildLinkShell EntryBuilder\ 4.0 30 EntryBuilder.exe "" ""