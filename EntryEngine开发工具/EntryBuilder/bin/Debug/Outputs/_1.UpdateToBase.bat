copy Dummy\EntryEngine.dll ..\..\..\..\..\EntryEngine项目模板\Code\
copy Dummy\Xna.dll ..\..\..\..\..\EntryEngine项目模板\Code\Client\
copy Dummy\Cmdline.dll ..\..\..\..\..\EntryEngine项目模板\Code\Server\

copy ..\..\..\..\EntryEngine\bin\Debug\EntryEngine.XML ..\..\..\..\ProjectBase\Code\
copy ..\..\..\..\..\EntryEngine运行时\Xna\bin\Debug\Xna.XML ..\..\..\..\ProjectBase\Code\Client\
copy ..\..\..\..\..\EntryEngine运行时\Cmdline\bin\Debug\Cmdline.XML ..\..\..\..\ProjectBase\Code\Server\

copy EntryBuilder.exe ..\..\..\..\..\EntryEngine项目模板\

copy Xna.exe ..\..\..\..\..\EntryEngine项目模板\Launch\Client\
copy Cmdline.exe ..\..\..\..\..\EntryEngine项目模板\Launch\Server\
copy UnityRuntime.bytes ..\..\..\..\..\EntryEngine项目模板\Publish\Project\Assets\StreamingAssets\

copy ..\..\..\..\编辑器\EntryEditor\bin\Debug\Plug-ins\EditorUI\EditorUI.* ..\..\..\..\..\EntryEngine项目模板\UIEditor\Plug-ins\EditorUI\
copy ..\..\..\..\编辑器\EntryEditor\bin\Debug\Plug-ins\EditorUI\Elements.xml ..\..\..\..\..\EntryEngine项目模板\UIEditor\Plug-ins\EditorUI\
copy ..\..\..\..\编辑器\EntryEditor\bin\Debug\Plug-ins\EditorUI\Template.design.txt ..\..\..\..\..\EntryEngine项目模板\UIEditor\Plug-ins\EditorUI\
copy ..\..\..\..\编辑器\EntryEditor\bin\Debug\Plug-ins\EditorUI\Template.txt ..\..\..\..\..\EntryEngine项目模板\UIEditor\Plug-ins\EditorUI\
copy ..\..\..\..\编辑器\EntryEditor\bin\Debug\Plug-ins\EditorParticle\EditorParticle.* ..\..\..\..\..\EntryEngine项目模板\UIEditor\Plug-ins\EditorParticle\
copy ..\..\..\..\编辑器\EntryEditor\bin\Debug\Plug-ins\EditorParticle\Content\*.* ..\..\..\..\..\EntryEngine项目模板\UIEditor\Plug-ins\EditorParticle\Content\
copy ..\..\..\..\编辑器\EntryEditor\bin\Debug\Plug-ins\EditorParticle\Preview\*.* ..\..\..\..\..\EntryEngine项目模板\UIEditor\Plug-ins\EditorParticle\Preview\
copy ..\..\..\..\编辑器\EntryEditor\bin\Debug\Plug-ins\EditorPicture\EditorPicture.* ..\..\..\..\..\EntryEngine项目模板\UIEditor\Plug-ins\EditorPicture\
copy EntryEditor.exe ..\..\..\..\..\EntryEngine项目模板\UIEditor\
pause