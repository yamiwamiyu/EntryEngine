using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine;
using EntryEngine.Network;
using LauncherManagerProtocol;
using EntryEngine.Serialize;
using LauncherProtocolStructure;

namespace LauncherManager
{
    class Maintainer : IManagerAgent, IDisposable
    {
        public static List<Maintainer> Maintainers
        {
            get;
            private set;
        }

        public static Maintainer Connect(string name, string password, string ip, ushort port)
        {
            if (Maintainers == null)
                Maintainers = new List<Maintainer>();

            Maintainer maintainer = new Maintainer();
            maintainer.reconnect = Entry.Instance.SetCoroutine(maintainer.ConnectManager(name, password, ip, port));
            Maintainers.Add(maintainer);

            return maintainer;
        }
        public static Maintainer Find(Server server)
        {
            return Maintainers.FirstOrDefault(m => m.Servers.Contains(server));
        }
        public static Maintainer Find(Service service)
        {
            return Maintainers.FirstOrDefault(m => m.Services.Contains(service));
        }
        public static Maintainer Find(string serviceName)
        {
            return Maintainers.FirstOrDefault(m => m.Services.Any(s => s.Name == serviceName));
        }
        public static Server FindServer(Service service)
        {
            var maintainer = Find(service);
            return maintainer.Servers.FirstOrDefault(s => s.Services.Any(ss => ss.Name == service.Name));
        }


        private ESecurity security;
        private string platform;
        private COROUTINE reconnect;

        public string Name
        {
            get;
            private set;
        }
        public ESecurity Security
        {
            get { return security; }
        }
        public LinkTcp Link
        {
            get;
            private set;
        }
        public Agent Agent
        {
            get;
            private set;
        }
        public IManagerProxy Proxy
        {
            get;
            private set;
        }
        public bool Connected
        {
            get
            {
                return Link != null && Link.IsConnected && Agent != null;
            }
        }
        public bool IsDisposed
        {
            get { return Link == null && Agent == null && Proxy == null; }
        }
        public string Platform
        {
            get { return platform; }
        }
        public Server[] Servers
        {
            get;
            private set;
        }
        public IEnumerable<Service> Services
        {
            get { return Servers.SelectMany(s => s.Services); }
        }
        public ServiceType[] ServiceTypes
        {
            get;
            private set;
        }
        public Manager[] Managers
        {
            get;
            private set;
        }

        private Maintainer()
        {
        }

        IEnumerable<ICoroutine> ConnectManager(string name, string password, string ip, ushort port)
        {
            this.Name = name;
            LinkTcp.MaxBuffer = ushort.MaxValue;
            this.Link = new LinkTcp();
            Link.Heartbeat = TimeSpan.FromMinutes(30);

            while (Link != null)
            {
                if (!Link.IsConnected)
                {
                    Agent = null;
                    Proxy = null;

                    var async = Link.Connect(ip, port);
                    yield return async;

                    if (async.IsFaulted)
                    {
                        _LOG.Error(async.FaultedReason, "Connect Error");
                    }
                    else
                    {
                        string endpoint = Link.EndPoint.ToString();

                        _LOG.Info("成功连接管理服务器{0}", Link.EndPoint);
                        ByteWriter writer = new ByteWriter();
                        writer.Write(name);
                        writer.Write(password);
                        Link.Write(writer.Buffer, 0, writer.Position);
                        Link.Flush();

                        TIME wait = new TIME();
                        while (Link.IsConnected)
                        {
                            byte[] callback = Link.Read();
                            if (callback == null)
                            {
                                wait.Interval += 100;
                                // 100, 200....1000 总计等待6000ms
                                if (wait.Interval > 1000)
                                {
                                    _LOG.Error("未能在时间内收到服务器的响应，关闭本次连接{0}", endpoint);
                                    Link.Close();
                                    break;
                                }
                                yield return wait;
                                continue;
                            }

                            ByteReader reader = new ByteReader(callback);
                            reader.Read(out security);
                            reader.Read(out platform);

                            Proxy = new IManagerProxy(this);
                            Agent = new AgentProtocolStub(Link, Proxy);
                            break;
                        }

                        // 连接被拒，可能用户名密码错误
                        if (!Link.IsConnected)
                        {
                            Logout();
                            break;
                        }
                    }
                }
                // 等待下一次检查与服务器的连接
                yield return new TIME(5000);
            }
        }
        internal void Update()
        {
            if (Connected)
            {
                Link.Flush();
                foreach (var item in Agent.Receive())
                {
                    Agent.OnProtocol(item);
                    if (!Connected)
                        break;
                }
            }
        }
        public void Logout()
        {
            if (Link != null)
            {
                Link.Close();
                Link = null;
            }
            Agent = null;
            Proxy = null;
            if (reconnect != null)
            {
                reconnect.Dispose();
            }
            Maintainers.Remove(this);
        }
        void IDisposable.Dispose()
        {
            Logout();
        }

        protected override void __OnGetServers(Server[] servers)
        {
            this.Servers = servers;
        }
        protected override void __OnGetServiceTypes(ServiceType[] serviceTypes)
        {
            this.ServiceTypes = serviceTypes;
        }
        protected override void __OnGetServices(ushort serverID, Service[] services)
        {
            var server = Servers.FirstOrDefault(s => s.ID == serverID);
            server.Services = services;
        }
        protected override void __OnServiceUpdate(Service service)
        {
            var server = Servers.FirstOrDefault(s => s.Services.Any(ss => ss.Name == service.Name));
            if (server == null)
                return;
            //int index = server.Services.IndexOf(s => s.Name == service.Name);
            //if (index != -1)
            //    server.Services[index] = service;
            var old = server.Services.FirstOrDefault(s => s.Name == service.Name);
            if (old != null)
            {
                old.Revision = service.Revision;
                old.LaunchCommand = service.LaunchCommand;
            }
        }
        protected override void __OnRevisionUpdate(ushort serverId, ServiceTypeRevision revision)
        {
            var server = Servers.FirstOrDefault(s => s.ID == serverId);
            foreach (var item in server.Services)
                if (item.Type == revision.Type)
                    item.RevisionOnServer = revision.Revision;
        }
        protected override void __OnStatusUpdate(string serviceName, EServiceStatus status, string time)
        {
            var service = Services.FirstOrDefault(s => s.Name == serviceName);
            service.Status = status;
            service.LastStatusTime = time;
        }
        protected override void __OnGetManagers(Manager[] managers)
        {
            Managers = managers;
        }
        protected override void __OnLoginAgain()
        {
            Logout();
            if (Maintainers.Count == 0)
                Entry.Instance.ShowMainScene<S登陆菜单>();
            S确认对话框.Hint("平台[{0}]的管理账号[{1}]再其它地方登陆", Platform, Name);
        }
        protected override void __OnLog(string name, Record record)
        {
            if (!string.IsNullOrEmpty(name))
                _LOG.Append("[{0}]", name);
            record.Level %= 4;
            _LOG.Log(ref record);

            SManagerLog.Log(name, record);
        }
    }
}
