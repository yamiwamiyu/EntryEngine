copy ..\..\..\..\Unity\bin\Debug\Unity.dll Unity\
copy ..\..\..\..\Unity\bin\Debug\EntryEngine.dll Unity\

::copy ..\..\..\..\Unity\bin\Debug\*.dll Unity\
::copy ..\..\..\..\Unity\bin\Debug\*.pdb Unity\

..\EntryBuilder.exe BuildLinkShell Unity\ 3.5 10 UnityRuntime.bytes "" "" true ""