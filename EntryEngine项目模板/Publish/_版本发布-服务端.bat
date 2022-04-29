if not exist __服务端 (
   svn co https://svn服务器IP/svn/项目名/ __服务端\ --username svn账号名 --password svn密码
)
svn update ..\Launch\Server
xcopy /D /Y /E ..\Launch\Server\*.dll __服务端
xcopy /D /Y /E ..\Launch\Server\*.pdb __服务端
xcopy /D /Y /E ..\Launch\Server\Server.exe __服务端
xcopy /D /Y /E ..\Launch\Server\*.csv __服务端

::svn add __服务端 --auto-props --force
::svn commit __服务端 -m "Publish __服务端 Commit"
pause