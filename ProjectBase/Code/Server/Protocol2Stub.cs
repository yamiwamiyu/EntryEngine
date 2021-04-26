using System;
using System.Collections.Generic;
using EntryEngine;
using EntryEngine.Network;
using EntryEngine.Serialize;

using System.Linq;
using System.Text;
using System.Net;
interface _Protocol2
{
    void TestLogin(CBProtocol2_TestLogin callback);
}
public class CBProtocol2_TestLogin : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBProtocol2_TestLogin(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(T_PLAYER obj) // INDEX = 0
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBProtocol2_TestLogin {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBProtocol2_TestLogin Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}

class Protocol2Stub : StubHttp
{
    public Action<HttpListenerContext, object> __AutoCallback;
    public _Protocol2 __Agent;
    public Func<_Protocol2> __GetAgent;
    public Func<HttpListenerContext, _Protocol2> __ReadAgent;
    public Protocol2Stub(_Protocol2 agent)
    {
        this.__Agent = agent;
        this.Protocol = "2";
        AddMethod("TestLogin", TestLogin);
    }
    void TestLogin(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        #if DEBUG
        _LOG.Debug("TestLogin");
        #endif
        var callback = new CBProtocol2_TestLogin(this);
        agent.TestLogin(callback);
    }
}
