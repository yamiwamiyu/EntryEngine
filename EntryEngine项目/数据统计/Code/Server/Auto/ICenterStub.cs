using System;
using System.Collections.Generic;
using EntryEngine;
using EntryEngine.Network;
using EntryEngine.Serialize;

using System.Linq;
using System.Text;
using System.Net;
interface _ICenter
{
    void GetUserInfo(CBICenter_GetUserInfo callback);
}
public class CBICenter_GetUserInfo : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBICenter_GetUserInfo(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(T_CENTER_USER obj) // INDEX = 0
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBICenter_GetUserInfo {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBICenter_GetUserInfo Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}

class ICenterStub : StubHttp
{
    public Action<HttpListenerContext, object> __AutoCallback;
    public _ICenter __Agent;
    public Func<_ICenter> __GetAgent;
    public Func<HttpListenerContext, _ICenter> __ReadAgent;
    public ICenterStub(_ICenter agent)
    {
        this.__Agent = agent;
        this.Protocol = "2";
        AddMethod("GetUserInfo", GetUserInfo);
    }
    void GetUserInfo(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        #if DEBUG
        _LOG.Debug("GetUserInfo");
        #endif
        var callback = new CBICenter_GetUserInfo(this);
        agent.GetUserInfo(callback);
    }
}
