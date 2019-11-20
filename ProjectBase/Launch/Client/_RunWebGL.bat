cd %~dp0
cd ..\..\Publish\WebGL
start SimpleHttpService.exe 65535
start http://localhost:65535/index.html