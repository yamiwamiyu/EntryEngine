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
        void LoggerToManager();
        void LoadTable();
        void SetDatabase(string dbconn, string dbname, bool isRebuild);
        void Launch(ushort port);
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

        void ICmd.LoggerToManager()
        {
            if (_LOG._Logger is LoggerFile)
            {
                ((LoggerFile)_LOG._Logger).Dispose();
            }
            _LOG._Logger = new LoggerFile(new LoggerToShell());
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
            var impl1 = new ImplProtocol1();
            Protocol1Stub _p1 = new Protocol1Stub(impl1);

            var impl2 = new ImplProtocol2();
            Protocol2Stub _p2 = new Protocol2Stub(impl2);
            _p2.__GetAgent = () =>
            {
                impl2.CheckToken(getContext());
                return null;
            };

            StubHttp[]
                stubs = 
                { 
                    _p1, 
                    _p2 
                };
            return stubs;
        }
    }
    public class StubBase
    {
        public T_PLAYER Player;
        protected static Dictionary<string, T_PLAYER> _cachePlayer = new Dictionary<string, T_PLAYER>();

        public void OP(string op, string way, int sign, int statistic, string detail)
        {
            T_OPLog log = new T_OPLog();
            log.PID = Player.ID;
            log.Time = DateTime.Now;
            log.Operation = op;
            log.Way = way;
            log.Sign = sign;
            log.Statistic = statistic;
            log.Detail = detail;
            _DB._T_OPLog.Insert(log);
        }
        public void OP(string op, string way, int sign, int statistic)
        {
            OP(op, way, sign, statistic, null);
        }
        public void OP(string op, string way, int sign)
        {
            OP(op, way, sign, 0, null);
        }
        public void OP(string op, string way)
        {
            OP(op, way, 0, 0, null);
        }
        public void OP(string op, int sign)
        {
            OP(op, null, sign, 0, null);
        }

        public void CheckToken(HttpListenerContext context)
        {
            string token = context.Request.Headers["AccessToken"];
            if (string.IsNullOrEmpty(token))
                throw new InvalidOperationException("没有登录!");

            // 做分布式使用多进程时，请不要使用以下缓存机制

            // 缓存
            lock (_cachePlayer)
            {
                _cachePlayer.TryGetValue(token, out Player);
            }
            bool hasCache = Player != null;
            if (!hasCache)
            {
                Player = _DB._T_PLAYER.Select(null, "WHERE Token=@p0", token);
                if (Player != null)
                {
                    Player.LastRefreshLoginTime = Player.LastLoginTime;
                    lock (_cachePlayer)
                    {
                        _cachePlayer.Add(token, Player);
                    }
                }
            }
            // 检查登录
            if (Player != null)
            {
                var now = GameTime.Time.CurrentFrame;
                var elapsed = (now - Player.LastLoginTime).TotalMinutes;
                // Token过期时间
                if (elapsed < 30)
                {
                    // 刷新Token过期时间到数据库
                    if (elapsed > 5)
                    {
                        Player.LastLoginTime = now;
                        Player.LastRefreshLoginTime = now;
                        _DB._T_PLAYER.Update(Player, null, ET_PLAYER.LastLoginTime);
                    }
                    return;
                }
                else
                {
                    _LOG.Debug("用户:{0}缓存过期", Player.Name);
                    lock (_cachePlayer)
                    {
                        _cachePlayer.Remove(token);
                    }
                }
            }
            throw new InvalidOperationException("没有登录!");
        }
    }
}
