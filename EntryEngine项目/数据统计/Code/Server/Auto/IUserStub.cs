using System;
using System.Collections.Generic;
using EntryEngine;
using EntryEngine.Network;
using EntryEngine.Serialize;

using System.Linq;
using System.Text;
using System.Net;
interface _IUser
{
    void Login(string gameName, string deviceID, string channel, CBIUser_Login callback);
    void Online(int loginID, CBIUser_Online callback);
    void Analysis(int loginID, System.Collections.Generic.List<T_Analysis> analysis, CBIUser_Analysis callback);
}
public class CBIUser_Login : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIUser_Login(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(int obj) // INDEX = 0
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIUser_Login {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIUser_Login Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIUser_Online : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIUser_Online(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 1
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIUser_Online {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIUser_Online Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIUser_Analysis : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIUser_Analysis(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 2
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIUser_Analysis {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIUser_Analysis Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}

class IUserStub : StubHttp
{
    public Action<HttpListenerContext, object> __AutoCallback;
    public _IUser __Agent;
    public Func<_IUser> __GetAgent;
    public Func<HttpListenerContext, _IUser> __ReadAgent;
    public IUserStub(_IUser agent)
    {
        this.__Agent = agent;
        this.Protocol = "3";
        AddMethod("Login", Login);
        AddMethod("Online", Online);
        AddMethod("Analysis", Analysis);
    }
    void Login(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("gameName");
        string gameName = __temp;
        __temp = GetParam("deviceID");
        string deviceID = __temp;
        __temp = GetParam("channel");
        string channel = __temp;
        #if DEBUG
        _LOG.Debug("Login gameName: {0}, deviceID: {1}, channel: {2},", gameName, deviceID, channel);
        #endif
        var callback = new CBIUser_Login(this);
        agent.Login(gameName, deviceID, channel, callback);
    }
    void Online(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("loginID");
        int loginID = string.IsNullOrEmpty(__temp) ? default(int) : int.Parse(__temp);
        #if DEBUG
        _LOG.Debug("Online loginID: {0},", loginID);
        #endif
        var callback = new CBIUser_Online(this);
        agent.Online(loginID, callback);
    }
    void Analysis(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("loginID");
        int loginID = string.IsNullOrEmpty(__temp) ? default(int) : int.Parse(__temp);
        __temp = GetParam("analysis");
        System.Collections.Generic.List<T_Analysis> analysis = string.IsNullOrEmpty(__temp) ? default(System.Collections.Generic.List<T_Analysis>) : JsonReader.Deserialize<System.Collections.Generic.List<T_Analysis>>(__temp);
        #if DEBUG
        _LOG.Debug("Analysis loginID: {0}, analysis: {1},", loginID, JsonWriter.Serialize(analysis));
        #endif
        var callback = new CBIUser_Analysis(this);
        agent.Analysis(loginID, analysis, callback);
    }
}
