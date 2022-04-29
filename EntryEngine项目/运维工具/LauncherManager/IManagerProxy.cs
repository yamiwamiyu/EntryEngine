using System;
using System.Collections.Generic;
using EntryEngine;
using EntryEngine.Network;
using EntryEngine.Serialize;
using LauncherManagerProtocol;

class IManagerProxy : StubClientAsync
{
    public LauncherManagerProtocol.IManagerCallback __Agent;
    public Func<LauncherManagerProtocol.IManagerCallback> __GetAgent;
    public Func<ByteReader, LauncherManagerProtocol.IManagerCallback> __ReadAgent;
    public Action<ByteWriter> __WriteAgent;
    
    public IManagerProxy()
    {
        this.Protocol = 101;
        AddMethod("NewServiceType", NewServiceType_0);
        AddMethod("ModifyServiceType", ModifyServiceType_1);
        AddMethod("OnGetServers", OnGetServers_2);
        AddMethod("OnGetServiceTypes", OnGetServiceTypes_3);
        AddMethod("OnGetServices", OnGetServices_4);
        AddMethod("OnServiceUpdate", OnServiceUpdate_5);
        AddMethod("OnRevisionUpdate", OnRevisionUpdate_6);
        AddMethod("OnStatusUpdate", OnStatusUpdate_7);
        AddMethod("OnGetManagers", OnGetManagers_8);
        AddMethod("OnLoginAgain", OnLoginAgain_9);
        AddMethod("OnGetLog", OnGetLog_10);
        AddMethod("OnGetLogRepeat", OnGetLogRepeat_11);
        AddMethod("OnLog", OnLog_12);
        AddMethod("GetServerStatusStatistic", GetServerStatusStatistic_15);
    }
    public IManagerProxy(LauncherManagerProtocol.IManagerCallback agent) : this()
    {
        this.__Agent = agent;
    }
    public IManagerProxy(Func<LauncherManagerProtocol.IManagerCallback> agent) : this()
    {
        this.__GetAgent = agent;
    }
    public IManagerProxy(Func<ByteReader, LauncherManagerProtocol.IManagerCallback> agent) : this()
    {
        this.__ReadAgent = agent;
    }
    
    public StubClientAsync.AsyncWaitCallback NewServiceType(LauncherProtocolStructure.ServiceType type, System.Action<bool> callback)
    {
        if (Link == null || !Link.IsConnected) return null;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)101);
        __writer.Write("NewServiceType");
        __writer.Write(type);
        var __async = Push(callback);
        if (__async == null) return null;
        __writer.Write(__async.ID);
        #if DEBUG
        _LOG.Debug("NewServiceType({0} bytes) type: {1}, callback: {2}", __writer.Position, JsonWriter.Serialize(type), "System.Action<bool>");
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
        return __async;
    }
    public StubClientAsync.AsyncWaitCallback ModifyServiceType(string name, LauncherProtocolStructure.ServiceType type, System.Action<bool> callback)
    {
        if (Link == null || !Link.IsConnected) return null;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)101);
        __writer.Write("ModifyServiceType");
        __writer.Write(name);
        __writer.Write(type);
        var __async = Push(callback);
        if (__async == null) return null;
        __writer.Write(__async.ID);
        #if DEBUG
        _LOG.Debug("ModifyServiceType({0} bytes) name: {1}, type: {2}, callback: {3}", __writer.Position, name, JsonWriter.Serialize(type), "System.Action<bool>");
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
        return __async;
    }
    public void DeleteServiceType(string name)
    {
        if (Link == null || !Link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)101);
        __writer.Write("DeleteServiceType");
        __writer.Write(name);
        #if DEBUG
        _LOG.Debug("DeleteServiceType({0} bytes) name: {1}", __writer.Position, name);
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void UpdateSVN()
    {
        if (Link == null || !Link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)101);
        __writer.Write("UpdateSVN");
        #if DEBUG
        _LOG.Debug("UpdateSVN({0} bytes)", __writer.Position);
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void GetServices()
    {
        if (Link == null || !Link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)101);
        __writer.Write("GetServices");
        #if DEBUG
        _LOG.Debug("GetServices({0} bytes)", __writer.Position);
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void GetServers()
    {
        if (Link == null || !Link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)101);
        __writer.Write("GetServers");
        #if DEBUG
        _LOG.Debug("GetServers({0} bytes)", __writer.Position);
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void NewService(ushort serverID, string serviceType, string name, string command)
    {
        if (Link == null || !Link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)101);
        __writer.Write("NewService");
        __writer.Write(serverID);
        __writer.Write(serviceType);
        __writer.Write(name);
        __writer.Write(command);
        #if DEBUG
        _LOG.Debug("NewService({0} bytes) serverID: {1}, serviceType: {2}, name: {3}, command: {4}", __writer.Position, serverID, serviceType, name, command);
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void SetServiceLaunchCommand(string serviceName, string command)
    {
        if (Link == null || !Link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)101);
        __writer.Write("SetServiceLaunchCommand");
        __writer.Write(serviceName);
        __writer.Write(command);
        #if DEBUG
        _LOG.Debug("SetServiceLaunchCommand({0} bytes) serviceName: {1}, command: {2}", __writer.Position, serviceName, command);
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void CallCommand(string serviceName, string command)
    {
        if (Link == null || !Link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)101);
        __writer.Write("CallCommand");
        __writer.Write(serviceName);
        __writer.Write(command);
        #if DEBUG
        _LOG.Debug("CallCommand({0} bytes) serviceName: {1}, command: {2}", __writer.Position, serviceName, command);
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void DeleteService(string serviceName)
    {
        if (Link == null || !Link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)101);
        __writer.Write("DeleteService");
        __writer.Write(serviceName);
        #if DEBUG
        _LOG.Debug("DeleteService({0} bytes) serviceName: {1}", __writer.Position, serviceName);
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void Launch(string name)
    {
        if (Link == null || !Link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)101);
        __writer.Write("Launch");
        __writer.Write(name);
        #if DEBUG
        _LOG.Debug("Launch({0} bytes) name: {1}", __writer.Position, name);
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void Update(string name)
    {
        if (Link == null || !Link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)101);
        __writer.Write("Update");
        __writer.Write(name);
        #if DEBUG
        _LOG.Debug("Update({0} bytes) name: {1}", __writer.Position, name);
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void Stop(string name)
    {
        if (Link == null || !Link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)101);
        __writer.Write("Stop");
        __writer.Write(name);
        #if DEBUG
        _LOG.Debug("Stop({0} bytes) name: {1}", __writer.Position, name);
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void NewManager(LauncherManagerProtocol.Manager manager)
    {
        if (Link == null || !Link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)101);
        __writer.Write("NewManager");
        __writer.Write(manager);
        #if DEBUG
        _LOG.Debug("NewManager({0} bytes) manager: {1}", __writer.Position, JsonWriter.Serialize(manager));
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void DeleteManager(string name)
    {
        if (Link == null || !Link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)101);
        __writer.Write("DeleteManager");
        __writer.Write(name);
        #if DEBUG
        _LOG.Debug("DeleteManager({0} bytes) name: {1}", __writer.Position, name);
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public StubClientAsync.AsyncWaitCallback GetServerStatusStatistic(ushort serverID, System.Action<LauncherProtocolStructure.ServerStatusData> callback)
    {
        if (Link == null || !Link.IsConnected) return null;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)101);
        __writer.Write("GetServerStatusStatistic");
        __writer.Write(serverID);
        var __async = Push(callback);
        if (__async == null) return null;
        __writer.Write(__async.ID);
        #if DEBUG
        _LOG.Debug("GetServerStatusStatistic({0} bytes) serverID: {1}, callback: {2}", __writer.Position, serverID, "System.Action<LauncherProtocolStructure.ServerStatusData>");
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
        return __async;
    }
    public void GetLog(string name, System.DateTime? start, System.DateTime? end, byte pageCount, int page, string content, string param, byte[] levels)
    {
        if (Link == null || !Link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)101);
        __writer.Write("GetLog");
        __writer.Write(name);
        __writer.Write(start);
        __writer.Write(end);
        __writer.Write(pageCount);
        __writer.Write(page);
        __writer.Write(content);
        __writer.Write(param);
        __writer.Write(levels);
        #if DEBUG
        _LOG.Debug("GetLog({0} bytes) name: {1}, start: {2}, end: {3}, pageCount: {4}, page: {5}, content: {6}, param: {7}, levels: {8}", __writer.Position, name, JsonWriter.Serialize(start), JsonWriter.Serialize(end), pageCount, page, content, param, JsonWriter.Serialize(levels));
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void GroupLog(string name, System.DateTime? start, System.DateTime? end, string content, string param, byte[] levels)
    {
        if (Link == null || !Link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)101);
        __writer.Write("GroupLog");
        __writer.Write(name);
        __writer.Write(start);
        __writer.Write(end);
        __writer.Write(content);
        __writer.Write(param);
        __writer.Write(levels);
        #if DEBUG
        _LOG.Debug("GroupLog({0} bytes) name: {1}, start: {2}, end: {3}, content: {4}, param: {5}, levels: {6}", __writer.Position, name, JsonWriter.Serialize(start), JsonWriter.Serialize(end), content, param, JsonWriter.Serialize(levels));
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void GetLogRepeat(int index)
    {
        if (Link == null || !Link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)101);
        __writer.Write("GetLogRepeat");
        __writer.Write(index);
        #if DEBUG
        _LOG.Debug("GetLogRepeat({0} bytes) index: {1}", __writer.Position, index);
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void FindContext(int index, System.DateTime? start, System.DateTime? end, byte pageCount, string content, string param, byte[] levels)
    {
        if (Link == null || !Link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)101);
        __writer.Write("FindContext");
        __writer.Write(index);
        __writer.Write(start);
        __writer.Write(end);
        __writer.Write(pageCount);
        __writer.Write(content);
        __writer.Write(param);
        __writer.Write(levels);
        #if DEBUG
        _LOG.Debug("FindContext({0} bytes) index: {1}, start: {2}, end: {3}, pageCount: {4}, content: {5}, param: {6}, levels: {7}", __writer.Position, index, JsonWriter.Serialize(start), JsonWriter.Serialize(end), pageCount, content, param, JsonWriter.Serialize(levels));
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void UpdateManager()
    {
        if (Link == null || !Link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)101);
        __writer.Write("UpdateManager");
        #if DEBUG
        _LOG.Debug("UpdateManager({0} bytes)", __writer.Position);
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    
    void NewServiceType_0(ByteReader __stream)
    {
        byte __id;
        sbyte __ret;
        __stream.Read(out __id);
        __stream.Read(out __ret);
        var __callback = Pop(__id);
        if (__ret == 0)
        {
            bool obj;
            __stream.Read(out obj);
            #if DEBUG
            _LOG.Debug("NewServiceType obj: {0}", JsonWriter.Serialize(obj));
            #endif
            var __invoke = (System.Action<bool>)__callback.Function;
            if (__invoke != null) __invoke(obj);
        }
        else
        {
            string __msg;
            __stream.Read(out __msg);
            _LOG.Error("NewServiceType_0 error! id={0} ret={1} msg={2}", __id, __ret, __msg);
            Error(__callback, 0, __ret, __msg);
        }
    }
    void ModifyServiceType_1(ByteReader __stream)
    {
        byte __id;
        sbyte __ret;
        __stream.Read(out __id);
        __stream.Read(out __ret);
        var __callback = Pop(__id);
        if (__ret == 0)
        {
            bool obj;
            __stream.Read(out obj);
            #if DEBUG
            _LOG.Debug("ModifyServiceType obj: {0}", JsonWriter.Serialize(obj));
            #endif
            var __invoke = (System.Action<bool>)__callback.Function;
            if (__invoke != null) __invoke(obj);
        }
        else
        {
            string __msg;
            __stream.Read(out __msg);
            _LOG.Error("ModifyServiceType_1 error! id={0} ret={1} msg={2}", __id, __ret, __msg);
            Error(__callback, 1, __ret, __msg);
        }
    }
    void OnGetServers_2(ByteReader __stream)
    {
        var __callback = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) __callback = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) __callback = temp; }
        LauncherProtocolStructure.Server[] servers;
        __stream.Read(out servers);
        #if DEBUG
        _LOG.Debug("OnGetServers servers: {0}", JsonWriter.Serialize(servers));
        #endif
        __callback.OnGetServers(servers);
    }
    void OnGetServiceTypes_3(ByteReader __stream)
    {
        var __callback = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) __callback = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) __callback = temp; }
        LauncherProtocolStructure.ServiceType[] serviceTypes;
        __stream.Read(out serviceTypes);
        #if DEBUG
        _LOG.Debug("OnGetServiceTypes serviceTypes: {0}", JsonWriter.Serialize(serviceTypes));
        #endif
        __callback.OnGetServiceTypes(serviceTypes);
    }
    void OnGetServices_4(ByteReader __stream)
    {
        var __callback = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) __callback = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) __callback = temp; }
        ushort serverId;
        LauncherProtocolStructure.Service[] services;
        __stream.Read(out serverId);
        __stream.Read(out services);
        #if DEBUG
        _LOG.Debug("OnGetServices serverId: {0}, services: {1}", JsonWriter.Serialize(serverId), JsonWriter.Serialize(services));
        #endif
        __callback.OnGetServices(serverId, services);
    }
    void OnServiceUpdate_5(ByteReader __stream)
    {
        var __callback = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) __callback = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) __callback = temp; }
        LauncherProtocolStructure.Service service;
        __stream.Read(out service);
        #if DEBUG
        _LOG.Debug("OnServiceUpdate service: {0}", JsonWriter.Serialize(service));
        #endif
        __callback.OnServiceUpdate(service);
    }
    void OnRevisionUpdate_6(ByteReader __stream)
    {
        var __callback = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) __callback = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) __callback = temp; }
        ushort serverId;
        LauncherProtocolStructure.ServiceTypeRevision revision;
        __stream.Read(out serverId);
        __stream.Read(out revision);
        #if DEBUG
        _LOG.Debug("OnRevisionUpdate serverId: {0}, revision: {1}", JsonWriter.Serialize(serverId), JsonWriter.Serialize(revision));
        #endif
        __callback.OnRevisionUpdate(serverId, revision);
    }
    void OnStatusUpdate_7(ByteReader __stream)
    {
        var __callback = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) __callback = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) __callback = temp; }
        string serviceName;
        LauncherProtocolStructure.EServiceStatus status;
        string time;
        __stream.Read(out serviceName);
        __stream.Read(out status);
        __stream.Read(out time);
        #if DEBUG
        _LOG.Debug("OnStatusUpdate serviceName: {0}, status: {1}, time: {2}", JsonWriter.Serialize(serviceName), JsonWriter.Serialize(status), JsonWriter.Serialize(time));
        #endif
        __callback.OnStatusUpdate(serviceName, status, time);
    }
    void OnGetManagers_8(ByteReader __stream)
    {
        var __callback = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) __callback = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) __callback = temp; }
        LauncherManagerProtocol.Manager[] managers;
        __stream.Read(out managers);
        #if DEBUG
        _LOG.Debug("OnGetManagers managers: {0}", JsonWriter.Serialize(managers));
        #endif
        __callback.OnGetManagers(managers);
    }
    void OnLoginAgain_9(ByteReader __stream)
    {
        var __callback = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) __callback = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) __callback = temp; }
        #if DEBUG
        _LOG.Debug("OnLoginAgain");
        #endif
        __callback.OnLoginAgain();
    }
    void OnGetLog_10(ByteReader __stream)
    {
        var __callback = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) __callback = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) __callback = temp; }
        int page;
        LauncherManagerProtocol.LogRecord[] logs;
        int pages;
        __stream.Read(out page);
        __stream.Read(out logs);
        __stream.Read(out pages);
        #if DEBUG
        _LOG.Debug("OnGetLog page: {0}, logs: {1}, pages: {2}", JsonWriter.Serialize(page), JsonWriter.Serialize(logs), JsonWriter.Serialize(pages));
        #endif
        __callback.OnGetLog(page, logs, pages);
    }
    void OnGetLogRepeat_11(ByteReader __stream)
    {
        var __callback = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) __callback = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) __callback = temp; }
        LauncherManagerProtocol.LogRepeat data;
        __stream.Read(out data);
        #if DEBUG
        _LOG.Debug("OnGetLogRepeat data: {0}", JsonWriter.Serialize(data));
        #endif
        __callback.OnGetLogRepeat(data);
    }
    void OnLog_12(ByteReader __stream)
    {
        var __callback = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) __callback = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) __callback = temp; }
        string name;
        EntryEngine.Record record;
        __stream.Read(out name);
        __stream.Read(out record);
        #if DEBUG
        _LOG.Debug("OnLog name: {0}, record: {1}", JsonWriter.Serialize(name), JsonWriter.Serialize(record));
        #endif
        __callback.OnLog(name, record);
    }
    void GetServerStatusStatistic_15(ByteReader __stream)
    {
        byte __id;
        sbyte __ret;
        __stream.Read(out __id);
        __stream.Read(out __ret);
        var __callback = Pop(__id);
        if (__ret == 0)
        {
            LauncherProtocolStructure.ServerStatusData obj;
            __stream.Read(out obj);
            #if DEBUG
            _LOG.Debug("GetServerStatusStatistic obj: {0}", JsonWriter.Serialize(obj));
            #endif
            var __invoke = (System.Action<LauncherProtocolStructure.ServerStatusData>)__callback.Function;
            if (__invoke != null) __invoke(obj);
        }
        else
        {
            string __msg;
            __stream.Read(out __msg);
            _LOG.Error("GetServerStatusStatistic_15 error! id={0} ret={1} msg={2}", __id, __ret, __msg);
            Error(__callback, 15, __ret, __msg);
        }
    }
}

class IManagerAgent : LauncherManagerProtocol.IManagerCallback
{
    public static class _OnGetServers
    {
        public static Action GlobalCallback;
        public static Action Callback;
        public static LauncherProtocolStructure.Server[] servers;
    }
    public static class _OnGetServiceTypes
    {
        public static Action GlobalCallback;
        public static Action Callback;
        public static LauncherProtocolStructure.ServiceType[] serviceTypes;
    }
    public static class _OnGetServices
    {
        public static Action GlobalCallback;
        public static Action Callback;
        public static ushort serverId;
        public static LauncherProtocolStructure.Service[] services;
    }
    public static class _OnServiceUpdate
    {
        public static Action GlobalCallback;
        public static Action Callback;
        public static LauncherProtocolStructure.Service service;
    }
    public static class _OnRevisionUpdate
    {
        public static Action GlobalCallback;
        public static Action Callback;
        public static ushort serverId;
        public static LauncherProtocolStructure.ServiceTypeRevision revision;
    }
    public static class _OnStatusUpdate
    {
        public static Action GlobalCallback;
        public static Action Callback;
        public static string serviceName;
        public static LauncherProtocolStructure.EServiceStatus status;
        public static string time;
    }
    public static class _OnGetManagers
    {
        public static Action GlobalCallback;
        public static Action Callback;
        public static LauncherManagerProtocol.Manager[] managers;
    }
    public static class _OnLoginAgain
    {
        public static Action GlobalCallback;
        public static Action Callback;
    }
    public static class _OnGetLog
    {
        public static Action GlobalCallback;
        public static Action Callback;
        public static int page;
        public static LauncherManagerProtocol.LogRecord[] logs;
        public static int pages;
    }
    public static class _OnGetLogRepeat
    {
        public static Action GlobalCallback;
        public static Action Callback;
        public static LauncherManagerProtocol.LogRepeat data;
    }
    public static class _OnLog
    {
        public static Action GlobalCallback;
        public static Action Callback;
        public static string name;
        public static EntryEngine.Record record;
    }
    
    void LauncherManagerProtocol.IManagerCallback.OnGetServers(LauncherProtocolStructure.Server[] servers)
    {
        _OnGetServers.servers = servers;
        __OnGetServers(servers);
        if (_OnGetServers.GlobalCallback != null) _OnGetServers.GlobalCallback();
        if (_OnGetServers.Callback != null) _OnGetServers.Callback();
    }
    void LauncherManagerProtocol.IManagerCallback.OnGetServiceTypes(LauncherProtocolStructure.ServiceType[] serviceTypes)
    {
        _OnGetServiceTypes.serviceTypes = serviceTypes;
        __OnGetServiceTypes(serviceTypes);
        if (_OnGetServiceTypes.GlobalCallback != null) _OnGetServiceTypes.GlobalCallback();
        if (_OnGetServiceTypes.Callback != null) _OnGetServiceTypes.Callback();
    }
    void LauncherManagerProtocol.IManagerCallback.OnGetServices(ushort serverId, LauncherProtocolStructure.Service[] services)
    {
        _OnGetServices.serverId = serverId;
        _OnGetServices.services = services;
        __OnGetServices(serverId, services);
        if (_OnGetServices.GlobalCallback != null) _OnGetServices.GlobalCallback();
        if (_OnGetServices.Callback != null) _OnGetServices.Callback();
    }
    void LauncherManagerProtocol.IManagerCallback.OnServiceUpdate(LauncherProtocolStructure.Service service)
    {
        _OnServiceUpdate.service = service;
        __OnServiceUpdate(service);
        if (_OnServiceUpdate.GlobalCallback != null) _OnServiceUpdate.GlobalCallback();
        if (_OnServiceUpdate.Callback != null) _OnServiceUpdate.Callback();
    }
    void LauncherManagerProtocol.IManagerCallback.OnRevisionUpdate(ushort serverId, LauncherProtocolStructure.ServiceTypeRevision revision)
    {
        _OnRevisionUpdate.serverId = serverId;
        _OnRevisionUpdate.revision = revision;
        __OnRevisionUpdate(serverId, revision);
        if (_OnRevisionUpdate.GlobalCallback != null) _OnRevisionUpdate.GlobalCallback();
        if (_OnRevisionUpdate.Callback != null) _OnRevisionUpdate.Callback();
    }
    void LauncherManagerProtocol.IManagerCallback.OnStatusUpdate(string serviceName, LauncherProtocolStructure.EServiceStatus status, string time)
    {
        _OnStatusUpdate.serviceName = serviceName;
        _OnStatusUpdate.status = status;
        _OnStatusUpdate.time = time;
        __OnStatusUpdate(serviceName, status, time);
        if (_OnStatusUpdate.GlobalCallback != null) _OnStatusUpdate.GlobalCallback();
        if (_OnStatusUpdate.Callback != null) _OnStatusUpdate.Callback();
    }
    void LauncherManagerProtocol.IManagerCallback.OnGetManagers(LauncherManagerProtocol.Manager[] managers)
    {
        _OnGetManagers.managers = managers;
        __OnGetManagers(managers);
        if (_OnGetManagers.GlobalCallback != null) _OnGetManagers.GlobalCallback();
        if (_OnGetManagers.Callback != null) _OnGetManagers.Callback();
    }
    void LauncherManagerProtocol.IManagerCallback.OnLoginAgain()
    {
        __OnLoginAgain();
        if (_OnLoginAgain.GlobalCallback != null) _OnLoginAgain.GlobalCallback();
        if (_OnLoginAgain.Callback != null) _OnLoginAgain.Callback();
    }
    void LauncherManagerProtocol.IManagerCallback.OnGetLog(int page, LauncherManagerProtocol.LogRecord[] logs, int pages)
    {
        _OnGetLog.page = page;
        _OnGetLog.logs = logs;
        _OnGetLog.pages = pages;
        __OnGetLog(page, logs, pages);
        if (_OnGetLog.GlobalCallback != null) _OnGetLog.GlobalCallback();
        if (_OnGetLog.Callback != null) _OnGetLog.Callback();
    }
    void LauncherManagerProtocol.IManagerCallback.OnGetLogRepeat(LauncherManagerProtocol.LogRepeat data)
    {
        _OnGetLogRepeat.data = data;
        __OnGetLogRepeat(data);
        if (_OnGetLogRepeat.GlobalCallback != null) _OnGetLogRepeat.GlobalCallback();
        if (_OnGetLogRepeat.Callback != null) _OnGetLogRepeat.Callback();
    }
    void LauncherManagerProtocol.IManagerCallback.OnLog(string name, EntryEngine.Record record)
    {
        _OnLog.name = name;
        _OnLog.record = record;
        __OnLog(name, record);
        if (_OnLog.GlobalCallback != null) _OnLog.GlobalCallback();
        if (_OnLog.Callback != null) _OnLog.Callback();
    }
    protected virtual void __OnGetServers(LauncherProtocolStructure.Server[] servers){ }
    protected virtual void __OnGetServiceTypes(LauncherProtocolStructure.ServiceType[] serviceTypes){ }
    protected virtual void __OnGetServices(ushort serverId, LauncherProtocolStructure.Service[] services){ }
    protected virtual void __OnServiceUpdate(LauncherProtocolStructure.Service service){ }
    protected virtual void __OnRevisionUpdate(ushort serverId, LauncherProtocolStructure.ServiceTypeRevision revision){ }
    protected virtual void __OnStatusUpdate(string serviceName, LauncherProtocolStructure.EServiceStatus status, string time){ }
    protected virtual void __OnGetManagers(LauncherManagerProtocol.Manager[] managers){ }
    protected virtual void __OnLoginAgain(){ }
    protected virtual void __OnGetLog(int page, LauncherManagerProtocol.LogRecord[] logs, int pages){ }
    protected virtual void __OnGetLogRepeat(LauncherManagerProtocol.LogRepeat data){ }
    protected virtual void __OnLog(string name, EntryEngine.Record record){ }
    public static void ResetAllCallback()
    {
        _OnGetServers.Callback = null;
        _OnGetServiceTypes.Callback = null;
        _OnGetServices.Callback = null;
        _OnServiceUpdate.Callback = null;
        _OnRevisionUpdate.Callback = null;
        _OnStatusUpdate.Callback = null;
        _OnGetManagers.Callback = null;
        _OnLoginAgain.Callback = null;
        _OnGetLog.Callback = null;
        _OnGetLogRepeat.Callback = null;
        _OnLog.Callback = null;
    }
}
