:: 使用Release编译项目，可以把使用大图，服务器IP的代码放入#if !Debug块中
cd ..\..\
EntryBuilder BuildDll Code\Client\Client\ Launch\Client\Client.dll 3.5 "Launch\Client\Protocol.dll|Launch\Client\DragonBone.dll" false Release
cd Launch\Client\