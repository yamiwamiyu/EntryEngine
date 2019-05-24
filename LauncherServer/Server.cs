using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine;
using EntryEngine.Network;
using LauncherProtocolStructure;
using LauncherProtocol;
using EntryEngine.Cmdline;

namespace LauncherServer
{
    class SERVER : _ILauncherService, IDisposable
    {
        public ServerStatusData Status;

        public ushort ID
        {
            get { return ServerData.ID; }
        }
        public Link Link
        {
            get;
            private set;
        }
        public ServiceManager Manager
        {
            get;
            private set;
        }
        public IManagerCallProxy Proxy
        {
            get;
            private set;
        }
        public Server ServerData
        {
            get;
            private set;
        }
        public Service[] Services
        {
            get { return ServerData.Services; }
        }

        private SERVER()
        {
        }

        public void Dispose()
        {
            if (Link != null)
                Link.Close();
            servers.Remove(this);
        }

        #region static members

        private static List<SERVER> servers = new List<SERVER>();
        private static Dictionary<string, LogStorage> logs = new Dictionary<string, LogStorage>();

        public static Link Broadcast
        {
            get
            {
                if (servers.Count == 0)
                    return null;
                else
                    return new LinkMultiple(servers.Select(s => s.Link));
            }
        }
        public static IEnumerable<SERVER> Servers
        {
            get { return servers.Enumerable(); }
        }
        public static IEnumerable<LogStorage> Logs
        {
            get { return logs.Values; }
        }

        public static LogStorage GetOrCreateLog(string serviceName)
        {
            LogStorage log;
            if (!logs.TryGetValue(serviceName, out log))
            {
                Service service;
                var server = GetServer(serviceName, out service);
                if (server == null)
                {
                    _LOG.Warning("不能创建服务[{0}]的日志", serviceName);
                    return null;
                }
                log = new LogStorage(service.Directory, service.Name);
                logs.Add(service.Name, log);
            }
            return log;

        }
        public static LogStorage GetLog(string serviceName)
        {
            LogStorage log;
            logs.TryGetValue(serviceName, out log);
            return log;
        }
        public static void RemoveClosedLog()
        {
            Queue<string> queue = new Queue<string>();
            foreach (var item in logs)
                if (item.Value.IsDisposed)
                    queue.Enqueue(item.Key);
            while (queue.Count > 0)
                logs.Remove(queue.Dequeue());
        }
        public static SERVER GetServer(ushort id)
        {
            var server = servers.FirstOrDefault(s => s.ID == id);
            if (server == null)
                throw new ArgumentException("没有服务器ID:" + id);
            return server;
        }
        public static SERVER GetServer(string serviceName, out Service service)
        {
            for (int i = 0; i < servers.Count; i++)
            {
                var services = servers[i].Services;
                if (services == null)
                    continue;
                service = services.FirstOrDefault(s => s.Name == serviceName);
                if (service != null)
                    return servers[i];
            }
            service = null;
            return null;
        }
        public static SERVER CreateServer(Link link, string ip, ServiceManager manager)
        {
            SERVER server = new SERVER();
            server.ServerData = new Server();
            server.ServerData.ID = (ushort)(servers.SelectLast(s => s.ID) + 1);
            server.ServerData.EndPoint = ip;

            server.Link = link;
            server.Manager = manager;
            server.Proxy = new IManagerCallProxy();
            servers.Add(server);

            return server;
        }

        #endregion

        void _ILauncherService.PushServices(Service[] services)
        {
            ServerData.Services = services;
        }
        void _ILauncherService.RevisionUpdate(ServiceTypeRevision revision)
        {
            foreach (var item in Services.Where(s => s.Type == revision.Type))
                item.RevisionOnServer = revision.Revision;
            IManagerCallbackProxy.OnRevisionUpdate(Manager.Link, ID, revision);
        }
        void _ILauncherService.StatusUpdate(string name, EServiceStatus status, string time)
        {
            var service = Services.FirstOrDefault(s => s.Name == name);
            service.Status = status;
            service.LastStatusTime = time;
            IManagerCallbackProxy.OnStatusUpdate(Manager.Link, name, status, time);

            LogStorage log;
            switch (status)
            {
                case EServiceStatus.Stop:
                    if (logs.TryGetValue(name, out log))
                    {
                        log.ClearCache();
                        // 日志大于10M时，清除5天前的日志
                        if (log.LogDataByteCount > 1024 * 1024 * 10)
                        {
                            _LOG.Info("服务[{0}]清理日志", service.Name);
                            var storages = log.ReadAllLog(DateTime.Now - TimeSpan.FromDays(5), null, null, null);
                            log.Renew();
                            foreach (var item in storages)
                                log.Log(ref item.Record);
                            log.Dispose();
                        }
                        else
                            log.Dispose();
                        logs.Remove(name);
                    }
                    break;

                case EServiceStatus.Starting:
                    if (!logs.TryGetValue(name, out log))
                    {
                        log = new LogStorage(service.Directory, service.Name);
                        logs.Add(name, log);
                    }
                    break;
            }
        }
        void _ILauncherService.ServerStatusStatistic(ServerStatusData data)
        {
            Status = data;
            // 做统计表格
        }
        void _ILauncherService.Log(string name, Record record)
        {
            LogStorage log;
            if (logs.TryGetValue(name, out log))
                log.Log(ref record);
        }
        void _ILauncherService.LogServer(string name, Record record)
        {
            IManagerCallbackProxy.OnLog(Manager.Link, name, record);
        }
    }
}
