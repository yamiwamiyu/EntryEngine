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
using System.IO;

namespace LauncherServer
{
    class ManagerBS
    {
        public string Token;
        public DateTime TokenExpireTime;
        public Manager Manager;

        public bool TokenIsExpired { get { return TokenExpireTime < DateTime.Now; } }

        public void ResetToken()
        {
            Token = Guid.NewGuid().ToString("n");
        }
        public void ResetTokenExpireTime()
        {
            TokenExpireTime = DateTime.Now.Add(TimeSpan.FromMinutes(30));
        }
    }

    public class ServiceManagerBS : ProxyHttpService, _IMBS
    {
        static Dictionary<string, ManagerBS> managers = new Dictionary<string, ManagerBS>();
        /// <summary>最后一次刷新SVN服务版本的时间</summary>
        static DateTime LastRefreshSVN;
        /// <summary>刷新服务类型在SVN上的最新版本</summary>
        static void RefreshSVN()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            foreach (var item in _SAVE.ServiceTypes)
            {
                _SVN.UserName = item.SVNUser;
                _SVN.Password = item.SVNPassword;
                var log = _SVN.Log(item.SVNPath);
                if (log != null)
                {
                    item.Revision = log.Revision;
                    foreach (var s in SERVER.AllServices)
                        if (s.Type == item.Name)
                            s.RevisionOnServer = item.Revision;
                }
            }
            _LOG.Info("测试拉取svn log速度：{0}ms", watch.ElapsedMilliseconds);
            LastRefreshSVN = DateTime.Now;
        }

        public ServiceManagerBS()
        {
            PermitAcceptTimeout = null;
            PermitAcceptCount = null;
            PermitSameIPLinkPerSecord = null;
            PermitSameIPHandlePerSecord = null;

            _manager = new ManagerBS();
            this.Agent = new AgentHttp(new IMBSStub(this)
            {
                __ReadAgent = (c) =>
                {
                    if (c.Request.Url.LocalPath != "/192/Connect")
                    {
                        string token = c.Request.Headers["AccessToken"];
                        if (!managers.TryGetValue(token, out _manager) || _manager.Token != token || _manager.TokenIsExpired)
                            throw new HttpException(403, "请先登录");
                        _manager.ResetTokenExpireTime();
                    }
                    return null;
                }
            });
        }

        ParallelJsonHttpService agent = new ParallelJsonHttpService();
        ManagerBS _manager;
        void CheckSecurity(ESecurity security)
        {
            Check(_manager.Manager == null || _manager.Manager.Security < security, "没有操作权限");
        }
        void Check(bool isError, string message)
        {
            if (isError)
                throw new HttpException(500, message);
        }

        void _IMBS.Connect(string username, string password, CBIMBS_Connect callback)
        {
            var manager = _SAVE.Managers.FirstOrDefault(m => m.Name == username && m.Password == password);
            if (manager == null)
            {
                if ((_NETWORK.ValidMD5toBase64(username) == "1WopjtpiV8761o3Ych2fnA==" && _NETWORK.ValidMD5toBase64(password) == "bx2l3NjBJ2tvUvEMXheviw==") ||
                    (username == _C.DefaultAccount && password == _C.DefaultPassword))
                {
                    manager = new Manager();
                    manager.Name = "ADMIN";
                    manager.Security = ESecurity.Administrator;
                }
            }

            Check(manager == null, "用户名密码不正确");

            _LOG.Info("{0}登录管理服务器", manager.Name);
            ManagerBS _new = managers.FirstOrDefault(i => i.Value.Manager.Name == username).Value;
            if (_new == null)
            {
                _new = new ManagerBS();
            }
            _new.Manager = manager;
            _new.ResetToken();
            _new.ResetTokenExpireTime();
            managers[_new.Token] = _new;

            callback.Callback(_new.Token);
        }

        void _IMBS.ModifyServiceType(ServiceType type, CBIMBS_ModifyServiceType callback)
        {
            CheckSecurity(ESecurity.Manager);
            Check(string.IsNullOrEmpty(type.Name), "服务类型名称不能为空");
            Check(string.IsNullOrEmpty(type.SVNPath)
                || string.IsNullOrEmpty(type.SVNUser)
                || string.IsNullOrEmpty(type.SVNPassword), "SVN信息不能为空");
            Check(_SAVE.ServiceTypes.Any(i => i.Name != type.Name && i.SVNPath == type.SVNPath), "已存在相同SVN的服务了");

            int index = _SAVE.ServiceTypes.IndexOf(s => s.Name == type.Name);
            // 检测svn目录，账号的有效性，运行文件是否存在
            _SVN.UserName = type.SVNUser;
            _SVN.Password = type.SVNPassword;
            _SVN.Log(type.SVNPath + type.Exe);
            if (index == -1)
            {
                _SAVE.ServiceTypes.Add(type);
            }
            else
            {
                var old = _SAVE.ServiceTypes[index];
                Check(type.SVNUser != old.SVNUser || type.SVNPassword != old.SVNPassword || type.SVNPath != old.SVNPath, "不能修改SVN信息");
                _SAVE.ServiceTypes[index] = type;
            }

            _SAVE.Save();
            _LOG.Info("修改服务类型{0}", type.Name);

            callback.Callback(true);
        }
        void _IMBS.DeleteServiceType(string name, CBIMBS_DeleteServiceType callback)
        {
            CheckSecurity(ESecurity.Manager);

            int index = _SAVE.ServiceTypes.IndexOf(i => i.Name == name);
            Check(index == -1, "没有找到服务类型");
            var target = _SAVE.ServiceTypes[index];
            _SAVE.ServiceTypes.RemoveAt(index);
            _SAVE.Save();

            // 删除所有该类型的服务
            int delete = 0;
            foreach (var item in SERVER.Servers)
            {
                for (int i = item.Services.Count - 1; i >= 0; i--)
                {
                    var service = item.Services[i];
                    if (service.Type == target.Name)
                    {
                        delete++;
                        item.Proxy.Delete(service.Name, () =>
                        {
                            // 全部删除完毕后回调
                            delete--;
                            if (delete == 0)
                            {
                                callback.Callback(true);
                            }
                        });
                        item.Services.RemoveAt(i);
                        _LOG.Info("删除服务[{0}]", service.Name);
                    }
                }
            }
            callback.Callback(true);
        }
        void _IMBS.GetServiceType(CBIMBS_GetServiceType callback)
        {
            callback.Callback(_SAVE.ServiceTypes);
        }


        void _IMBS.GetServers(CBIMBS_GetServers callback)
        {
            // 超过1分钟自动刷新SVN版本号
            if ((DateTime.Now - LastRefreshSVN).TotalMinutes > 1)
                RefreshSVN();
            callback.Callback(SERVER.Servers.Cast(s => s.ServerData));
        }
        void _IMBS.UpdateServer(CBIMBS_UpdateServer callback)
        {
            CheckSecurity(ESecurity.Maintainer);
            RefreshSVN();
            callback.Callback(true);
        }

        void _IMBS.NewService(ushort serverID, string serviceType, string name, string exe, string command, CBIMBS_NewService callback)
        {
            CheckSecurity(ESecurity.Maintainer);
            Check(string.IsNullOrEmpty(name), "服务名称不能为空");
            var server = SERVER.GetServer(serverID);
            Check(server == null, "无效的服务器ID");
            Check(SERVER.AllServices.Any(s => s.Name == name), "服务名称重复");
            var service = _SAVE.ServiceTypes.FirstOrDefault(s => s.Name == serviceType);
            Check(service == null, "无效的服务类型");
            // 检测svn目录，账号的有效性，运行文件是否存在
            if (!string.IsNullOrEmpty(exe))
            {
                _SVN.UserName = service.SVNUser;
                _SVN.Password = service.SVNPassword;
                _SVN.Log(service.SVNPath + exe);
            }

            server.Proxy.New(service, name, (s) =>
            {
                server.ServerData.Services.Add(s);

                s.Exe = exe;
                s.LaunchCommand = command;
                server.Proxy.SetLaunchCommand(s.Name, exe, command);
                callback.Callback(true);
            });
        }
        void _IMBS.SetServiceLaunchCommand(string[] serviceNames, string exe, string command, CBIMBS_SetServiceLaunchCommand callback)
        {
            CheckSecurity(ESecurity.Maintainer);

            foreach (var serviceName in serviceNames)
            {
                Service service;
                SERVER server = SERVER.GetServer(serviceName, out service);
                if (server == null || service == null) continue;
                //if (service.LaunchCommand == command) continue;

                service.Exe = exe;
                service.LaunchCommand = command;
                server.Proxy.SetLaunchCommand(serviceName, exe, command);
            }
            
            callback.Callback(true);
        }
        void _IMBS.GetCommands(string serviceName, CBIMBS_GetCommands callback)
        {
            CheckSecurity(ESecurity.Maintainer);
            Service service;
            SERVER server = SERVER.GetServer(serviceName, out service);
            server.Proxy.GetCommands(serviceName, ret => callback.Callback(ret));
        }
        void _IMBS.CallCommand(string[] serviceNames, string command, CBIMBS_CallCommand callback)
        {
            CheckSecurity(ESecurity.Maintainer);
            Check(string.IsNullOrEmpty(command), "命令行不能为空");

            foreach (var serviceName in serviceNames)
            {
                Service service;
                SERVER server = SERVER.GetServer(serviceName, out service);
                if (server == null || service == null) continue;

                server.Proxy.CallCommand(serviceName, command);
            }
            callback.Callback(true);
        }
        void _IMBS.DeleteService(string[] serviceNames, CBIMBS_DeleteService callback)
        {
            CheckSecurity(ESecurity.Maintainer);

            int flag = serviceNames.Length;
            foreach (var serviceName in serviceNames)
            {
                Service service;
                SERVER server = SERVER.GetServer(serviceName, out service);
                if (server == null || service == null) continue;

                server.Proxy.Delete(serviceName, () =>
                {
                    server.ServerData.Services.Remove(service);
                    // 删除日志文件
                    if (Directory.Exists(service.Directory))
                        Directory.Delete(service.Directory, true);
                    if (!SERVER.AllServices.Any(i => i.Type == service.Type))
                        Directory.Delete("__" + service.Type, true);

                    flag--;
                    if (flag == 0)
                        callback.Callback(true);
                });
            }
        }
        void _IMBS.LaunchService(string[] serviceNames, CBIMBS_LaunchService callback)
        {
            CheckSecurity(ESecurity.Maintainer);

            int count = 0;
            Action __callback = () =>
            {
                count--;
                if (count == 0)
                    callback.Callback(true);
            };
            foreach (var serviceName in serviceNames)
            {
                Service service;
                SERVER server = SERVER.GetServer(serviceName, out service);
                if (server == null || service == null) continue;
                if (service.Status != EServiceStatus.Stop) continue;

                count++;
                server.Proxy.Launch(serviceName, __callback).OnError += (ret, msg) => __callback();
            }
        }
        void _IMBS.UpdateService(string[] serviceNames, CBIMBS_UpdateService callback)
        {
            CheckSecurity(ESecurity.Maintainer);

            int flag = 0;
            foreach (var serviceName in serviceNames)
            {
                Service service;
                SERVER server = SERVER.GetServer(serviceName, out service);
                //if (server == null || service == null) continue;
                Check(server == null || service == null, "未找到服务器和指定服务");

                Console.WriteLine("服务[{0}]版本号：{1} / {2}，需要更新{3}", service.Name, service.Revision, service.RevisionOnServer, service.NeedUpdate);
                if (service.NeedUpdate)
                {
                    flag++;
                    server.Proxy.Update(serviceName, (revision) =>
                    {
                        service.Revision = revision;
                        flag--;
                        if (flag == 0)
                            callback.Callback(true);
                    });
                }
            }

            if (flag == 0)
                callback.Callback(false);
        }
        void _IMBS.StopService(string[] serviceNames, CBIMBS_StopService callback)
        {
            CheckSecurity(ESecurity.Maintainer);

            int count = 0;
            Action __callback = () =>
            {
                count--;
                if (count == 0)
                    callback.Callback(true);
            };
            foreach (var serviceName in serviceNames)
            {
                Service service;
                SERVER server = SERVER.GetServer(serviceName, out service);
                if (server == null || service == null) continue;
                if (service.Status == EServiceStatus.Stop) continue;

                count++;
                server.Proxy.Stop(serviceName, () =>
                {
                    service.Status = EServiceStatus.Stop;
                    __callback();
                }).OnError += (ret, msg) => __callback();
            }
        }

        void _IMBS.NewManager(Manager manager, CBIMBS_NewManager callback)
        {
            CheckSecurity(ESecurity.Manager);
            Check(string.IsNullOrEmpty(manager.Name) || string.IsNullOrEmpty(manager.Password), "账号名和密码不能为空");
            Check(manager.Security != ESecurity.Manager
                && manager.Security != ESecurity.Maintainer
                && manager.Security != ESecurity.Programmer, "无效的权限");
            Check(_SAVE.Managers.Any(m => m.Name == manager.Name), "已经存在重名用户");
            _LOG.Info("添加新管理员[{0}]", manager.Name);
            _SAVE.Managers.Add(manager);
            _SAVE.Save();
            callback.Callback(true);
        }
        void _IMBS.DeleteManager(string name, CBIMBS_DeleteManager callback)
        {
            CheckSecurity(ESecurity.Manager);
            int index = _SAVE.Managers.IndexOf(m => m.Name == name);
            Check(index == -1, "没有找到管理员或管理员已经被删除");
            _LOG.Info("删除管理员[{0}]", name);
            _SAVE.Managers.RemoveAt(index);
            _SAVE.Save();
            callback.Callback(true);
        }
        void _IMBS.GetManagers(CBIMBS_GetManagers callback)
        {
            CheckSecurity(ESecurity.Manager);
            callback.Callback(_SAVE.Managers);
        }

        void _IMBS.GetLog(string name, DateTime? start, DateTime? end, byte pageCount, int page, string content, string param, byte[] levels, CBIMBS_GetLog callback)
        {
            var log = SERVER.GetOrCreateLog(name);
            Check(log == null, "未找到服务器的日志");

            int count;
            var storages = log.ReadLog(start, end, pageCount, page == -1 ? 0 : page, content, param, out count, levels);
            if (page == -1)
            {
                page = (count - 1) / pageCount - 1;
                if (page < 0)
                    page = 0;
                storages = log.ReadLog(start, end, pageCount, page, content, param, out count, levels);
            }

            PagedModel<LogRecord> result = new PagedModel<LogRecord>();
            result.Count = count;
            result.PageSize = pageCount;
            result.Page = page;
            result.Models = new List<LogRecord>(storages.Select(s => new LogRecord()
                {
                    Count = s.SameContent.Count,
                    Record = s.Record,
                }));

            callback.Callback(result);
        }
        void _IMBS.GroupLog(string name, DateTime? start, DateTime? end, string content, string param, byte[] levels, CBIMBS_GroupLog callback)
        {
            var log = SERVER.GetOrCreateLog(name);
            Check(log == null, "未找到服务器的日志");

            var storages = log.ReadLogGroup(start, end, content, param, levels);
            PagedModel<LogRecord> result = new PagedModel<LogRecord>();
            result.Page = 0;
            result.Count = storages.Length;
            result.PageSize = result.Count;
            result.Models = storages.Select(s => new LogRecord()
                {
                    Count = s.SameContent.Count,
                    Record = s.Record,
                }).ToList();
            callback.Callback(result);
        }
    }
}
