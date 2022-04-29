if not exist __Unity (
   svn co https://svn服务器IP/svn/项目名/ __Unity\ --username svn账号名 --password svn密码
)
svn update ..\Launch\Client
cd ..\Launch\Client\
:: Release编译项目
call _CompileReleaseClient.bat
:: 发布到Unity项目
call _PublishToUnity.bat
:: 资源加密发布
call _EncryptContentToUnity.bat
cd /d %~dp0

:: 直接发布
xcopy /S /Y ..\Publish\Project\Assets\StreamingAssets\*.* __Unity\
del /S /Q __Unity\*.meta

::svn add __Unity --auto-props --force
::svn commit __Unity -m "Publish __Unity Commit"
pause