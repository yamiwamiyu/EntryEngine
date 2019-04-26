del fmodex.dll
..\EntryBuilder.exe BuildDll ..\..\..\..\EntryEngine\ Dummy\EntryEngine.dll 3.5 "" true CLIENT;SERVER;

copy ..\..\..\..\Xna\bin\Debug\*.dll Xna\
..\EntryBuilder.exe BuildDll ..\..\..\..\Xna\ Dummy\Xna.dll 3.5 "System.Drawing.dll|System.Windows.Forms.dll|Xna\EntryEngine.dll|..\..\..\..\Xna\Microsoft.Xna.Framework.dll|..\..\..\..\Xna\Microsoft.Xna.Framework.Game.dll" true ""

..\EntryBuilder.exe BuildDll ..\..\..\..\Cmdline\ Dummy\Cmdline.dll 3.5 "..\EntryEngine.dll" true ""

..\EntryBuilder.exe BuildDll ..\..\..\..\Unity\ Dummy\Unity.dll 3.5 "..\EntryEngine.dll|..\..\..\..\Unity\bin\Debug\UnityEngine.dll" true ""