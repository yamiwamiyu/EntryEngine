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

### 入口引擎包含的功能模块
通用模块
1. 数据结构：树Tree<T>，平衡二叉树AVLTree<T>，池Pool<T>，表TableList<T, K>
2. 协程/异步：时间GameTime，协程/异步基类Async，时间线Timeline和关键帧KeyFrame<T>
3. 静态辅助：日志_LOG，文件_IO，数学_MATH，随机_RANDOM，其它Utility
4. 序列化：二进制格式ByteWriter & ByteReader，支持引用的二进制格式ByteRefWriter & ByteRefReader，Json格式JsonWriter & JsonReader & JsonObject，Excel格式CSVWriter & CSVReader，XML格式XmlWriter & XmlReader & XmlNode

前端模块
1. 数据结构：二维向量VECTOR2，三维向量VECTOR3，三维矩阵MATRIX，二维矩阵MATRIX2x3，颜色COLOR，直线LINE，矩形RECT，圆形CIRCLE，图Graph<T>
2. 用户输入：鼠标MOUSE，触屏TOUCH，键盘KEYBOARD，文字输入InputText
3. 资源和加载：资源Content，包含图片TEXTURE，着色器SHADER，字体FONT，声音SOUND；资源管理ContentManager，包含加载ContentPipeline，缓存，卸载；
4. 图片&动画：渲染GRAPHICS，图片基类TEXTURE，大图上的一小块PIECE(EntryBuilder生产)，九宫格PATCH(EntryBuilder生产)，序列帧ANIMATION(EntryBuilder生产)，例子系统ParticleSystem(EditorParticle编辑器生产)
5. UI：UI管理Entry，UI基类UIElement，场景UIScene，UI特效UIEffect，图片框TextureBox，按钮Button，单选/复选框CheckBox，下拉框DropDown，标签Label，面板Panel，输入框TextBox

后端模块
1. 数据库：ORM数据映射_DATABASE.Database，CodeFirst生成MySQL数据库EntryBuilder.BuildDatabaseMysql
2. 服务器：TCP服务器ProxyTcp，HTTP服务器ProxyHttpAsync，WebSocket服务器ProxyWebSocket
3. 交互协议：交互协议ProtocolStubAttribute，对接口标记此特性后，由EntryBuilder.BuildProtocolAgentBinary生成TCP交互代码，由EntryBuilder.BuildProtocolAgentHttp生成HTTP和WebSocket交互代码

### svn拉取EntryEngine项目
gitee:       svn://gitee.com/yamiwamiyu/EntryEngine
github:      https://github.com/yamiwamiyu/EntryEngine.git

### 模块 Demo 视频教程
 **https://space.bilibili.com/2832385?spm_id_from=333.788.b_765f7570696e666f.2** 
bilibili关注暗和自由，入口引擎模块教程视频持续更新中
QQ官方交流群 981793689
