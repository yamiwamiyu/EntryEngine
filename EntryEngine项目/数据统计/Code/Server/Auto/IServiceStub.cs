using System;
using System.Collections.Generic;
using EntryEngine;
using EntryEngine.Network;
using EntryEngine.Serialize;

using System.Linq;
using System.Text;
using System.Net;
interface _IService
{
    void SendSMSCode(string phone, CBIService_SendSMSCode callback);
    void Login(string account, string password, CBIService_Login callback);
    void CenterLoginBySMSCode(string phone, string code, CBIService_CenterLoginBySMSCode callback);
}
public class CBIService_SendSMSCode : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIService_SendSMSCode(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(int obj) // INDEX = 0
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIService_SendSMSCode {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIService_SendSMSCode Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIService_Login : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIService_Login(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(string obj) // INDEX = 1
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIService_Login {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIService_Login Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIService_CenterLoginBySMSCode : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIService_CenterLoginBySMSCode(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(T_CENTER_USER obj) // INDEX = 2
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIService_CenterLoginBySMSCode {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIService_CenterLoginBySMSCode Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}

class IServiceStub : StubHttp
{
    public Action<HttpListenerContext, object> __AutoCallback;
    public _IService __Agent;
    public Func<_IService> __GetAgent;
    public Func<HttpListenerContext, _IService> __ReadAgent;
    public IServiceStub(_IService agent)
    {
        this.__Agent = agent;
        this.Protocol = "4";
        AddMethod("SendSMSCode", SendSMSCode);
        AddMethod("Login", Login);
        AddMethod("CenterLoginBySMSCode", CenterLoginBySMSCode);
    }
    void SendSMSCode(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("phone");
        string phone = __temp;
        #if DEBUG
        _LOG.Debug("SendSMSCode phone: {0},", phone);
        #endif
        var callback = new CBIService_SendSMSCode(this);
        agent.SendSMSCode(phone, callback);
    }
    void Login(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("account");
        string account = __temp;
        __temp = GetParam("password");
        string password = __temp;
        #if DEBUG
        _LOG.Debug("Login account: {0}, password: {1},", account, password);
        #endif
        var callback = new CBIService_Login(this);
        agent.Login(account, password, callback);
    }
    void CenterLoginBySMSCode(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("phone");
        string phone = __temp;
        __temp = GetParam("code");
        string code = __temp;
        #if DEBUG
        _LOG.Debug("CenterLoginBySMSCode phone: {0}, code: {1},", phone, code);
        #endif
        var callback = new CBIService_CenterLoginBySMSCode(this);
        agent.CenterLoginBySMSCode(phone, code, callback);
    }
}
