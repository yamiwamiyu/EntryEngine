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
    void UserInfo(CBICenter_UserInfo callback);
    void UserModifyPassword(string opass, string npass, CBICenter_UserModifyPassword callback);
    void UserModifyPhone(string phone, string code, CBICenter_UserModifyPhone callback);
    void UserExitLogin(CBICenter_UserExitLogin callback);
}
public class CBICenter_UserInfo : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBICenter_UserInfo(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(T_CENTER_USER obj) // INDEX = 0
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBICenter_UserInfo {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBICenter_UserInfo Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBICenter_UserModifyPassword : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBICenter_UserModifyPassword(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 1
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBICenter_UserModifyPassword {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBICenter_UserModifyPassword Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBICenter_UserModifyPhone : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBICenter_UserModifyPhone(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 2
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBICenter_UserModifyPhone {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBICenter_UserModifyPhone Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBICenter_UserExitLogin : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBICenter_UserExitLogin(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 3
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBICenter_UserExitLogin {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBICenter_UserExitLogin Error ret={0} msg={1}", ret, msg);
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
        AddMethod("UserInfo", UserInfo);
        AddMethod("UserModifyPassword", UserModifyPassword);
        AddMethod("UserModifyPhone", UserModifyPhone);
        AddMethod("UserExitLogin", UserExitLogin);
    }
    void UserInfo(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        #if DEBUG
        _LOG.Debug("UserInfo");
        #endif
        var callback = new CBICenter_UserInfo(this);
        agent.UserInfo(callback);
    }
    void UserModifyPassword(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("opass");
        string opass = __temp;
        __temp = GetParam("npass");
        string npass = __temp;
        #if DEBUG
        _LOG.Debug("UserModifyPassword opass: {0}, npass: {1},", opass, npass);
        #endif
        var callback = new CBICenter_UserModifyPassword(this);
        agent.UserModifyPassword(opass, npass, callback);
    }
    void UserModifyPhone(HttpListenerContext __context)
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
        _LOG.Debug("UserModifyPhone phone: {0}, code: {1},", phone, code);
        #endif
        var callback = new CBICenter_UserModifyPhone(this);
        agent.UserModifyPhone(phone, code, callback);
    }
    void UserExitLogin(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        #if DEBUG
        _LOG.Debug("UserExitLogin");
        #endif
        var callback = new CBICenter_UserExitLogin(this);
        agent.UserExitLogin(callback);
    }
}
