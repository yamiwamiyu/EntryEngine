using System;
using System.Collections.Generic;
using EntryEngine;
using EntryEngine.Network;
using EntryEngine.Serialize;
using LauncherProtocol;

class ILauncherServiceProxy : StubClientAsync, ILauncherService
{
    public Action<ByteWriter> __WriteAgent;
    
    public ILauncherServiceProxy()
    {
        this.Protocol = 0;
    }
    
    public void PushServices(LauncherProtocolStructure.Service[] services)
    {
        if (Link == null) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)0);
        __writer.Write((ushort)0);
        __writer.Write(services);
        #if DEBUG
        _LOG.Debug("PushServices({0} bytes) services: {1}", __writer.Position, JsonWriter.Serialize(services));
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void RevisionUpdate(LauncherProtocolStructure.ServiceTypeRevision revision)
    {
        if (Link == null) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)0);
        __writer.Write((ushort)1);
        __writer.Write(revision);
        #if DEBUG
        _LOG.Debug("RevisionUpdate({0} bytes) revision: {1}", __writer.Position, JsonWriter.Serialize(revision));
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void StatusUpdate(string name, LauncherProtocolStructure.EServiceStatus status, string time)
    {
        if (Link == null) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)0);
        __writer.Write((ushort)2);
        __writer.Write(name);
        __writer.Write(status);
        __writer.Write(time);
        #if DEBUG
        _LOG.Debug("StatusUpdate({0} bytes) name: {1}, status: {2}, time: {3}", __writer.Position, name, status, time);
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void ServerStatusStatistic(LauncherProtocolStructure.ServerStatusData data)
    {
        if (Link == null) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)0);
        __writer.Write((ushort)3);
        __writer.Write(data);
        #if DEBUG
        _LOG.Debug("ServerStatusStatistic({0} bytes) data: {1}", __writer.Position, JsonWriter.Serialize(data));
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void Log(string name, EntryEngine.Record record)
    {
        if (Link == null) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)0);
        __writer.Write((ushort)4);
        __writer.Write(name);
        __writer.Write(record);
        #if DEBUG
        _LOG.Debug("Log({0} bytes) name: {1}, record: {2}", __writer.Position, name, JsonWriter.Serialize(record));
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    public void LogServer(string name, EntryEngine.Record record)
    {
        if (Link == null) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)0);
        __writer.Write((ushort)5);
        __writer.Write(name);
        __writer.Write(record);
        #if DEBUG
        _LOG.Debug("LogServer({0} bytes) name: {1}, record: {2}", __writer.Position, name, JsonWriter.Serialize(record));
        #endif
        Link.Write(__writer.Buffer, 0, __writer.Position);
    }
    
}
