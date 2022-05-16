using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.Network;
using LauncherProtocolStructure;
using EntryEngine;

namespace LauncherManagerProtocol
{
    [ProtocolStub(101, typeof(IManagerCallback), "LauncherServer._Manager")]
    public interface IManager
    {
        /// <summary>开设新服务类型，例如游戏服务器，跨服服务器等</summary>
        void NewServiceType(ServiceType type, Action<bool> callback);
        void ModifyServiceType(string name, ServiceType type, Action<bool> callback);
        void DeleteServiceType(string name);
        void UpdateSVN();
        void GetServices();
        void GetServers();

        /// <summary>
        /// 启用一个服务，例如启用游戏1服，游戏2服
        /// </summary>
        /// <param name="serverID">要启用服务的服务器ID</param>
        /// <param name="serviceType">服务类型</param>
        /// <param name="name">启用服务的名称，例如S1，S2</param>
        void NewService(ushort serverID, string serviceType, string name, string command);
        /// <summary>
        /// 设置服务的启动命令
        /// </summary>
        /// <param name="serviceName">一个平台服务唯一名称</param>
        /// <param name="command">命令和参数间用空格隔开，不同命令间用换行隔开</param>
        void SetServiceLaunchCommand(string serviceName, string command);
        void CallCommand(string serviceName, string command);
        void DeleteService(string serviceName);
        void Launch(string name);
        void Update(string name);
        void Stop(string name);

        void NewManager(Manager manager);
        void DeleteManager(string name);

        void GetServerStatusStatistic(ushort serverID, Action<ServerStatusData> callback);

        void GetLog(string name, DateTime? start, DateTime? end, byte pageCount, int page, string content, string param, byte[] levels);
        void GroupLog(string name, DateTime? start, DateTime? end, string content, string param, byte[] levels);
        void GetLogRepeat(int index);
        void FindContext(int index, DateTime? start, DateTime? end, byte pageCount, string content, string param, byte[] levels);

        void UpdateManager();
    }
    public interface IManagerCallback
    {
        void OnGetServers(Server[] servers);
        void OnGetServiceTypes(ServiceType[] serviceTypes);
        void OnGetServices(ushort serverId, Service[] services);
        void OnServiceUpdate(Service service);
        void OnRevisionUpdate(ushort serverId, ServiceTypeRevision revision);
        void OnStatusUpdate(string serviceName, EServiceStatus status, string time);
        void OnGetManagers(Manager[] managers);
        void OnLoginAgain();
        void OnGetLog(int page, LogRecord[] logs, int pages);
        void OnGetLogRepeat(LogRepeat data);
        void OnLog(string name, Record record);
    }

    public enum ESecurity : byte
    {
        /// <summary>
        /// 程序员，只允许查看服务器日志
        /// </summary>
        Programmer,
        /// <summary>
        /// 运维，可以操作服务器
        /// </summary>
        Maintainer,
        /// <summary>
        /// 管理员，可以管理账号，新建服务类型
        /// </summary>
        Manager,
        /// <summary>
        /// 超级管理员，可以查看统计数据
        /// </summary>
        Administrator,
    }
    public class Manager
    {
        public string Name;
        public string Password;
        public ESecurity Security;
    }
    public class LogRecord
    {
        public Record Record;
        /// <summary>
        /// 相同日志内容的日志数量
        /// </summary>
        public int Count;
    }
    public class LogRepeat
    {
        public byte Level;
        public string Content;
        public LogRepeatData[] Records;
    }
    public class LogRepeatData
    {
        public DateTime Time;
        public string[] Param;
    }


    [ProtocolStub(192, null)]
    public interface IMBS
    {
        /// <summary>连接服务器</summary>
        /// <param name="username">账号</param>
        /// <param name="password">密码</param>
        /// <param name="callback">Token</param>
        void Connect(string username, string password, Action<string> callback);

        /// <summary>开设新服务类型，例如游戏服务器，跨服服务器等</summary>
        void ModifyServiceType(ServiceType type, Action<bool> callback);
        void DeleteServiceType(string name, Action<bool> callback);
        void GetServiceType(Action<List<ServiceType>> callback);

        void GetServers(Action<List<Server>> callback);
        void UpdateServer(Action<bool> callback);

        /// <summary>启用一个服务，例如启用游戏1服，游戏2服</summary>
        /// <param name="serverID">要启用服务的服务器ID</param>
        /// <param name="serviceType">服务类型</param>
        /// <param name="name">启用服务的名称，例如S1，S2</param>
        void NewService(ushort serverID, string serviceType, string name, string exe, string command, Action<bool> callback);
        /// <summary>设置服务的启动命令</summary>
        /// <param name="serviceName">一个平台服务唯一名称</param>
        /// <param name="command">命令和参数间用空格隔开，不同命令间用换行隔开</param>
        void SetServiceLaunchCommand(string[] serviceNames, string exe, string command, Action<bool> callback);
        void CallCommand(string[] serviceNames, string command, Action<bool> callback);
        void DeleteService(string[] serviceNames, Action<bool> callback);
        void LaunchService(string[] serviceNames, Action<bool> callback);
        void UpdateService(string[] serviceNames, Action<bool> callback);
        void StopService(string[] serviceNames, Action<bool> callback);

        void NewManager(Manager manager, Action<bool> callback);
        void DeleteManager(string name, Action<bool> callback);
        void GetManagers(Action<List<Manager>> callback);

        void GetLog(string name, DateTime? start, DateTime? end, byte pageCount, int page, string content, string param, byte[] levels, Action<PagedModel<LogRecord>> callback);
        void GroupLog(string name, DateTime? start, DateTime? end, string content, string param, byte[] levels, Action<PagedModel<LogRecord>> callback);
    }
}
