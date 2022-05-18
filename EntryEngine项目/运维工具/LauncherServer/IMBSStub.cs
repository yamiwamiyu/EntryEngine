using System;
using System.Collections.Generic;
using EntryEngine;
using EntryEngine.Network;
using EntryEngine.Serialize;
using LauncherManagerProtocol;

using System.Linq;
using System.Text;
using System.Net;
interface _IMBS
{
    void Connect(string username, string password, CBIMBS_Connect callback);
    void ModifyServiceType(LauncherProtocolStructure.ServiceType type, CBIMBS_ModifyServiceType callback);
    void DeleteServiceType(string name, CBIMBS_DeleteServiceType callback);
    void GetServiceType(CBIMBS_GetServiceType callback);
    void GetServers(CBIMBS_GetServers callback);
    void UpdateServer(CBIMBS_UpdateServer callback);
    void NewService(ushort serverID, string serviceType, string name, string exe, string command, CBIMBS_NewService callback);
    void SetServiceLaunchCommand(string[] serviceNames, string exe, string command, CBIMBS_SetServiceLaunchCommand callback);
    void GetCommands(string serviceName, CBIMBS_GetCommands callback);
    void CallCommand(string[] serviceNames, string command, CBIMBS_CallCommand callback);
    void DeleteService(string[] serviceNames, CBIMBS_DeleteService callback);
    void LaunchService(string[] serviceNames, CBIMBS_LaunchService callback);
    void UpdateService(string[] serviceNames, CBIMBS_UpdateService callback);
    void StopService(string[] serviceNames, CBIMBS_StopService callback);
    void NewManager(LauncherManagerProtocol.Manager manager, CBIMBS_NewManager callback);
    void DeleteManager(string name, CBIMBS_DeleteManager callback);
    void GetManagers(CBIMBS_GetManagers callback);
    void GetLog(string name, System.DateTime? start, System.DateTime? end, byte pageCount, int page, string content, string param, byte[] levels, CBIMBS_GetLog callback);
    void GroupLog(string name, System.DateTime? start, System.DateTime? end, string content, string param, byte[] levels, CBIMBS_GroupLog callback);
}
public class CBIMBS_Connect : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIMBS_Connect(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(string obj) // INDEX = 0
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIMBS_Connect {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIMBS_Connect Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIMBS_ModifyServiceType : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIMBS_ModifyServiceType(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 1
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIMBS_ModifyServiceType {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIMBS_ModifyServiceType Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIMBS_DeleteServiceType : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIMBS_DeleteServiceType(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 2
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIMBS_DeleteServiceType {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIMBS_DeleteServiceType Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIMBS_GetServiceType : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIMBS_GetServiceType(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(System.Collections.Generic.List<LauncherProtocolStructure.ServiceType> obj) // INDEX = 3
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIMBS_GetServiceType {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIMBS_GetServiceType Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIMBS_GetServers : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIMBS_GetServers(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(System.Collections.Generic.List<LauncherProtocolStructure.Server> obj) // INDEX = 4
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIMBS_GetServers {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIMBS_GetServers Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIMBS_UpdateServer : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIMBS_UpdateServer(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 5
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIMBS_UpdateServer {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIMBS_UpdateServer Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIMBS_NewService : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIMBS_NewService(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 6
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIMBS_NewService {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIMBS_NewService Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIMBS_SetServiceLaunchCommand : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIMBS_SetServiceLaunchCommand(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 7
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIMBS_SetServiceLaunchCommand {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIMBS_SetServiceLaunchCommand Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIMBS_GetCommands : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIMBS_GetCommands(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(System.Collections.Generic.List<string> obj) // INDEX = 8
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIMBS_GetCommands {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIMBS_GetCommands Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIMBS_CallCommand : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIMBS_CallCommand(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 9
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIMBS_CallCommand {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIMBS_CallCommand Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIMBS_DeleteService : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIMBS_DeleteService(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 10
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIMBS_DeleteService {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIMBS_DeleteService Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIMBS_LaunchService : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIMBS_LaunchService(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 11
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIMBS_LaunchService {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIMBS_LaunchService Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIMBS_UpdateService : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIMBS_UpdateService(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 12
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIMBS_UpdateService {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIMBS_UpdateService Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIMBS_StopService : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIMBS_StopService(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 13
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIMBS_StopService {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIMBS_StopService Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIMBS_NewManager : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIMBS_NewManager(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 14
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIMBS_NewManager {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIMBS_NewManager Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIMBS_DeleteManager : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIMBS_DeleteManager(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 15
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIMBS_DeleteManager {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIMBS_DeleteManager Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIMBS_GetManagers : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIMBS_GetManagers(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(System.Collections.Generic.List<LauncherManagerProtocol.Manager> obj) // INDEX = 16
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIMBS_GetManagers {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIMBS_GetManagers Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIMBS_GetLog : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIMBS_GetLog(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(EntryEngine.Network.PagedModel<LauncherManagerProtocol.LogRecord> obj) // INDEX = 17
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIMBS_GetLog {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIMBS_GetLog Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIMBS_GroupLog : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIMBS_GroupLog(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(EntryEngine.Network.PagedModel<LauncherManagerProtocol.LogRecord> obj) // INDEX = 18
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIMBS_GroupLog {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIMBS_GroupLog Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}

class IMBSStub : StubHttp
{
    public Action<HttpListenerContext, object> __AutoCallback;
    public _IMBS __Agent;
    public Func<_IMBS> __GetAgent;
    public Func<HttpListenerContext, _IMBS> __ReadAgent;
    public IMBSStub(_IMBS agent)
    {
        this.__Agent = agent;
        this.Protocol = "192";
        AddMethod("Connect", Connect);
        AddMethod("ModifyServiceType", ModifyServiceType);
        AddMethod("DeleteServiceType", DeleteServiceType);
        AddMethod("GetServiceType", GetServiceType);
        AddMethod("GetServers", GetServers);
        AddMethod("UpdateServer", UpdateServer);
        AddMethod("NewService", NewService);
        AddMethod("SetServiceLaunchCommand", SetServiceLaunchCommand);
        AddMethod("GetCommands", GetCommands);
        AddMethod("CallCommand", CallCommand);
        AddMethod("DeleteService", DeleteService);
        AddMethod("LaunchService", LaunchService);
        AddMethod("UpdateService", UpdateService);
        AddMethod("StopService", StopService);
        AddMethod("NewManager", NewManager);
        AddMethod("DeleteManager", DeleteManager);
        AddMethod("GetManagers", GetManagers);
        AddMethod("GetLog", GetLog);
        AddMethod("GroupLog", GroupLog);
    }
    void Connect(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("username");
        string username = __temp;
        __temp = GetParam("password");
        string password = __temp;
        #if DEBUG
        _LOG.Debug("Connect username: {0}, password: {1},", username, password);
        #endif
        var callback = new CBIMBS_Connect(this);
        agent.Connect(username, password, callback);
    }
    void ModifyServiceType(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("type");
        LauncherProtocolStructure.ServiceType type = string.IsNullOrEmpty(__temp) ? default(LauncherProtocolStructure.ServiceType) : JsonReader.Deserialize<LauncherProtocolStructure.ServiceType>(__temp);
        #if DEBUG
        _LOG.Debug("ModifyServiceType type: {0},", JsonWriter.Serialize(type));
        #endif
        var callback = new CBIMBS_ModifyServiceType(this);
        agent.ModifyServiceType(type, callback);
    }
    void DeleteServiceType(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("name");
        string name = __temp;
        #if DEBUG
        _LOG.Debug("DeleteServiceType name: {0},", name);
        #endif
        var callback = new CBIMBS_DeleteServiceType(this);
        agent.DeleteServiceType(name, callback);
    }
    void GetServiceType(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        #if DEBUG
        _LOG.Debug("GetServiceType");
        #endif
        var callback = new CBIMBS_GetServiceType(this);
        agent.GetServiceType(callback);
    }
    void GetServers(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        #if DEBUG
        _LOG.Debug("GetServers");
        #endif
        var callback = new CBIMBS_GetServers(this);
        agent.GetServers(callback);
    }
    void UpdateServer(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        #if DEBUG
        _LOG.Debug("UpdateServer");
        #endif
        var callback = new CBIMBS_UpdateServer(this);
        agent.UpdateServer(callback);
    }
    void NewService(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("serverID");
        ushort serverID = string.IsNullOrEmpty(__temp) ? default(ushort) : ushort.Parse(__temp);
        __temp = GetParam("serviceType");
        string serviceType = __temp;
        __temp = GetParam("name");
        string name = __temp;
        __temp = GetParam("exe");
        string exe = __temp;
        __temp = GetParam("command");
        string command = __temp;
        #if DEBUG
        _LOG.Debug("NewService serverID: {0}, serviceType: {1}, name: {2}, exe: {3}, command: {4},", serverID, serviceType, name, exe, command);
        #endif
        var callback = new CBIMBS_NewService(this);
        agent.NewService(serverID, serviceType, name, exe, command, callback);
    }
    void SetServiceLaunchCommand(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("serviceNames");
        string[] serviceNames = string.IsNullOrEmpty(__temp) ? default(string[]) : JsonReader.Deserialize<string[]>(__temp);
        __temp = GetParam("exe");
        string exe = __temp;
        __temp = GetParam("command");
        string command = __temp;
        #if DEBUG
        _LOG.Debug("SetServiceLaunchCommand serviceNames: {0}, exe: {1}, command: {2},", JsonWriter.Serialize(serviceNames), exe, command);
        #endif
        var callback = new CBIMBS_SetServiceLaunchCommand(this);
        agent.SetServiceLaunchCommand(serviceNames, exe, command, callback);
    }
    void GetCommands(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("serviceName");
        string serviceName = __temp;
        #if DEBUG
        _LOG.Debug("GetCommands serviceName: {0},", serviceName);
        #endif
        var callback = new CBIMBS_GetCommands(this);
        agent.GetCommands(serviceName, callback);
    }
    void CallCommand(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("serviceNames");
        string[] serviceNames = string.IsNullOrEmpty(__temp) ? default(string[]) : JsonReader.Deserialize<string[]>(__temp);
        __temp = GetParam("command");
        string command = __temp;
        #if DEBUG
        _LOG.Debug("CallCommand serviceNames: {0}, command: {1},", JsonWriter.Serialize(serviceNames), command);
        #endif
        var callback = new CBIMBS_CallCommand(this);
        agent.CallCommand(serviceNames, command, callback);
    }
    void DeleteService(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("serviceNames");
        string[] serviceNames = string.IsNullOrEmpty(__temp) ? default(string[]) : JsonReader.Deserialize<string[]>(__temp);
        #if DEBUG
        _LOG.Debug("DeleteService serviceNames: {0},", JsonWriter.Serialize(serviceNames));
        #endif
        var callback = new CBIMBS_DeleteService(this);
        agent.DeleteService(serviceNames, callback);
    }
    void LaunchService(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("serviceNames");
        string[] serviceNames = string.IsNullOrEmpty(__temp) ? default(string[]) : JsonReader.Deserialize<string[]>(__temp);
        #if DEBUG
        _LOG.Debug("LaunchService serviceNames: {0},", JsonWriter.Serialize(serviceNames));
        #endif
        var callback = new CBIMBS_LaunchService(this);
        agent.LaunchService(serviceNames, callback);
    }
    void UpdateService(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("serviceNames");
        string[] serviceNames = string.IsNullOrEmpty(__temp) ? default(string[]) : JsonReader.Deserialize<string[]>(__temp);
        #if DEBUG
        _LOG.Debug("UpdateService serviceNames: {0},", JsonWriter.Serialize(serviceNames));
        #endif
        var callback = new CBIMBS_UpdateService(this);
        agent.UpdateService(serviceNames, callback);
    }
    void StopService(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("serviceNames");
        string[] serviceNames = string.IsNullOrEmpty(__temp) ? default(string[]) : JsonReader.Deserialize<string[]>(__temp);
        #if DEBUG
        _LOG.Debug("StopService serviceNames: {0},", JsonWriter.Serialize(serviceNames));
        #endif
        var callback = new CBIMBS_StopService(this);
        agent.StopService(serviceNames, callback);
    }
    void NewManager(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("manager");
        LauncherManagerProtocol.Manager manager = string.IsNullOrEmpty(__temp) ? default(LauncherManagerProtocol.Manager) : JsonReader.Deserialize<LauncherManagerProtocol.Manager>(__temp);
        #if DEBUG
        _LOG.Debug("NewManager manager: {0},", JsonWriter.Serialize(manager));
        #endif
        var callback = new CBIMBS_NewManager(this);
        agent.NewManager(manager, callback);
    }
    void DeleteManager(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("name");
        string name = __temp;
        #if DEBUG
        _LOG.Debug("DeleteManager name: {0},", name);
        #endif
        var callback = new CBIMBS_DeleteManager(this);
        agent.DeleteManager(name, callback);
    }
    void GetManagers(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        #if DEBUG
        _LOG.Debug("GetManagers");
        #endif
        var callback = new CBIMBS_GetManagers(this);
        agent.GetManagers(callback);
    }
    void GetLog(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("name");
        string name = __temp;
        __temp = GetParam("start");
        System.DateTime? start = string.IsNullOrEmpty(__temp) ? default(System.DateTime?) : JsonReader.Deserialize<System.DateTime?>(__temp);
        __temp = GetParam("end");
        System.DateTime? end = string.IsNullOrEmpty(__temp) ? default(System.DateTime?) : JsonReader.Deserialize<System.DateTime?>(__temp);
        __temp = GetParam("pageCount");
        byte pageCount = string.IsNullOrEmpty(__temp) ? default(byte) : byte.Parse(__temp);
        __temp = GetParam("page");
        int page = string.IsNullOrEmpty(__temp) ? default(int) : int.Parse(__temp);
        __temp = GetParam("content");
        string content = __temp;
        __temp = GetParam("param");
        string param = __temp;
        __temp = GetParam("levels");
        byte[] levels = string.IsNullOrEmpty(__temp) ? default(byte[]) : JsonReader.Deserialize<byte[]>(__temp);
        #if DEBUG
        _LOG.Debug("GetLog name: {0}, start: {1}, end: {2}, pageCount: {3}, page: {4}, content: {5}, param: {6}, levels: {7},", name, JsonWriter.Serialize(start), JsonWriter.Serialize(end), pageCount, page, content, param, JsonWriter.Serialize(levels));
        #endif
        var callback = new CBIMBS_GetLog(this);
        agent.GetLog(name, start, end, pageCount, page, content, param, levels, callback);
    }
    void GroupLog(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("name");
        string name = __temp;
        __temp = GetParam("start");
        System.DateTime? start = string.IsNullOrEmpty(__temp) ? default(System.DateTime?) : JsonReader.Deserialize<System.DateTime?>(__temp);
        __temp = GetParam("end");
        System.DateTime? end = string.IsNullOrEmpty(__temp) ? default(System.DateTime?) : JsonReader.Deserialize<System.DateTime?>(__temp);
        __temp = GetParam("content");
        string content = __temp;
        __temp = GetParam("param");
        string param = __temp;
        __temp = GetParam("levels");
        byte[] levels = string.IsNullOrEmpty(__temp) ? default(byte[]) : JsonReader.Deserialize<byte[]>(__temp);
        #if DEBUG
        _LOG.Debug("GroupLog name: {0}, start: {1}, end: {2}, content: {3}, param: {4}, levels: {5},", name, JsonWriter.Serialize(start), JsonWriter.Serialize(end), content, param, JsonWriter.Serialize(levels));
        #endif
        var callback = new CBIMBS_GroupLog(this);
        agent.GroupLog(name, start, end, content, param, levels, callback);
    }
}
