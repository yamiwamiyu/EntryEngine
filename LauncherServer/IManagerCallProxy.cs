using System;
using System.Collections.Generic;
using EntryEngine;
using EntryEngine.Network;
using EntryEngine.Serialize;
using LauncherProtocol;

class IManagerCallProxy : StubClientAsync
{
    public Action<ByteWriter> __WriteAgent;
    
    public IManagerCallProxy()
    {
        this.Protocol = 1;
        AddMethod("New", New_0);
        AddMethod("Delete", Delete_1);
        AddMethod("Update", Update_3);
    }
    
    public StubClientAsync.AsyncWaitCallback New(LauncherProtocolStructure.ServiceType serviceType, string name, System.Action<LauncherProtocolStructure.Service> callback)
    {
        if (Link == null) return null;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)1);
        __writer.Write("New");
        __writer.Write(serviceType);
        __writer.Write(name);
        var __async = Push(callback);
        if (__async == null) return null;
        __writer.Write(__async.ID);
        #if DEBUG
        _LOG.Debug("New({0} bytes) serviceType: {1}, name: {2}, callback: {3}", __writer.Position, JsonWriter.Serialize(serviceType), name, "System.Action<LauncherProtocolStructure.Service>");
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
        return __async;
    }
    public StubClientAsync.AsyncWaitCallback Delete(string name, System.Action callback)
    {
        if (Link == null) return null;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)1);
        __writer.Write("Delete");
        __writer.Write(name);
        var __async = Push(callback);
        if (__async == null) return null;
        __writer.Write(__async.ID);
        #if DEBUG
        _LOG.Debug("Delete({0} bytes) name: {1}, callback: {2}", __writer.Position, name, "System.Action");
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
        return __async;
    }
    public void Launch(string name)
    {
        if (Link == null) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)1);
        __writer.Write("Launch");
        __writer.Write(name);
        #if DEBUG
        _LOG.Debug("Launch({0} bytes) name: {1}", __writer.Position, name);
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public StubClientAsync.AsyncWaitCallback Update(string name, System.Action<int> callback)
    {
        if (Link == null) return null;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)1);
        __writer.Write("Update");
        __writer.Write(name);
        var __async = Push(callback);
        if (__async == null) return null;
        __writer.Write(__async.ID);
        #if DEBUG
        _LOG.Debug("Update({0} bytes) name: {1}, callback: {2}", __writer.Position, name, "System.Action<int>");
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
        return __async;
    }
    public void Stop(string name)
    {
        if (Link == null) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)1);
        __writer.Write("Stop");
        __writer.Write(name);
        #if DEBUG
        _LOG.Debug("Stop({0} bytes) name: {1}", __writer.Position, name);
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void CallCommand(string name, string command)
    {
        if (Link == null) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)1);
        __writer.Write("CallCommand");
        __writer.Write(name);
        __writer.Write(command);
        #if DEBUG
        _LOG.Debug("CallCommand({0} bytes) name: {1}, command: {2}", __writer.Position, name, command);
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void UpdateSVN()
    {
        if (Link == null) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)1);
        __writer.Write("UpdateSVN");
        #if DEBUG
        _LOG.Debug("UpdateSVN({0} bytes)", __writer.Position);
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void ServiceTypeUpdate(LauncherProtocolStructure.ServiceType type)
    {
        if (Link == null) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)1);
        __writer.Write("ServiceTypeUpdate");
        __writer.Write(type);
        #if DEBUG
        _LOG.Debug("ServiceTypeUpdate({0} bytes) type: {1}", __writer.Position, JsonWriter.Serialize(type));
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void SetLaunchCommand(string name, string command)
    {
        if (Link == null) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)1);
        __writer.Write("SetLaunchCommand");
        __writer.Write(name);
        __writer.Write(command);
        #if DEBUG
        _LOG.Debug("SetLaunchCommand({0} bytes) name: {1}, command: {2}", __writer.Position, name, command);
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void UpdateLauncher()
    {
        if (Link == null) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)1);
        __writer.Write("UpdateLauncher");
        #if DEBUG
        _LOG.Debug("UpdateLauncher({0} bytes)", __writer.Position);
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    
    void New_0(ByteReader __stream)
    {
        byte __id;
        sbyte __ret;
        __stream.Read(out __id);
        __stream.Read(out __ret);
        var __callback = Pop(__id);
        if (__ret == 0)
        {
            LauncherProtocolStructure.Service obj;
            __stream.Read(out obj);
            #if DEBUG
            _LOG.Debug("New obj: {0}", JsonWriter.Serialize(obj));
            #endif
            var __invoke = (System.Action<LauncherProtocolStructure.Service>)__callback.Function;
            if (__invoke != null) __invoke(obj);
        }
        else
        {
            string __msg;
            __stream.Read(out __msg);
            _LOG.Error("New_0 error! id={0} ret={1} msg={2}", __id, __ret, __msg);
            Error(__callback, 0, __ret, __msg);
        }
    }
    void Delete_1(ByteReader __stream)
    {
        byte __id;
        sbyte __ret;
        __stream.Read(out __id);
        __stream.Read(out __ret);
        var __callback = Pop(__id);
        if (__ret == 0)
        {
            #if DEBUG
            _LOG.Debug("Delete");
            #endif
            var __invoke = (System.Action)__callback.Function;
            if (__invoke != null) __invoke();
        }
        else
        {
            string __msg;
            __stream.Read(out __msg);
            _LOG.Error("Delete_1 error! id={0} ret={1} msg={2}", __id, __ret, __msg);
            Error(__callback, 1, __ret, __msg);
        }
    }
    void Update_3(ByteReader __stream)
    {
        byte __id;
        sbyte __ret;
        __stream.Read(out __id);
        __stream.Read(out __ret);
        var __callback = Pop(__id);
        if (__ret == 0)
        {
            int obj;
            __stream.Read(out obj);
            #if DEBUG
            _LOG.Debug("Update obj: {0}", JsonWriter.Serialize(obj));
            #endif
            var __invoke = (System.Action<int>)__callback.Function;
            if (__invoke != null) __invoke(obj);
        }
        else
        {
            string __msg;
            __stream.Read(out __msg);
            _LOG.Error("Update_3 error! id={0} ret={1} msg={2}", __id, __ret, __msg);
            Error(__callback, 3, __ret, __msg);
        }
    }
}
