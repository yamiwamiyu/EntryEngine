copy ..\..\..\..\Unity\bin\Debug\Unity.dll Unity\
copy ..\..\..\..\EntryEngine\bin\CLIENT\*.dll Unity\

::copy ..\..\..\..\Unity\bin\Debug\*.dll Unity\
::copy ..\..\..\..\Unity\bin\Debug\*.pdb Unity\

..\EntryBuilder.exe BuildLinkShell Unity\ 3.5 10 UnityRuntime.bytes "" "2019-12-31"