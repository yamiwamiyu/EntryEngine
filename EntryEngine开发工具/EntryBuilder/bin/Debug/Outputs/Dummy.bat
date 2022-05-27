del fmodex.dll
..\EntryBuilder.exe BuildDll ..\..\..\..\..\EntryEngine\ Dummy\EntryEngine.dll 3.5 "" true CLIENT;SERVER;DEBUG;

copy ..\..\..\..\..\EntryEngine运行时\Xna\bin\Debug\*.dll Xna\
..\EntryBuilder.exe BuildDll ..\..\..\..\..\EntryEngine运行时\Xna\ Dummy\Xna.dll 3.5 "System.Drawing.dll|System.Windows.Forms.dll|Xna\EntryEngine.dll|..\..\..\..\..\EntryEngine运行时\Xna\Microsoft.Xna.Framework.dll|..\..\..\..\..\EntryEngine运行时\Xna\Microsoft.Xna.Framework.Game.dll" true ""

..\EntryBuilder.exe BuildDll ..\..\..\..\..\EntryEngine运行时\Cmdline\ Dummy\Cmdline.dll 3.5 "..\EntryEngine.dll" true ""

..\EntryBuilder.exe BuildDll ..\..\..\..\..\EntryEngine运行时\Unity\ Dummy\Unity.dll 3.5 "..\EntryEngine.dll|..\..\..\..\..\EntryEngine运行时\Unity\bin\Debug\UnityEngine.dll" true ""