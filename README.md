# EntryEngine
### 入口引擎开发环境
1. 开发框架：.net framework 3.5，未引用其它第三方框架，理论可直接编译到.net core，standard或其它
2. 编码工具：Visual Studio 2010，可用任意更高版本

### 入口引擎可用于的开发场景
1. 服务端
    包含TCP，HTTP，WebSocket

2. 2D游戏客户端
    包含基于Xna(客户机无需安装Xna)发布的Windows PC平台(.exe)
    包含基于Unity3D可导出的所有平台

### 使用入口引擎
1. 若仅使用入口引擎的序列化，数据库等个别模块，则克隆EntryEngine编译出dll引用即可
2. 若要使用入口引擎开发流程开发完整项目，则建议克隆ProjectBase文件夹即可，这个文件夹已经搭建好了使用入口引擎开发完整项目的模板，项目模板可开发服务端(基于控制台应用程序)和2D游戏客户端。具体各模块的使用可参考下方的Demo视频教程

### 入口引擎功能模块
1. EntryEngine

### svn拉取EntryEngine项目
gitee:       svn://gitee.com/yamiwamiyu/EntryEngine
github:      https://github.com/yamiwamiyu/EntryEngine.git

### 模块 Demo 视频教程
 **https://space.bilibili.com/2832385?spm_id_from=333.788.b_765f7570696e666f.2** 
bilibili关注暗和自由，入口引擎模块教程视频持续更新中
QQ官方交流群 981793689
