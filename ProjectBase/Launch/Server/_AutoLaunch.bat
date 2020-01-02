cd /d %~dp0
:: 需要改数据库的名字，则修改dbname即可
Cmdline.exe "Launch 888 Server=127.0.0.1;Port=3306;User=root;Password=123456; dbname" "LoadTable"