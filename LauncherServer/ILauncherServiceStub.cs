using System;
using System.Collections.Generic;
using EntryEngine;
using EntryEngine.Network;
using EntryEngine.Serialize;
using LauncherProtocol;

interface _ILauncherService
{
    void PushServices(LauncherProtocolStructure.Service[] services);
    void RevisionUpdate(LauncherProtocolStructure.ServiceTypeRevision revision);
    void StatusUpdate(string name, LauncherProtocolStructure.EServiceStatus status, string time);
    void ServerStatusStatistic(LauncherProtocolStructure.ServerStatusData data);
    void Log(string name, EntryEngine.Record record);
    void LogServer(string name, EntryEngine.Record record);
}


static class ILauncherServiceCallbackProxy
{
    public static Action<ByteWriter> __WriteAgent;
}

class ILauncherServiceStub : Stub
{
    public _ILauncherService __Agent;
    public Func<_ILauncherService> __GetAgent;
    public Func<ByteReader, _ILauncherService> __ReadAgent;
    public ILauncherServiceStub(_ILauncherService agent) : base(0)
    {
        this.__Agent = agent;
        AddMethod(PushServices);
        AddMethod(RevisionUpdate);
        AddMethod(StatusUpdate);
        AddMethod(ServerStatusStatistic);
        AddMethod(Log);
        AddMethod(LogServer);
    }
    public ILauncherServiceStub(Func<_ILauncherService> agent) : this((_ILauncherService)null)
    {
        this.__GetAgent = agent;
    }
    public ILauncherServiceStub(Func<ByteReader, _ILauncherService> agent) : this((_ILauncherService)null)
    {
        this.__ReadAgent = agent;
    }
    void PushServices(ByteReader __stream)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) agent = temp; }
        LauncherProtocolStructure.Service[] services;
        __stream.Read(out services);
        #if DEBUG
        _LOG.Debug("PushServices services: {0}", JsonWriter.Serialize(services));
        #endif
        agent.PushServices(services);
    }
    void RevisionUpdate(ByteReader __stream)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) agent = temp; }
        LauncherProtocolStructure.ServiceTypeRevision revision;
        __stream.Read(out revision);
        #if DEBUG
        _LOG.Debug("RevisionUpdate revision: {0}", JsonWriter.Serialize(revision));
        #endif
        agent.RevisionUpdate(revision);
    }
    void StatusUpdate(ByteReader __stream)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) agent = temp; }
        string name;
        LauncherProtocolStructure.EServiceStatus status;
        string time;
        __stream.Read(out name);
        __stream.Read(out status);
        __stream.Read(out time);
        #if DEBUG
        _LOG.Debug("StatusUpdate name: {0}, status: {1}, time: {2}", name, status, time);
        #endif
        agent.StatusUpdate(name, status, time);
    }
    void ServerStatusStatistic(ByteReader __stream)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) agent = temp; }
        LauncherProtocolStructure.ServerStatusData data;
        __stream.Read(out data);
        #if DEBUG
        _LOG.Debug("ServerStatusStatistic data: {0}", JsonWriter.Serialize(data));
        #endif
        agent.ServerStatusStatistic(data);
    }
    void Log(ByteReader __stream)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) agent = temp; }
        string name;
        EntryEngine.Record record;
        __stream.Read(out name);
        __stream.Read(out record);
        #if DEBUG
        _LOG.Debug("Log name: {0}, record: {1}", name, JsonWriter.Serialize(record));
        #endif
        agent.Log(name, record);
    }
    void LogServer(ByteReader __stream)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) agent = temp; }
        string name;
        EntryEngine.Record record;
        __stream.Read(out name);
        __stream.Read(out record);
        #if DEBUG
        _LOG.Debug("LogServer name: {0}, record: {1}", name, JsonWriter.Serialize(record));
        #endif
        agent.LogServer(name, record);
    }
}
