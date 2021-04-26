using System;
using System.Collections.Generic;
using EntryEngine;
using EntryEngine.Network;
using EntryEngine.Serialize;

using System.Linq;
using System.Text;
using System.Net;
interface _Protocol1
{
    void PlayerExists(string name, CBProtocol1_PlayerExists callback);
    void Register(string name, string password, CBProtocol1_Register callback);
    void Login(string name, string password, CBProtocol1_Login callback);
}
public class CBProtocol1_PlayerExists : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBProtocol1_PlayerExists(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 0
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBProtocol1_PlayerExists {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBProtocol1_PlayerExists Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBProtocol1_Register : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBProtocol1_Register(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(T_PLAYER obj) // INDEX = 1
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBProtocol1_Register {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBProtocol1_Register Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBProtocol1_Login : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBProtocol1_Login(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(T_PLAYER obj) // INDEX = 2
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBProtocol1_Login {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBProtocol1_Login Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}

class Protocol1Stub : StubHttp
{
    public Action<HttpListenerContext, object> __AutoCallback;
    public _Protocol1 __Agent;
    public Func<_Protocol1> __GetAgent;
    public Func<HttpListenerContext, _Protocol1> __ReadAgent;
    public Protocol1Stub(_Protocol1 agent)
    {
        this.__Agent = agent;
        this.Protocol = "1";
        AddMethod("PlayerExists", PlayerExists);
        AddMethod("Register", Register);
        AddMethod("Login", Login);
    }
    void PlayerExists(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("name");
        string name = __temp;
        #if DEBUG
        _LOG.Debug("PlayerExists name: {0},", name);
        #endif
        var callback = new CBProtocol1_PlayerExists(this);
        agent.PlayerExists(name, callback);
    }
    void Register(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("name");
        string name = __temp;
        __temp = GetParam("password");
        string password = __temp;
        #if DEBUG
        _LOG.Debug("Register name: {0}, password: {1},", name, password);
        #endif
        var callback = new CBProtocol1_Register(this);
        agent.Register(name, password, callback);
    }
    void Login(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("name");
        string name = __temp;
        __temp = GetParam("password");
        string password = __temp;
        #if DEBUG
        _LOG.Debug("Login name: {0}, password: {1},", name, password);
        #endif
        var callback = new CBProtocol1_Login(this);
        agent.Login(name, password, callback);
    }
}
