cd /d %~dp0
:: 需要改数据库的名字，则修改dbname即可
Server.exe "SetDatabase Server=127.0.0.1;Port=3306;User=root;Password=123456; ASSDK true" "SetCache 2022-05-01" "Launch 888"