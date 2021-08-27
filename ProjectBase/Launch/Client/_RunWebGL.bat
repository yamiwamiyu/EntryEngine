:: 开启一个用于测试热更新的简易服务器，就不需要特别开启IIS了
cd %~dp0
cd ..\..\Publish\
start SimpleHttpService.exe 9901
start http://localhost:9901/index.html