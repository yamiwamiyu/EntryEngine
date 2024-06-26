﻿using System;
using System.Collections.Generic;
using EntryEngine;
using EntryEngine.Network;
using EntryEngine.Serialize;
using LauncherProtocol;

interface _IManagerCall
{
    void New(LauncherProtocolStructure.ServiceType serviceType, string name, CBIManagerCall_New callback);
    void Delete(string name, CBIManagerCall_Delete callback);
    void Launch(string name, CBIManagerCall_Launch callback);
    void Update(string name, CBIManagerCall_Update callback);
    void Stop(string name, CBIManagerCall_Stop callback);
    void GetCommands(string name, CBIManagerCall_GetCommands callback);
    void CallCommand(string name, string command);
    void UpdateSVN();
    void ServiceTypeUpdate(LauncherProtocolStructure.ServiceType type);
    void SetLaunchCommand(string name, string exe, string command);
    void UpdateLauncher();
}

class CBIManagerCall_New : IDisposable
{
    private byte __id;
    private Link __link;
    internal bool IsCallback { get; private set; }
    public CBIManagerCall_New(byte id, Link link)
    {
        this.__id = id;
        this.__link = link;
    }
    public void Callback(LauncherProtocolStructure.Service obj) // INDEX = 0
    {
        if (IsCallback) return;
        ByteWriter __writer = new ByteWriter();
        __writer.Write(__id);
        __writer.Write((sbyte)0);
        __writer.Write(obj);
        #if DEBUG
        _LOG.Debug("CBIManagerCall_New Callback({0} bytes) obj: {1}", __writer.Position, JsonWriter.Serialize(obj));
        #endif
        IManagerCallCallbackProxy.New_0(__link, __writer.Buffer, __writer.Position);
        IsCallback = true;
    }
    public void Error(sbyte ret, string msg)
    {
        if (IsCallback) return;
        ByteWriter __writer = new ByteWriter();
        __writer.Write((byte)1);
        __writer.Write((ushort)0);
        __writer.Write(__id);
        __writer.Write(ret);
        __writer.Write(msg);
        #if DEBUG
        _LOG.Debug("CBIManagerCall_New Error({0} bytes) ret={1} msg={2}", __writer.Position, ret, msg);
        #endif
        __link.Write(__writer.Buffer, 0, __writer.Position);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
class CBIManagerCall_Delete : IDisposable
{
    private byte __id;
    private Link __link;
    internal bool IsCallback { get; private set; }
    public CBIManagerCall_Delete(byte id, Link link)
    {
        this.__id = id;
        this.__link = link;
    }
    public void Callback() // INDEX = 1
    {
        if (IsCallback) return;
        ByteWriter __writer = new ByteWriter();
        __writer.Write(__id);
        __writer.Write((sbyte)0);
        #if DEBUG
        _LOG.Debug("CBIManagerCall_Delete Callback({0} bytes)", __writer.Position);
        #endif
        IManagerCallCallbackProxy.Delete_1(__link, __writer.Buffer, __writer.Position);
        IsCallback = true;
    }
    public void Error(sbyte ret, string msg)
    {
        if (IsCallback) return;
        ByteWriter __writer = new ByteWriter();
        __writer.Write((byte)1);
        __writer.Write((ushort)1);
        __writer.Write(__id);
        __writer.Write(ret);
        __writer.Write(msg);
        #if DEBUG
        _LOG.Debug("CBIManagerCall_Delete Error({0} bytes) ret={1} msg={2}", __writer.Position, ret, msg);
        #endif
        __link.Write(__writer.Buffer, 0, __writer.Position);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
class CBIManagerCall_Launch : IDisposable
{
    private byte __id;
    private Link __link;
    internal bool IsCallback { get; private set; }
    public CBIManagerCall_Launch(byte id, Link link)
    {
        this.__id = id;
        this.__link = link;
    }
    public void Callback() // INDEX = 2
    {
        if (IsCallback) return;
        ByteWriter __writer = new ByteWriter();
        __writer.Write(__id);
        __writer.Write((sbyte)0);
        #if DEBUG
        _LOG.Debug("CBIManagerCall_Launch Callback({0} bytes)", __writer.Position);
        #endif
        IManagerCallCallbackProxy.Launch_2(__link, __writer.Buffer, __writer.Position);
        IsCallback = true;
    }
    public void Error(sbyte ret, string msg)
    {
        if (IsCallback) return;
        ByteWriter __writer = new ByteWriter();
        __writer.Write((byte)1);
        __writer.Write((ushort)2);
        __writer.Write(__id);
        __writer.Write(ret);
        __writer.Write(msg);
        #if DEBUG
        _LOG.Debug("CBIManagerCall_Launch Error({0} bytes) ret={1} msg={2}", __writer.Position, ret, msg);
        #endif
        __link.Write(__writer.Buffer, 0, __writer.Position);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
class CBIManagerCall_Update : IDisposable
{
    private byte __id;
    private Link __link;
    internal bool IsCallback { get; private set; }
    public CBIManagerCall_Update(byte id, Link link)
    {
        this.__id = id;
        this.__link = link;
    }
    public void Callback(int obj) // INDEX = 3
    {
        if (IsCallback) return;
        ByteWriter __writer = new ByteWriter();
        __writer.Write(__id);
        __writer.Write((sbyte)0);
        __writer.Write(obj);
        #if DEBUG
        _LOG.Debug("CBIManagerCall_Update Callback({0} bytes) obj: {1}", __writer.Position, JsonWriter.Serialize(obj));
        #endif
        IManagerCallCallbackProxy.Update_3(__link, __writer.Buffer, __writer.Position);
        IsCallback = true;
    }
    public void Error(sbyte ret, string msg)
    {
        if (IsCallback) return;
        ByteWriter __writer = new ByteWriter();
        __writer.Write((byte)1);
        __writer.Write((ushort)3);
        __writer.Write(__id);
        __writer.Write(ret);
        __writer.Write(msg);
        #if DEBUG
        _LOG.Debug("CBIManagerCall_Update Error({0} bytes) ret={1} msg={2}", __writer.Position, ret, msg);
        #endif
        __link.Write(__writer.Buffer, 0, __writer.Position);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
class CBIManagerCall_Stop : IDisposable
{
    private byte __id;
    private Link __link;
    internal bool IsCallback { get; private set; }
    public CBIManagerCall_Stop(byte id, Link link)
    {
        this.__id = id;
        this.__link = link;
    }
    public void Callback() // INDEX = 4
    {
        if (IsCallback) return;
        ByteWriter __writer = new ByteWriter();
        __writer.Write(__id);
        __writer.Write((sbyte)0);
        #if DEBUG
        _LOG.Debug("CBIManagerCall_Stop Callback({0} bytes)", __writer.Position);
        #endif
        IManagerCallCallbackProxy.Stop_4(__link, __writer.Buffer, __writer.Position);
        IsCallback = true;
    }
    public void Error(sbyte ret, string msg)
    {
        if (IsCallback) return;
        ByteWriter __writer = new ByteWriter();
        __writer.Write((byte)1);
        __writer.Write((ushort)4);
        __writer.Write(__id);
        __writer.Write(ret);
        __writer.Write(msg);
        #if DEBUG
        _LOG.Debug("CBIManagerCall_Stop Error({0} bytes) ret={1} msg={2}", __writer.Position, ret, msg);
        #endif
        __link.Write(__writer.Buffer, 0, __writer.Position);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
class CBIManagerCall_GetCommands : IDisposable
{
    private byte __id;
    private Link __link;
    internal bool IsCallback { get; private set; }
    public CBIManagerCall_GetCommands(byte id, Link link)
    {
        this.__id = id;
        this.__link = link;
    }
    public void Callback(System.Collections.Generic.List<string> obj) // INDEX = 5
    {
        if (IsCallback) return;
        ByteWriter __writer = new ByteWriter();
        __writer.Write(__id);
        __writer.Write((sbyte)0);
        __writer.Write(obj);
        #if DEBUG
        _LOG.Debug("CBIManagerCall_GetCommands Callback({0} bytes) obj: {1}", __writer.Position, JsonWriter.Serialize(obj));
        #endif
        IManagerCallCallbackProxy.GetCommands_5(__link, __writer.Buffer, __writer.Position);
        IsCallback = true;
    }
    public void Error(sbyte ret, string msg)
    {
        if (IsCallback) return;
        ByteWriter __writer = new ByteWriter();
        __writer.Write((byte)1);
        __writer.Write((ushort)5);
        __writer.Write(__id);
        __writer.Write(ret);
        __writer.Write(msg);
        #if DEBUG
        _LOG.Debug("CBIManagerCall_GetCommands Error({0} bytes) ret={1} msg={2}", __writer.Position, ret, msg);
        #endif
        __link.Write(__writer.Buffer, 0, __writer.Position);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}

static class IManagerCallCallbackProxy
{
    public static Action<ByteWriter> __WriteAgent;
    internal static void New_0(Link __link, byte[] data, int position)
    {
        if (__link == null || !__link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)1);
        __writer.Write("New");
        __writer.WriteBytes(data, 0, position);
        __link.Write(__writer.Buffer, 0, __writer.Position);
    }
    internal static void Delete_1(Link __link, byte[] data, int position)
    {
        if (__link == null || !__link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)1);
        __writer.Write("Delete");
        __writer.WriteBytes(data, 0, position);
        __link.Write(__writer.Buffer, 0, __writer.Position);
    }
    internal static void Launch_2(Link __link, byte[] data, int position)
    {
        if (__link == null || !__link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)1);
        __writer.Write("Launch");
        __writer.WriteBytes(data, 0, position);
        __link.Write(__writer.Buffer, 0, __writer.Position);
    }
    internal static void Update_3(Link __link, byte[] data, int position)
    {
        if (__link == null || !__link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)1);
        __writer.Write("Update");
        __writer.WriteBytes(data, 0, position);
        __link.Write(__writer.Buffer, 0, __writer.Position);
    }
    internal static void Stop_4(Link __link, byte[] data, int position)
    {
        if (__link == null || !__link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)1);
        __writer.Write("Stop");
        __writer.WriteBytes(data, 0, position);
        __link.Write(__writer.Buffer, 0, __writer.Position);
    }
    internal static void GetCommands_5(Link __link, byte[] data, int position)
    {
        if (__link == null || !__link.IsConnected) return;
        ByteWriter __writer = new ByteWriter();
        if (__WriteAgent != null) __WriteAgent(__writer);
        __writer.Write((byte)1);
        __writer.Write("GetCommands");
        __writer.WriteBytes(data, 0, position);
        __link.Write(__writer.Buffer, 0, __writer.Position);
    }
}

class IManagerCallStub : Stub
{
    public _IManagerCall __Agent;
    public Func<_IManagerCall> __GetAgent;
    public Func<ByteReader, _IManagerCall> __ReadAgent;
    public IManagerCallStub(_IManagerCall agent) : base(1)
    {
        this.__Agent = agent;
        AddMethod("New", New);
        AddMethod("Delete", Delete);
        AddMethod("Launch", Launch);
        AddMethod("Update", Update);
        AddMethod("Stop", Stop);
        AddMethod("GetCommands", GetCommands);
        AddMethod("CallCommand", CallCommand);
        AddMethod("UpdateSVN", UpdateSVN);
        AddMethod("ServiceTypeUpdate", ServiceTypeUpdate);
        AddMethod("SetLaunchCommand", SetLaunchCommand);
        AddMethod("UpdateLauncher", UpdateLauncher);
    }
    public IManagerCallStub(Func<_IManagerCall> agent) : this((_IManagerCall)null)
    {
        this.__GetAgent = agent;
    }
    public IManagerCallStub(Func<ByteReader, _IManagerCall> agent) : this((_IManagerCall)null)
    {
        this.__ReadAgent = agent;
    }
    void New(ByteReader __stream)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) agent = temp; }
        LauncherProtocolStructure.ServiceType serviceType;
        string name;
        byte callback;
        __stream.Read(out serviceType);
        __stream.Read(out name);
        __stream.Read(out callback);
        #if DEBUG
        _LOG.Debug("New serviceType: {0}, name: {1}, callback: {2}", JsonWriter.Serialize(serviceType), name, "System.Action<LauncherProtocolStructure.Service>");
        #endif
        var __callback = new CBIManagerCall_New(callback, Link);
        try
        {
            agent.New(serviceType, name, __callback);
        }
        catch (Exception ex)
        {
            _LOG.Error("Callback_New error! msg={0} stack={1}", ex.Message, ex.StackTrace);
            if (!__callback.IsCallback) __callback.Error(-1, ex.Message);
        }
    }
    void Delete(ByteReader __stream)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) agent = temp; }
        string name;
        byte callback;
        __stream.Read(out name);
        __stream.Read(out callback);
        #if DEBUG
        _LOG.Debug("Delete name: {0}, callback: {1}", name, "System.Action");
        #endif
        var __callback = new CBIManagerCall_Delete(callback, Link);
        try
        {
            agent.Delete(name, __callback);
        }
        catch (Exception ex)
        {
            _LOG.Error("Callback_Delete error! msg={0} stack={1}", ex.Message, ex.StackTrace);
            if (!__callback.IsCallback) __callback.Error(-1, ex.Message);
        }
    }
    void Launch(ByteReader __stream)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) agent = temp; }
        string name;
        byte callback;
        __stream.Read(out name);
        __stream.Read(out callback);
        #if DEBUG
        _LOG.Debug("Launch name: {0}, callback: {1}", name, "System.Action");
        #endif
        var __callback = new CBIManagerCall_Launch(callback, Link);
        try
        {
            agent.Launch(name, __callback);
        }
        catch (Exception ex)
        {
            _LOG.Error("Callback_Launch error! msg={0} stack={1}", ex.Message, ex.StackTrace);
            if (!__callback.IsCallback) __callback.Error(-1, ex.Message);
        }
    }
    void Update(ByteReader __stream)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) agent = temp; }
        string name;
        byte callback;
        __stream.Read(out name);
        __stream.Read(out callback);
        #if DEBUG
        _LOG.Debug("Update name: {0}, callback: {1}", name, "System.Action<int>");
        #endif
        var __callback = new CBIManagerCall_Update(callback, Link);
        try
        {
            agent.Update(name, __callback);
        }
        catch (Exception ex)
        {
            _LOG.Error("Callback_Update error! msg={0} stack={1}", ex.Message, ex.StackTrace);
            if (!__callback.IsCallback) __callback.Error(-1, ex.Message);
        }
    }
    void Stop(ByteReader __stream)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) agent = temp; }
        string name;
        byte callback;
        __stream.Read(out name);
        __stream.Read(out callback);
        #if DEBUG
        _LOG.Debug("Stop name: {0}, callback: {1}", name, "System.Action");
        #endif
        var __callback = new CBIManagerCall_Stop(callback, Link);
        try
        {
            agent.Stop(name, __callback);
        }
        catch (Exception ex)
        {
            _LOG.Error("Callback_Stop error! msg={0} stack={1}", ex.Message, ex.StackTrace);
            if (!__callback.IsCallback) __callback.Error(-1, ex.Message);
        }
    }
    void GetCommands(ByteReader __stream)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) agent = temp; }
        string name;
        byte callback;
        __stream.Read(out name);
        __stream.Read(out callback);
        #if DEBUG
        _LOG.Debug("GetCommands name: {0}, callback: {1}", name, "System.Action<System.Collections.Generic.List<string>>");
        #endif
        var __callback = new CBIManagerCall_GetCommands(callback, Link);
        try
        {
            agent.GetCommands(name, __callback);
        }
        catch (Exception ex)
        {
            _LOG.Error("Callback_GetCommands error! msg={0} stack={1}", ex.Message, ex.StackTrace);
            if (!__callback.IsCallback) __callback.Error(-1, ex.Message);
        }
    }
    void CallCommand(ByteReader __stream)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) agent = temp; }
        string name;
        string command;
        __stream.Read(out name);
        __stream.Read(out command);
        #if DEBUG
        _LOG.Debug("CallCommand name: {0}, command: {1}", name, command);
        #endif
        agent.CallCommand(name, command);
    }
    void UpdateSVN(ByteReader __stream)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) agent = temp; }
        #if DEBUG
        _LOG.Debug("UpdateSVN");
        #endif
        agent.UpdateSVN();
    }
    void ServiceTypeUpdate(ByteReader __stream)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) agent = temp; }
        LauncherProtocolStructure.ServiceType type;
        __stream.Read(out type);
        #if DEBUG
        _LOG.Debug("ServiceTypeUpdate type: {0}", JsonWriter.Serialize(type));
        #endif
        agent.ServiceTypeUpdate(type);
    }
    void SetLaunchCommand(ByteReader __stream)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) agent = temp; }
        string name;
        string exe;
        string command;
        __stream.Read(out name);
        __stream.Read(out exe);
        __stream.Read(out command);
        #if DEBUG
        _LOG.Debug("SetLaunchCommand name: {0}, exe: {1}, command: {2}", name, exe, command);
        #endif
        agent.SetLaunchCommand(name, exe, command);
    }
    void UpdateLauncher(ByteReader __stream)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__stream); if (temp != null) agent = temp; }
        #if DEBUG
        _LOG.Debug("UpdateLauncher");
        #endif
        agent.UpdateLauncher();
    }
}
