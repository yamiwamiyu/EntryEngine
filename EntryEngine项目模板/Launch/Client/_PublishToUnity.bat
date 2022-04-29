:: 发布Unity，不需要EntryEngine.dll，已经打包入UnityRuntime.bytes中了
move EntryEngine.dll ..\EntryEngine.dll

:: 若是发布到正式服svn目录上，可以先拷贝Unity中的文件到正式服目录中
:: copy ..\..\Publish\Project\Assets\StreamingAssets\UnityRuntime.bytes StreamingAssets\
:: copy ..\..\Publish\Project\Assets\StreamingAssets\web.config StreamingAssets\
..\..\EntryBuilder PublishToUnity "" ..\..\Publish\Project\Assets\StreamingAssets\

move ..\EntryEngine.dll EntryEngine.dll