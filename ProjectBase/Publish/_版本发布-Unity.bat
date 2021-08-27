if not exist __Unity (
   svn co https://svn服务器IP/svn/项目名/ __Unity\ --username svn账号名 --password svn密码
)
svn update ..\Launch\Client
cd ..\Launch\Client\
:: Release编译项目
call _CompileReleaseClient.bat
call _PublishToUnity.bat
cd /d %~dp0

:: 资源加密发布
md __UnityTemp
xcopy /S /Y ..\Publish\Project\Assets\StreamingAssets\*.* __UnityTemp\
del /S /Q __UnityTemp\*.meta
move __UnityTemp\__filelist.txt __Unity\
move __UnityTemp\__version.txt __Unity\
move __UnityTemp\web.config __Unity\
xcopy /Y __UnityTemp\*.bytes __Unity\
del /Q __UnityTemp\*.bytes
..\..\EntryBuilder BuildEncrypt __UnityTemp __Unity
rd /S /Q __UnityTemp

:: 直接发布
::xcopy /S /Y ..\Publish\Project\Assets\StreamingAssets\*.* __Unity\
::del /S /Q __UnityTemp\*.meta

::svn add __Unity --auto-props --force
::svn commit __Unity -m "Publish __Unity Commit"
pause