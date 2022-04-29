using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine;
using EntryEngine.Network;
using EntryEngine.Cmdline;
using System.Net;
using EntryEngine.Serialize;
using System.IO;
using Server.Impl;

namespace Server
{
    public interface ICmd
    {
        /// <summary>设置数据库</summary>
        /// <param name="dbconn">数据库连接字符串</param>
        /// <param name="dbname">数据库名称</param>
        /// <param name="isRebuild">是否自动更新数据库结构</param>
        void SetDatabase(string dbconn, string dbname, bool isRebuild);
        /// <summary>执行查询语句并将执行结果打印到控制台</summary>
        void Select(string sql);
        /// <summary>执行非查询语句</summary>
        void Update(string sql);

        /// <summary>线上采用运维管理工具开启服务时，应调用此方法来将日志写入运维管理工具</summary>
        void LoggerToManager();
        /// <summary>测试模式</summary>
        void DebugMode();
        /// <summary>设置上传文件的资源目录</summary>
        /// <param name="localDir">写入文件的计算机本地目录</param>
        /// <param name="accessUrl">访问上传的资源的外网地址</param>
        void SetResourceDir(string localDir, string accessUrl);
        /// <summary>加载游戏数据表</summary>
        void LoadTable();
        /// <summary>启动服务器</summary>
        /// <param name="port">服务器端口</param>
        void Launch(ushort port);

        /// <summary>注册一个后台管理员账号</summary>
        void AddAdmin(string name, string password);
        /// <summary>主动查询订单支付状态</summary>
        /// <param name="orderID">订单ID，数字ID和字符串ID都可</param>
        //void QueryOrder(string orderID);
        /// <summary>设置支付宝参数</summary>
        /// <param name="notifyUrl">支付成功后支付宝回调通知给我们服务器后台的回调地址</param>
        /// <param name="returnUrl">前端支付完成后页面跳转的地址</param>
        void SetZFB(string notifyUrl, string returnUrl);
    }
    partial class Service : ProxyHttpAsync, ICmd
    {
        ParallelRouter<HttpListenerContext> router = new ParallelRouter<HttpListenerContext>();
        ParallelJsonHttpService[] nonLogin = new ParallelJsonHttpService[8];
        ParallelJsonHttpService[] doneLogin = new ParallelJsonHttpService[8];

        protected override Link InternalAccept(HttpListenerContext context)
        {
            router.Route(context);
            return null;
        }
        protected override IEnumerator<LoginResult> Login(Link link)
        {
            throw new NotImplementedException();
        }
        protected override void OnUpdate(GameTime time)
        {
            // 每帧执行的业务逻辑
        }

        
        void ICmd.SetDatabase(string dbconn, string dbname, bool isRebuild)
        {
            // 设置数据库
            _DB.IsDropColumn = true;
            _DB.DatabaseName = dbname;
            _DB._DAO = new ConnectionPool(new MYSQL_DATABASE());
            if (isRebuild)
            {
                _DB.IsDropColumn = true;
                _DB._DAO.ConnectionString = dbconn;
            }
            else
            {
                _DB._DAO.ConnectionString = string.Format("{0}Database={1};", dbconn, dbname);
                _DB._DAO.OnTestConnection -= _DB.UPDATE_DATABASE_STRUCTURE;
            }
            _DB._DAO.TestConnection();
            _LOG.Info("设置数据库：{0} 连接字符串：{1}", dbname, dbconn);
        }
        void ICmd.Select(string sql)
        {
            _LOG.Info("查询SQL: {0}", sql);
            _DB._DAO.ExecuteReader(reader =>
            {
                StringBuilder builder = new StringBuilder();
                int field = reader.FieldCount;
                while (reader.Read())
                {
                    for (int i = 0; i < field; i++)
                    {
                        builder.Append(reader[i]);
                        builder.Append('\t');
                    }
                    builder.AppendLine();
                }
                _LOG.Info(builder.ToString());
            }, sql);
        }
        void ICmd.Update(string sql)
        {
            _LOG.Info("更新SQL: {0}, 结果{1}", sql, _DB._DAO.ExecuteNonQuery(sql));
        }

        void ICmd.LoggerToManager()
        {
            if (_LOG._Logger is LoggerFile)
                ((LoggerFile)_LOG._Logger).Base = new LoggerToShell();
            else
                _LOG._Logger = new LoggerFile(new LoggerToShell());
        }
        void ICmd.DebugMode()
        {
            T_SMSCode.IsValid = false;
            _LOG.Info("开启测试模式");
        }
        void ICmd.SetResourceDir(string localDir, string accessUrl)
        {
            localDir = Path.GetFullPath(localDir);
            _LOG.Info("上传资源目录：{0} 资源访问URL：{1}", localDir, accessUrl);
            Environment.CurrentDirectory = localDir;
            _FILE.AccessURL = accessUrl;
        }
        void ICmd.LoadTable()
        {
            // 加载数据表
            //_TABLE.Load("Tables");
            //_LOG.Info("加载数据表完成");
            //_C.Load(_IO.ReadText("Tables\\C.xml"));
            //_LOG.Info("加载共用常量表完成");
            //_CS.Load(_IO.ReadText("Tables\\CS.xml"));
            //_LOG.Info("加载服务端常量表完成");
        }
        void ICmd.Launch(ushort port)
        {
            string prefix;
            if (port == 80)
                prefix = string.Format("http://*/Action/");
            else
                prefix = string.Format("http://*:{0}/Action/", port);

            _LOG.Info("Lauching \"{0}\"", prefix);

            this.RegistServeiceUriPrefix(prefix);
            this.Initialize(IPAddress.Loopback, port);

            {
                // 处理登录和注册
                for (int i = 0; i < nonLogin.Length; i++)
                {
                    nonLogin[i] = new ParallelJsonHttpService();
                    var stubs = GetStubs(null);
                    nonLogin[i].HotFixAgent(GetStubs);
                }
                router.AddRouter(c =>
                {
                    string token = c.Request.Headers["AccessToken"];
                    if (string.IsNullOrEmpty(token))
                    {
                        // 注册时，必须队列执行（否则可能创建多个相同账号），所以必须分配到相同线程
                        if (c.Request.Url.LocalPath == "/Action/1/Register")
                            return 1024;
                        // 随机分配线程
                        return _RANDOM.Next(0, 1000);
                    }
                    else
                        // 不处理
                        return -1;
                }, nonLogin);
            }
            {
                // 处理登录后的请求
                doneLogin = new ParallelJsonHttpService[8];
                for (int i = 0; i < doneLogin.Length; i++)
                {
                    doneLogin[i] = new ParallelJsonHttpService();
                    doneLogin[i].HotFixAgent(GetStubs);
                }
                router.AddRouter(c =>
                {
                    string token = c.Request.Headers["AccessToken"];
                    if (string.IsNullOrEmpty(token))
                    {
                        c.Response.StatusCode = 403;
                        c.Response.Close();
                        return -1;
                    }
                    // 按用户分配线程
                    return _MATH.Abs(token.GetHashCode());
                }, doneLogin);
            }

            // 正式环境通知管理器服务器已正常开启
            _LOG.Write(255, string.Empty);
        }
        public static
            StubHttp[]
            GetStubs(Func<HttpListenerContext> getContext)
        {
            var _service = new ImplIService();
            var _iservice = new IServiceStub(_service);

            var _user = new ImplIUser();
            var _iuser = new IUserStub(_user);
            _iuser.__GetAgent = () =>
            {
                _user.CheckToken(getContext().Request.Headers["AccessToken"]);
                return null;
            };

            var _center = new ImplICenter();
            var _icenter = new ICenterStub(_center);
            _icenter.__GetAgent = () =>
            {
                _center.CheckToken(getContext().Request.Headers["AccessToken"]);
                return null;
            };

            StubHttp[]
                stubs = 
                { 
                    _iservice, 
                    _iuser,
                    _icenter,
                };
            return stubs;
        }

        void ICmd.AddAdmin(string name, string password)
        {
            "用户名不能为空".Check(string.IsNullOrEmpty(name));
            "密码不能为空".Check(string.IsNullOrEmpty(password));

            ImplICenter center = new ImplICenter();
            center.InitializeByAccount(name);
            "账号已存在".Check(center.User != null);

            T_CENTER_USER user = new T_CENTER_USER();
            user.Name = name;
            user.Account = name;
            user.Password = password;
            if (T_SMSCode.IsTelephone(name))
                user.Phone = long.Parse(name);

            center.Register(user, ELoginWay.其它);
            _LOG.Info("注册后台账号：{0}", name);
        }
        void ICmd.SetZFB(string notifyUrl, string returnUrl)
        {
            //_LOG.Info("设置支付宝参数：\r\n回调通知：{0}\r\n跳转页面：{1}", notifyUrl, returnUrl);
            //_DB._ZFB.NOTIFY_URL = notifyUrl;
            //_DB._ZFB.RETURN_URL = returnUrl;
        }
    }
}
