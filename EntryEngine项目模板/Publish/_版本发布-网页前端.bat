if not exist __APP前端 (
   svn co https://svn服务器IP/svn/项目名/ __网页前端\ --username svn账号名 --password svn密码
)
svn update ..\Code\WebClient\dist\
..\EntryBuilder DeleteContentExcept __网页前端 .svn
xcopy /D /Y /E ..\Code\WebClient\dist\*.* __网页前端

::svn add __网页前端 --auto-props --force
::svn commit __网页前端 -m "Publish __网页前端 Commit"
pause