:: 设置变量d为要反编译的aar文件
set d=open_ad_sdk_4.4.0.2.aar

:: 创建temp目录，jar解压时只能解压到当前目录
mkdir temp
cd temp\
jar -xvf ..\%d%

:: 移动classes.jar，删除temp的内容，同样的方法解压classes.jar
cd ..\
mkdir tempj
move temp\classes.jar tempj\
copy classes.jar ..\
rd /s /q temp
cd tempj
jar -xvf classes.jar

:: 会在tempj目录下生成src目录即为源码，将src移动到aar同名的文件夹内，删除tempj
..\jad158g.win\jad -o -r -s java -d src com/**/*.class
cd ..\
move tempj\src %d:~0,-4%
rd /s /q tempj
