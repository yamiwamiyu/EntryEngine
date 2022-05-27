copy ..\..\..\..\..\EntryEngine运行时\Cmdline\bin\Debug\*.dll Cmdline\
copy ..\..\..\..\..\EntryEngine运行时\Cmdline\bin\Debug\*.pdb Cmdline\

copy ..\..\..\..\..\EntryEngine\bin\Debug\*.dll Cmdline\
copy ..\..\..\..\..\EntryEngine\bin\Debug\*.pdb Cmdline\

..\EntryBuilder.exe BuildLinkShell Cmdline\ 4.0 10 Cmdline.exe "" "" true ""