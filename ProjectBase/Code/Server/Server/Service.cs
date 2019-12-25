#if DEBUG
using ParallelQueue = EntryEngine.Network.ParallelBinaryHttpService;
#else
using ParallelQueue = EntryEngine.Network.ParallelJsonHttpService;
#endif
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
        void Launch(ushort port, string dbconn, string dbname);
        void FileService(ushort fileServerPort, string relativePath);
    }
    partial class Service : ProxyHttpAsync, ICmd
    {
        ParallelRouter<HttpListenerContext> router = new ParallelRouter<HttpListenerContext>();
        ParallelQueue[] nonLogin = new ParallelQueue[8];
        ParallelQueue[] doneLogin = new ParallelQueue[8];

        protected override Link InternalAccept(System.Net.HttpListenerContext context)
        {
            throw new NotImplementedException();
        }
        protected override IEnumerator<LoginResult> Login(Link link)
        {
            throw new NotImplementedException();
        }
        protected override Link Accept()
        {
            HttpListenerContext[] requests;
            lock (Contexts)
            {
                requests = Contexts.ToArray();
                Contexts.Clear();
            }

            // 分配异步线程处理请求
            for (int i = 0; i < requests.Length; i++)
                router.Route(requests[i]);

            return null;
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
        void ICmd.Launch(ushort port, string dbconn, string dbname)
        {
            // 设置数据库
            //_DB.IsDropColumn = true;
            //_DB.DatabaseName = dbname;
            //_DB._DAO = new ConnectionPool(new MYSQL_DATABASE());
            //_DB._DAO.ConnectionString = dbconn;
            //_DB._DAO.TestConnection();

            this.RegistServeiceUriPrefix(string.Format("http://*:{0}/Action/", port));
            this.Initialize(System.Net.IPAddress.Loopback, port);

            {
                // 处理登录和注册
                for (int i = 0; i < nonLogin.Length; i++)
                {
                    nonLogin[i] = new ParallelQueue();
                    nonLogin[i].HotFixAgent(GetStubs);
                }
                router.AddRouter(c =>
                {
                    string token = c.Request.Headers["AccessToken"];
                    if (string.IsNullOrEmpty(token))
                        // 随机分配线程
                        return _RANDOM.Next(0, 1000);
                    else
                        // 不处理
                        return -1;
                }, nonLogin);
            }
            {
                // 处理登录后的请求
                doneLogin = new ParallelQueue[8];
                for (int i = 0; i < doneLogin.Length; i++)
                {
                    doneLogin[i] = new ParallelQueue();
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
#if DEBUG
            Stub[]
#else
            StubHttp[]
#endif
            GetStubs(Func<HttpListenerContext> getContext)
        {
            Serializable.RecognitionChildType = false;

            var impl1 = new ImplProtocol1();
            Protocol1Stub _p1 = new Protocol1Stub(impl1);

            var impl2 = new ImplProtocol2();
            Protocol2Stub _p2 = new Protocol2Stub(impl2);
            _p2.__GetAgent = () =>
            {
                impl2.CheckToken(getContext());
                return null;
            };

#if DEBUG
            Stub[]
#else
            StubHttp[]
#endif
                stubs = 
                { 
                    _p1, 
                    _p2 
                };
            return stubs;
        }
        FileService fileService;
        void ICmd.FileService(ushort fileServerPort, string relativePath)
        {
            // 不想配置IIS但需要文件下载服务时使用的文件服务器
            if (fileServerPort != 0)
            {
                if (fileService == null)
                {
                    fileService = new FileService();
                    fileService.Start(fileServerPort);
                }
                else
                {
                    if (fileService.Port != fileServerPort)
                    {
                        fileService.Stop();
                        fileService.Start(fileServerPort);
                    }
                }
                fileService.LocalPath = relativePath;
            }
            else
            {
                if (fileService != null)
                {
                    fileService.Stop();
                    fileService = null;
                }
            }
            _IO.RootDirectory = relativePath;
            _LOG.Info("设置文件系统路径: {0}", Path.GetFullPath(relativePath));
        }
    }
    public class StubBase
    {
        public T_PLAYER Player;

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

            // 检查登录
            Player = _DB._T_PLAYER.Select(null, "WHERE Token=@p0", token);
            if (Player != null)
            {
                Player.LastLoginTime = GameTime.Time.CurrentFrame;
                _DB._T_PLAYER.Update(Player, null, ET_PLAYER.LastLoginTime);
                return;
            }
            throw new InvalidOperationException("没有登录!");
        }
    }
}
