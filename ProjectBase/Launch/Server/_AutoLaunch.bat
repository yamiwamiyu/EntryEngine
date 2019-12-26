:: 下面是默认盘符，当前文件夹是D盘，就改成"D:"即可
C:
cd %~dp0
:: 需要改数据库的名字，则修改dbname即可
Cmdline.exe "Launch 888 Server=127.0.0.1;Port=3306;User=root;Password=123456; dbname" "LoadTable"