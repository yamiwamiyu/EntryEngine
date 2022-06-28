cd ..\..\..\..\..\
set dir="EntryEngine项目\通用后台管理网页"
xcopy /D /Y /E EntryEngine项目模板 %dir%
:: 删除服务端不必要的内容
cd %dir%
rmdir /Q /S Code\Client
del /Q /S Code\Project.sln
rmdir /Q /S Design
rmdir /Q /S Graphics
rmdir /Q /S Launch\Client
rmdir /Q /S Publish\IOS发布
rmdir /Q /S Publish\Project
rmdir /Q /S Publish\WebGL
rmdir /Q /S Publish\渠道包工具
del /Q /S Publish\_版本发布-Unity.bat
rmdir /Q /S UIEditor
del /Q /S 说明.txt