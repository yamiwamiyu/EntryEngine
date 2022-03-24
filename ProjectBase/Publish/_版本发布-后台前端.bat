if not exist __后台前端 (
   svn co https://svn服务器IP/svn/项目名/ __后台前端\ --username svn账号名 --password svn密码
)
svn update ..\Code\WebCenter\dist\
..\EntryBuilder DeleteContentExcept __后台前端 .svn
xcopy /D /Y /E ..\Code\WebCenter\dist\*.* __后台前端

::svn add __后台前端 --auto-props --force
::svn commit __后台前端 -m "Publish __后台前端 Commit"
pause