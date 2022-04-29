:: 不需要Content内的所有资源，仅发布程序到Unity，资源由热更新自行下载
ren Content #Content
md Content
move EntryEngine.dll ..\EntryEngine.dll
..\..\EntryBuilder PublishToUnity "" ..\..\Publish\Project\Assets\StreamingAssets\
move ..\EntryEngine.dll EntryEngine.dll
rd /S /Q Content
ren #Content Content