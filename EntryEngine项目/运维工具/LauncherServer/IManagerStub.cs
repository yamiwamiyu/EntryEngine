//using System;
//using System.Collections.Generic;
//using EntryEngine;
//using EntryEngine.Network;
//using EntryEngine.Serialize;
//using LauncherManagerProtocol;

//interface _IManager
//{
//    void NewServiceType(LauncherServer._Manager __client, LauncherProtocolStructure.ServiceType type, CBIManager_NewServiceType callback);
//    void ModifyServiceType(LauncherServer._Manager __client, string name, LauncherProtocolStructure.ServiceType type, CBIManager_ModifyServiceType callback);
//    void DeleteServiceType(LauncherServer._Manager __client, string name);
//    void UpdateSVN(LauncherServer._Manager __client);
//    void GetServices(LauncherServer._Manager __client);
//    void GetServers(LauncherServer._Manager __client);
//    void NewService(LauncherServer._Manager __client, ushort serverID, string serviceType, string name, string command);
//    void SetServiceLaunchCommand(LauncherServer._Manager __client, string serviceName, string command);
//    void CallCommand(LauncherServer._Manager __client, string serviceName, string command);
//    void DeleteService(LauncherServer._Manager __client, string serviceName);
//    void Launch(LauncherServer._Manager __client, string name);
//    void Update(LauncherServer._Manager __client, string name);
//    void Stop(LauncherServer._Manager __client, string name);
//    void NewManager(LauncherServer._Manager __client, LauncherManagerProtocol.Manager manager);
//    void DeleteManager(LauncherServer._Manager __client, string name);
//    void GetServerStatusStatistic(LauncherServer._Manager __client, ushort serverID, CBIManager_GetServerStatusStatistic callback);
//    void GetLog(LauncherServer._Manager __client, string name, System.DateTime? start, System.DateTime? end, byte pageCount, int page, string content, string param, byte[] levels);
//    void GroupLog(LauncherServer._Manager __client, string name, System.DateTime? start, System.DateTime? end, string content, string param, byte[] levels);
//    void GetLogRepeat(LauncherServer._Manager __client, int index);
//    void FindContext(LauncherServer._Manager __client, int index, System.DateTime? start, System.DateTime? end, byte pageCount, string content, string param, byte[] levels);
//    void UpdateManager(LauncherServer._Manager __client);
//}

//class CBIManager_NewServiceType : IDisposable
//{
//    private byte __id;
//    private Link __link;
//    internal bool IsCallback { get; private set; }
//    public CBIManager_NewServiceType(byte id, Link link)
//    {
//        this.__id = id;
//        this.__link = link;
//    }
//    public void Callback(bool obj) // INDEX = 0
//    {
//        if (IsCallback) return;
//        ByteWriter __writer = new ByteWriter();
//        __writer.Write(__id);
//        __writer.Write((sbyte)0);
//        __writer.Write(obj);
//        #if DEBUG
//        _LOG.Debug("CBIManager_NewServiceType Callback({0} bytes) obj: {1}", __writer.Position, JsonWriter.Serialize(obj));
//        #endif
//        IManagerCallbackProxy.NewServiceType_0(__link, __writer.Buffer, __writer.Position);
//        IsCallback = true;
//    }
//    public void Error(sbyte ret, string msg)
//    {
//        if (IsCallback) return;
//        ByteWriter __writer = new ByteWriter();
//        __writer.Write((byte)101);
//        __writer.Write((ushort)0);
//        __writer.Write(__id);
//        __writer.Write(ret);
//        __writer.Write(msg);
//        #if DEBUG
//        _LOG.Debug("CBIManager_NewServiceType Error({0} bytes) ret={1} msg={2}", __writer.Position, ret, msg);
//        #endif
//        __link.Write(__writer.Buffer, 0, __writer.Position);
//        IsCallback = true;
//    }
//    public void Dispose()
//    {
//        if (!IsCallback) Error(-2, "no callback");
//    }
//}
//class CBIManager_ModifyServiceType : IDisposable
//{
//    private byte __id;
//    private Link __link;
//    internal bool IsCallback { get; private set; }
//    public CBIManager_ModifyServiceType(byte id, Link link)
//    {
//        this.__id = id;
//        this.__link = link;
//    }
//    public void Callback(bool obj) // INDEX = 1
//    {
//        if (IsCallback) return;
//        ByteWriter __writer = new ByteWriter();
//        __writer.Write(__id);
//        __writer.Write((sbyte)0);
//        __writer.Write(obj);
//        #if DEBUG
//        _LOG.Debug("CBIManager_ModifyServiceType Callback({0} bytes) obj: {1}", __writer.Position, JsonWriter.Serialize(obj));
//        #endif
//        IManagerCallbackProxy.ModifyServiceType_1(__link, __writer.Buffer, __writer.Position);
//        IsCallback = true;
//    }
//    public void Error(sbyte ret, string msg)
//    {
//        if (IsCallback) return;
//        ByteWriter __writer = new ByteWriter();
//        __writer.Write((byte)101);
//        __writer.Write((ushort)1);
//        __writer.Write(__id);
//        __writer.Write(ret);
//        __writer.Write(msg);
//        #if DEBUG
//        _LOG.Debug("CBIManager_ModifyServiceType Error({0} bytes) ret={1} msg={2}", __writer.Position, ret, msg);
//        #endif
//        __link.Write(__writer.Buffer, 0, __writer.Position);
//        IsCallback = true;
//    }
//    public void Dispose()
//    {
//        if (!IsCallback) Error(-2, "no callback");
//    }
//}
//class CBIManager_GetServerStatusStatistic : IDisposable
//{
//    private byte __id;
//    private Link __link;
//    internal bool IsCallback { get; private set; }
//    public CBIManager_GetServerStatusStatistic(byte id, Link link)
//    {
//        this.__id = id;
//        this.__link = link;
//    }
//    public void Callback(LauncherProtocolStructure.ServerStatusData obj) // INDEX = 15
//    {
//        if (IsCallback) return;
//        ByteWriter __writer = new ByteWriter();
//        __writer.Write(__id);
//        __writer.Write((sbyte)0);
//        __writer.Write(obj);
//        #if DEBUG
//        _LOG.Debug("CBIManager_GetServerStatusStatistic Callback({0} bytes) obj: {1}", __writer.Position, JsonWriter.Serialize(obj));
//        #endif
//        IManagerCallbackProxy.GetServerStatusStatistic_15(__link, __writer.Buffer, __writer.Position);
//        IsCallback = true;
//    }
//    public void Error(sbyte ret, string msg)
//    {
//        if (IsCallback) return;
//        ByteWriter __writer = new ByteWriter();
//        __writer.Write((byte)101);
//        __writer.Write((ushort)15);
//        __writer.Write(__id);
//        __writer.Write(ret);
//        __writer.Write(msg);
//        #if DEBUG
//        _LOG.Debug("CBIManager_GetServerStatusStatistic Error({0} bytes) ret={1} msg={2}", __writer.Position, ret, msg);
//        #endif
//        __link.Write(__writer.Buffer, 0, __writer.Position);
//        IsCallback = true;
//    }
//    public void Dispose()
//    {
//        if (!IsCallback) Error(-2, "no callback");
//    }
//}

//static class IManagerCallbackProxy
//{
//    public static Action<ByteWriter> __WriteAgent;
//    internal static void NewServiceType_0(Link __link, byte[] data, int position)
//    {
//        if (__link == null || !__link.IsConnected) return;
//        ByteWriter __writer = new ByteWriter();
//        if (__WriteAgent != null) __WriteAgent(__writer);
//        __writer.Write((byte)101);
//        __writer.Write("NewServiceType");
//        __writer.WriteBytes(data, 0, position);
//        __link.Write(__writer.Buffer, 0, __writer.Position);
//    }
//    internal static void ModifyServiceType_1(Link __link, byte[] data, int position)
//    {
//        if (__link == null || !__link.IsConnected) return;
//        ByteWriter __writer = new ByteWriter();
//        if (__WriteAgent != null) __WriteAgent(__writer);
//        __writer.Write((byte)101);
//        __writer.Write("ModifyServiceType");
//        __writer.WriteBytes(data, 0, position);
//        __link.Write(__writer.Buffer, 0, __writer.Position);
//    }
//    public static void OnGetServers(Link __link, LauncherProtocolStructure.Server[] servers)
//    {
//        if (__link == null || !__link.IsConnected) return;
//        ByteWriter __writer = new ByteWriter();
//        if (__WriteAgent != null) __WriteAgent(__writer);
//        __writer.Write((byte)101);
//        __writer.Write("OnGetServers");
//        __writer.Write(servers);
//        #if DEBUG
//        _LOG.Debug("OnGetServers({0} bytes) servers: {1}", __writer.Position, JsonWriter.Serialize(servers));
//        #endif
//        __link.Write(__writer.Buffer, 0, __writer.Position);
//    }
//    public static void OnGetServiceTypes(Link __link, LauncherProtocolStructure.ServiceType[] serviceTypes)
//    {
//        if (__link == null || !__link.IsConnected) return;
//        ByteWriter __writer = new ByteWriter();
//        if (__WriteAgent != null) __WriteAgent(__writer);
//        __writer.Write((byte)101);
//        __writer.Write("OnGetServiceTypes");
//        __writer.Write(serviceTypes);
//        #if DEBUG
//        _LOG.Debug("OnGetServiceTypes({0} bytes) serviceTypes: {1}", __writer.Position, JsonWriter.Serialize(serviceTypes));
//        #endif
//        __link.Write(__writer.Buffer, 0, __writer.Position);
//    }
//    public static void OnGetServices(Link __link, ushort serverId, LauncherProtocolStructure.Service[] services)
//    {
//        if (__link == null || !__link.IsConnected) return;
//        ByteWriter __writer = new ByteWriter();
//        if (__WriteAgent != null) __WriteAgent(__writer);
//        __writer.Write((byte)101);
//        __writer.Write("OnGetServices");
//        __writer.Write(serverId);
//        __writer.Write(services);
//        #if DEBUG
//        _LOG.Debug("OnGetServices({0} bytes) serverId: {1}, services: {2}", __writer.Position, serverId, JsonWriter.Serialize(services));
//        #endif
//        __link.Write(__writer.Buffer, 0, __writer.Position);
//    }
//    public static void OnServiceUpdate(Link __link, LauncherProtocolStructure.Service service)
//    {
//        if (__link == null || !__link.IsConnected) return;
//        ByteWriter __writer = new ByteWriter();
//        if (__WriteAgent != null) __WriteAgent(__writer);
//        __writer.Write((byte)101);
//        __writer.Write("OnServiceUpdate");
//        __writer.Write(service);
//        #if DEBUG
//        _LOG.Debug("OnServiceUpdate({0} bytes) service: {1}", __writer.Position, JsonWriter.Serialize(service));
//        #endif
//        __link.Write(__writer.Buffer, 0, __writer.Position);
//    }
//    public static void OnRevisionUpdate(Link __link, ushort serverId, LauncherProtocolStructure.ServiceTypeRevision revision)
//    {
//        if (__link == null || !__link.IsConnected) return;
//        ByteWriter __writer = new ByteWriter();
//        if (__WriteAgent != null) __WriteAgent(__writer);
//        __writer.Write((byte)101);
//        __writer.Write("OnRevisionUpdate");
//        __writer.Write(serverId);
//        __writer.Write(revision);
//        #if DEBUG
//        _LOG.Debug("OnRevisionUpdate({0} bytes) serverId: {1}, revision: {2}", __writer.Position, serverId, JsonWriter.Serialize(revision));
//        #endif
//        __link.Write(__writer.Buffer, 0, __writer.Position);
//    }
//    public static void OnStatusUpdate(Link __link, string serviceName, LauncherProtocolStructure.EServiceStatus status, string time)
//    {
//        if (__link == null || !__link.IsConnected) return;
//        ByteWriter __writer = new ByteWriter();
//        if (__WriteAgent != null) __WriteAgent(__writer);
//        __writer.Write((byte)101);
//        __writer.Write("OnStatusUpdate");
//        __writer.Write(serviceName);
//        __writer.Write(status);
//        __writer.Write(time);
//        #if DEBUG
//        _LOG.Debug("OnStatusUpdate({0} bytes) serviceName: {1}, status: {2}, time: {3}", __writer.Position, serviceName, status, time);
//        #endif
//        __link.Write(__writer.Buffer, 0, __writer.Position);
//    }
//    public static void OnGetManagers(Link __link, LauncherManagerProtocol.Manager[] managers)
//    {
//        if (__link == null || !__link.IsConnected) return;
//        ByteWriter __writer = new ByteWriter();
//        if (__WriteAgent != null) __WriteAgent(__writer);
//        __writer.Write((byte)101);
//        __writer.Write("OnGetManagers");
//        __writer.Write(managers);
//        #if DEBUG
//        _LOG.Debug("OnGetManagers({0} bytes) managers: {1}", __writer.Position, JsonWriter.Serialize(managers));
//        #endif
//        __link.Write(__writer.Buffer, 0, __writer.Position);
//    }
//    public static void OnLoginAgain(Link __link)
//    {
//        if (__link == null || !__link.IsConnected) return;
//        ByteWriter __writer = new ByteWriter();
//        if (__WriteAgent != null) __WriteAgent(__writer);
//        __writer.Write((byte)101);
//        __writer.Write("OnLoginAgain");
//        #if DEBUG
//        _LOG.Debug("OnLoginAgain({0} bytes)", __writer.Position);
//        #endif
//        __link.Write(__writer.Buffer, 0, __writer.Position);
//    }
//    public static void OnGetLog(Link __link, int page, LauncherManagerProtocol.LogRecord[] logs, int pages)
//    {
//        if (__link == null || !__link.IsConnected) return;
//        ByteWriter __writer = new ByteWriter();
//        if (__WriteAgent != null) __WriteAgent(__writer);
//        __writer.Write((byte)101);
//        __writer.Write("OnGetLog");
//        __writer.Write(page);
//        __writer.Write(logs);
//        __writer.Write(pages);
//        #if DEBUG
//        _LOG.Debug("OnGetLog({0} bytes) page: {1}, logs: {2}, pages: {3}", __writer.Position, page, JsonWriter.Serialize(logs), pages);
//        #endif
//        __link.Write(__writer.Buffer, 0, __writer.Position);
//    }
//    public static void OnGetLogRepeat(Link __link, LauncherManagerProtocol.LogRepeat data)
//    {
//        if (__link == null || !__link.IsConnected) return;
//        ByteWriter __writer = new ByteWriter();
//        if (__WriteAgent != null) __WriteAgent(__writer);
//        __writer.Write((byte)101);
//        __writer.Write("OnGetLogRepeat");
//        __writer.Write(data);
//        #if DEBUG
//        _LOG.Debug("OnGetLogRepeat({0} bytes) data: {1}", __writer.Position, JsonWriter.Serialize(data));
//        #endif
//        __link.Write(__writer.Buffer, 0, __writer.Position);
//    }
//    public static void OnLog(Link __link, string name, EntryEngine.Record record)
//    {
//        if (__link == null || !__link.IsConnected) return;
//        ByteWriter __writer = new ByteWriter();
//        if (__WriteAgent != null) __WriteAgent(__writer);
//        __writer.Write((byte)101);
//        __writer.Write("OnLog");
//        __writer.Write(name);
//        __writer.Write(record);
//        #if DEBUG
//        _LOG.Debug("OnLog({0} bytes) name: {1}, record: {2}", __writer.Position, name, JsonWriter.Serialize(record));
//        #endif
//        __link.Write(__writer.Buffer, 0, __writer.Position);
//    }
//    internal static void GetServerStatusStatistic_15(Link __link, byte[] data, int position)
//    {
//        if (__link == null || !__link.IsConnected) return;
//        ByteWriter __writer = new ByteWriter();
//        if (__WriteAgent != null) __WriteAgent(__writer);
//        __writer.Write((byte)101);
//        __writer.Write("GetServerStatusStatistic");
//        __writer.WriteBytes(data, 0, position);
//        __link.Write(__writer.Buffer, 0, __writer.Position);
//    }
//}

//class IManagerStub : Stub
//{
//    public _IManager __Agent;
//    public Func<_IManager> __GetAgent;
//    public Func<ByteReader, LauncherServer._Manager> __ReadAgent;
//    public IManagerStub(_IManager agent) : base(101)
//    {
//        this.__Agent = agent;
//        AddMethod("NewServiceType", NewServiceType);
//        AddMethod("ModifyServiceType", ModifyServiceType);
//        AddMethod("DeleteServiceType", DeleteServiceType);
//        AddMethod("UpdateSVN", UpdateSVN);
//        AddMethod("GetServices", GetServices);
//        AddMethod("GetServers", GetServers);
//        AddMethod("NewService", NewService);
//        AddMethod("SetServiceLaunchCommand", SetServiceLaunchCommand);
//        AddMethod("CallCommand", CallCommand);
//        AddMethod("DeleteService", DeleteService);
//        AddMethod("Launch", Launch);
//        AddMethod("Update", Update);
//        AddMethod("Stop", Stop);
//        AddMethod("NewManager", NewManager);
//        AddMethod("DeleteManager", DeleteManager);
//        AddMethod("GetServerStatusStatistic", GetServerStatusStatistic);
//        AddMethod("GetLog", GetLog);
//        AddMethod("GroupLog", GroupLog);
//        AddMethod("GetLogRepeat", GetLogRepeat);
//        AddMethod("FindContext", FindContext);
//        AddMethod("UpdateManager", UpdateManager);
//    }
//    public IManagerStub(Func<_IManager> agent) : this((_IManager)null)
//    {
//        this.__GetAgent = agent;
//    }
//    public IManagerStub(Func<ByteReader, LauncherServer._Manager> agent) : this((_IManager)null)
//    {
//        this.__ReadAgent = agent;
//    }
//    void NewServiceType(ByteReader __stream)
//    {
//        var agent = __Agent;
//        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
//        var __client = default(LauncherServer._Manager);
//        if (__ReadAgent != null) { var temp = __ReadAgent(__stream);if (temp != null) __client = temp; }
//        LauncherProtocolStructure.ServiceType type;
//        byte callback;
//        __stream.Read(out type);
//        __stream.Read(out callback);
//        #if DEBUG
//        _LOG.Debug("NewServiceType type: {0}, callback: {1}", JsonWriter.Serialize(type), "System.Action<bool>");
//        #endif
//        var __callback = new CBIManager_NewServiceType(callback, Link);
//        try
//        {
//            agent.NewServiceType(__client, type, __callback);
//        }
//        catch (Exception ex)
//        {
//            _LOG.Error("Callback_NewServiceType error! msg={0} stack={1}", ex.Message, ex.StackTrace);
//            if (!__callback.IsCallback) __callback.Error(-1, ex.Message);
//        }
//    }
//    void ModifyServiceType(ByteReader __stream)
//    {
//        var agent = __Agent;
//        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
//        var __client = default(LauncherServer._Manager);
//        if (__ReadAgent != null) { var temp = __ReadAgent(__stream);if (temp != null) __client = temp; }
//        string name;
//        LauncherProtocolStructure.ServiceType type;
//        byte callback;
//        __stream.Read(out name);
//        __stream.Read(out type);
//        __stream.Read(out callback);
//        #if DEBUG
//        _LOG.Debug("ModifyServiceType name: {0}, type: {1}, callback: {2}", name, JsonWriter.Serialize(type), "System.Action<bool>");
//        #endif
//        var __callback = new CBIManager_ModifyServiceType(callback, Link);
//        try
//        {
//            agent.ModifyServiceType(__client, name, type, __callback);
//        }
//        catch (Exception ex)
//        {
//            _LOG.Error("Callback_ModifyServiceType error! msg={0} stack={1}", ex.Message, ex.StackTrace);
//            if (!__callback.IsCallback) __callback.Error(-1, ex.Message);
//        }
//    }
//    void DeleteServiceType(ByteReader __stream)
//    {
//        var agent = __Agent;
//        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
//        var __client = default(LauncherServer._Manager);
//        if (__ReadAgent != null) { var temp = __ReadAgent(__stream);if (temp != null) __client = temp; }
//        string name;
//        __stream.Read(out name);
//        #if DEBUG
//        _LOG.Debug("DeleteServiceType name: {0}", name);
//        #endif
//        agent.DeleteServiceType(__client, name);
//    }
//    void UpdateSVN(ByteReader __stream)
//    {
//        var agent = __Agent;
//        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
//        var __client = default(LauncherServer._Manager);
//        if (__ReadAgent != null) { var temp = __ReadAgent(__stream);if (temp != null) __client = temp; }
//        #if DEBUG
//        _LOG.Debug("UpdateSVN");
//        #endif
//        agent.UpdateSVN(__client);
//    }
//    void GetServices(ByteReader __stream)
//    {
//        var agent = __Agent;
//        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
//        var __client = default(LauncherServer._Manager);
//        if (__ReadAgent != null) { var temp = __ReadAgent(__stream);if (temp != null) __client = temp; }
//        #if DEBUG
//        _LOG.Debug("GetServices");
//        #endif
//        agent.GetServices(__client);
//    }
//    void GetServers(ByteReader __stream)
//    {
//        var agent = __Agent;
//        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
//        var __client = default(LauncherServer._Manager);
//        if (__ReadAgent != null) { var temp = __ReadAgent(__stream);if (temp != null) __client = temp; }
//        #if DEBUG
//        _LOG.Debug("GetServers");
//        #endif
//        agent.GetServers(__client);
//    }
//    void NewService(ByteReader __stream)
//    {
//        var agent = __Agent;
//        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
//        var __client = default(LauncherServer._Manager);
//        if (__ReadAgent != null) { var temp = __ReadAgent(__stream);if (temp != null) __client = temp; }
//        ushort serverID;
//        string serviceType;
//        string name;
//        string command;
//        __stream.Read(out serverID);
//        __stream.Read(out serviceType);
//        __stream.Read(out name);
//        __stream.Read(out command);
//        #if DEBUG
//        _LOG.Debug("NewService serverID: {0}, serviceType: {1}, name: {2}, command: {3}", serverID, serviceType, name, command);
//        #endif
//        agent.NewService(__client, serverID, serviceType, name, command);
//    }
//    void SetServiceLaunchCommand(ByteReader __stream)
//    {
//        var agent = __Agent;
//        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
//        var __client = default(LauncherServer._Manager);
//        if (__ReadAgent != null) { var temp = __ReadAgent(__stream);if (temp != null) __client = temp; }
//        string serviceName;
//        string command;
//        __stream.Read(out serviceName);
//        __stream.Read(out command);
//        #if DEBUG
//        _LOG.Debug("SetServiceLaunchCommand serviceName: {0}, command: {1}", serviceName, command);
//        #endif
//        agent.SetServiceLaunchCommand(__client, serviceName, command);
//    }
//    void CallCommand(ByteReader __stream)
//    {
//        var agent = __Agent;
//        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
//        var __client = default(LauncherServer._Manager);
//        if (__ReadAgent != null) { var temp = __ReadAgent(__stream);if (temp != null) __client = temp; }
//        string serviceName;
//        string command;
//        __stream.Read(out serviceName);
//        __stream.Read(out command);
//        #if DEBUG
//        _LOG.Debug("CallCommand serviceName: {0}, command: {1}", serviceName, command);
//        #endif
//        agent.CallCommand(__client, serviceName, command);
//    }
//    void DeleteService(ByteReader __stream)
//    {
//        var agent = __Agent;
//        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
//        var __client = default(LauncherServer._Manager);
//        if (__ReadAgent != null) { var temp = __ReadAgent(__stream);if (temp != null) __client = temp; }
//        string serviceName;
//        __stream.Read(out serviceName);
//        #if DEBUG
//        _LOG.Debug("DeleteService serviceName: {0}", serviceName);
//        #endif
//        agent.DeleteService(__client, serviceName);
//    }
//    void Launch(ByteReader __stream)
//    {
//        var agent = __Agent;
//        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
//        var __client = default(LauncherServer._Manager);
//        if (__ReadAgent != null) { var temp = __ReadAgent(__stream);if (temp != null) __client = temp; }
//        string name;
//        __stream.Read(out name);
//        #if DEBUG
//        _LOG.Debug("Launch name: {0}", name);
//        #endif
//        agent.Launch(__client, name);
//    }
//    void Update(ByteReader __stream)
//    {
//        var agent = __Agent;
//        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
//        var __client = default(LauncherServer._Manager);
//        if (__ReadAgent != null) { var temp = __ReadAgent(__stream);if (temp != null) __client = temp; }
//        string name;
//        __stream.Read(out name);
//        #if DEBUG
//        _LOG.Debug("Update name: {0}", name);
//        #endif
//        agent.Update(__client, name);
//    }
//    void Stop(ByteReader __stream)
//    {
//        var agent = __Agent;
//        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
//        var __client = default(LauncherServer._Manager);
//        if (__ReadAgent != null) { var temp = __ReadAgent(__stream);if (temp != null) __client = temp; }
//        string name;
//        __stream.Read(out name);
//        #if DEBUG
//        _LOG.Debug("Stop name: {0}", name);
//        #endif
//        agent.Stop(__client, name);
//    }
//    void NewManager(ByteReader __stream)
//    {
//        var agent = __Agent;
//        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
//        var __client = default(LauncherServer._Manager);
//        if (__ReadAgent != null) { var temp = __ReadAgent(__stream);if (temp != null) __client = temp; }
//        LauncherManagerProtocol.Manager manager;
//        __stream.Read(out manager);
//        #if DEBUG
//        _LOG.Debug("NewManager manager: {0}", JsonWriter.Serialize(manager));
//        #endif
//        agent.NewManager(__client, manager);
//    }
//    void DeleteManager(ByteReader __stream)
//    {
//        var agent = __Agent;
//        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
//        var __client = default(LauncherServer._Manager);
//        if (__ReadAgent != null) { var temp = __ReadAgent(__stream);if (temp != null) __client = temp; }
//        string name;
//        __stream.Read(out name);
//        #if DEBUG
//        _LOG.Debug("DeleteManager name: {0}", name);
//        #endif
//        agent.DeleteManager(__client, name);
//    }
//    void GetServerStatusStatistic(ByteReader __stream)
//    {
//        var agent = __Agent;
//        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
//        var __client = default(LauncherServer._Manager);
//        if (__ReadAgent != null) { var temp = __ReadAgent(__stream);if (temp != null) __client = temp; }
//        ushort serverID;
//        byte callback;
//        __stream.Read(out serverID);
//        __stream.Read(out callback);
//        #if DEBUG
//        _LOG.Debug("GetServerStatusStatistic serverID: {0}, callback: {1}", serverID, "System.Action<LauncherProtocolStructure.ServerStatusData>");
//        #endif
//        var __callback = new CBIManager_GetServerStatusStatistic(callback, Link);
//        try
//        {
//            agent.GetServerStatusStatistic(__client, serverID, __callback);
//        }
//        catch (Exception ex)
//        {
//            _LOG.Error("Callback_GetServerStatusStatistic error! msg={0} stack={1}", ex.Message, ex.StackTrace);
//            if (!__callback.IsCallback) __callback.Error(-1, ex.Message);
//        }
//    }
//    void GetLog(ByteReader __stream)
//    {
//        var agent = __Agent;
//        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
//        var __client = default(LauncherServer._Manager);
//        if (__ReadAgent != null) { var temp = __ReadAgent(__stream);if (temp != null) __client = temp; }
//        string name;
//        System.DateTime? start;
//        System.DateTime? end;
//        byte pageCount;
//        int page;
//        string content;
//        string param;
//        byte[] levels;
//        __stream.Read(out name);
//        __stream.Read(out start);
//        __stream.Read(out end);
//        __stream.Read(out pageCount);
//        __stream.Read(out page);
//        __stream.Read(out content);
//        __stream.Read(out param);
//        __stream.Read(out levels);
//        #if DEBUG
//        _LOG.Debug("GetLog name: {0}, start: {1}, end: {2}, pageCount: {3}, page: {4}, content: {5}, param: {6}, levels: {7}", name, JsonWriter.Serialize(start), JsonWriter.Serialize(end), pageCount, page, content, param, JsonWriter.Serialize(levels));
//        #endif
//        agent.GetLog(__client, name, start, end, pageCount, page, content, param, levels);
//    }
//    void GroupLog(ByteReader __stream)
//    {
//        var agent = __Agent;
//        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
//        var __client = default(LauncherServer._Manager);
//        if (__ReadAgent != null) { var temp = __ReadAgent(__stream);if (temp != null) __client = temp; }
//        string name;
//        System.DateTime? start;
//        System.DateTime? end;
//        string content;
//        string param;
//        byte[] levels;
//        __stream.Read(out name);
//        __stream.Read(out start);
//        __stream.Read(out end);
//        __stream.Read(out content);
//        __stream.Read(out param);
//        __stream.Read(out levels);
//        #if DEBUG
//        _LOG.Debug("GroupLog name: {0}, start: {1}, end: {2}, content: {3}, param: {4}, levels: {5}", name, JsonWriter.Serialize(start), JsonWriter.Serialize(end), content, param, JsonWriter.Serialize(levels));
//        #endif
//        agent.GroupLog(__client, name, start, end, content, param, levels);
//    }
//    void GetLogRepeat(ByteReader __stream)
//    {
//        var agent = __Agent;
//        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
//        var __client = default(LauncherServer._Manager);
//        if (__ReadAgent != null) { var temp = __ReadAgent(__stream);if (temp != null) __client = temp; }
//        int index;
//        __stream.Read(out index);
//        #if DEBUG
//        _LOG.Debug("GetLogRepeat index: {0}", index);
//        #endif
//        agent.GetLogRepeat(__client, index);
//    }
//    void FindContext(ByteReader __stream)
//    {
//        var agent = __Agent;
//        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
//        var __client = default(LauncherServer._Manager);
//        if (__ReadAgent != null) { var temp = __ReadAgent(__stream);if (temp != null) __client = temp; }
//        int index;
//        System.DateTime? start;
//        System.DateTime? end;
//        byte pageCount;
//        string content;
//        string param;
//        byte[] levels;
//        __stream.Read(out index);
//        __stream.Read(out start);
//        __stream.Read(out end);
//        __stream.Read(out pageCount);
//        __stream.Read(out content);
//        __stream.Read(out param);
//        __stream.Read(out levels);
//        #if DEBUG
//        _LOG.Debug("FindContext index: {0}, start: {1}, end: {2}, pageCount: {3}, content: {4}, param: {5}, levels: {6}", index, JsonWriter.Serialize(start), JsonWriter.Serialize(end), pageCount, content, param, JsonWriter.Serialize(levels));
//        #endif
//        agent.FindContext(__client, index, start, end, pageCount, content, param, levels);
//    }
//    void UpdateManager(ByteReader __stream)
//    {
//        var agent = __Agent;
//        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
//        var __client = default(LauncherServer._Manager);
//        if (__ReadAgent != null) { var temp = __ReadAgent(__stream);if (temp != null) __client = temp; }
//        #if DEBUG
//        _LOG.Debug("UpdateManager");
//        #endif
//        agent.UpdateManager(__client);
//    }
//}
