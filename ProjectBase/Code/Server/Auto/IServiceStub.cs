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
    void SendSMSCode(string telphone, CBIService_SendSMSCode callback);
    void LoginBySMSCode(string telphone, string code, CBIService_LoginBySMSCode callback);
    void LoginByToken(string token, CBIService_LoginByToken callback);
    void LoginByPassword(string telphone, string password, CBIService_LoginByPassword callback);
    void ForgetPassword(string telphone, string code, string password, CBIService_ForgetPassword callback);
    void CenterLoginByPassword(string name, string password, CBIService_CenterLoginByPassword callback);
    void CenterLoginBySMSCode(string telphone, string code, CBIService_CenterLoginBySMSCode callback);
    void UploadImage(EntryEngine.Network.FileUpload file, CBIService_UploadImage callback);
    void UploadFile(EntryEngine.Network.FileUpload file, CBIService_UploadFile callback);
    void WeChatPayCallback(HttpListenerContext __context);
    void AlipayCallback(string trade_no, string out_trade_no, string buyer_id, string buyer_logon_id, string trade_status, string total_amount, string gmt_payment, CBIService_AlipayCallback callback);
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
public class CBIService_LoginBySMSCode : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIService_LoginBySMSCode(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(T_USER obj) // INDEX = 1
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIService_LoginBySMSCode {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIService_LoginBySMSCode Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIService_LoginByToken : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIService_LoginByToken(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(T_USER obj) // INDEX = 2
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIService_LoginByToken {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIService_LoginByToken Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIService_LoginByPassword : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIService_LoginByPassword(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(T_USER obj) // INDEX = 3
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIService_LoginByPassword {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIService_LoginByPassword Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIService_ForgetPassword : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIService_ForgetPassword(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(T_USER obj) // INDEX = 4
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIService_ForgetPassword {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIService_ForgetPassword Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIService_CenterLoginByPassword : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIService_CenterLoginByPassword(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(T_CENTER_USER obj) // INDEX = 5
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIService_CenterLoginByPassword {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIService_CenterLoginByPassword Error ret={0} msg={1}", ret, msg);
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
    public void Callback(T_CENTER_USER obj) // INDEX = 6
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
public class CBIService_UploadImage : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIService_UploadImage(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(string obj) // INDEX = 7
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIService_UploadImage {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIService_UploadImage Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIService_UploadFile : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIService_UploadFile(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(string obj) // INDEX = 8
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIService_UploadFile {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIService_UploadFile Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBIService_AlipayCallback : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBIService_AlipayCallback(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(string obj) // INDEX = 10
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBIService_AlipayCallback {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBIService_AlipayCallback Error ret={0} msg={1}", ret, msg);
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
        this.Protocol = "1";
        AddMethod("SendSMSCode", SendSMSCode);
        AddMethod("LoginBySMSCode", LoginBySMSCode);
        AddMethod("LoginByToken", LoginByToken);
        AddMethod("LoginByPassword", LoginByPassword);
        AddMethod("ForgetPassword", ForgetPassword);
        AddMethod("CenterLoginByPassword", CenterLoginByPassword);
        AddMethod("CenterLoginBySMSCode", CenterLoginBySMSCode);
        AddMethod("UploadImage", UploadImage);
        AddMethod("UploadFile", UploadFile);
        AddMethod("WeChatPayCallback", WeChatPayCallback);
        AddMethod("AlipayCallback", AlipayCallback);
    }
    void SendSMSCode(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("telphone");
        string telphone = __temp;
        #if DEBUG
        _LOG.Debug("SendSMSCode telphone: {0},", telphone);
        #endif
        var callback = new CBIService_SendSMSCode(this);
        agent.SendSMSCode(telphone, callback);
    }
    void LoginBySMSCode(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("telphone");
        string telphone = __temp;
        __temp = GetParam("code");
        string code = __temp;
        #if DEBUG
        _LOG.Debug("LoginBySMSCode telphone: {0}, code: {1},", telphone, code);
        #endif
        var callback = new CBIService_LoginBySMSCode(this);
        agent.LoginBySMSCode(telphone, code, callback);
    }
    void LoginByToken(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("token");
        string token = __temp;
        #if DEBUG
        _LOG.Debug("LoginByToken token: {0},", token);
        #endif
        var callback = new CBIService_LoginByToken(this);
        agent.LoginByToken(token, callback);
    }
    void LoginByPassword(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("telphone");
        string telphone = __temp;
        __temp = GetParam("password");
        string password = __temp;
        #if DEBUG
        _LOG.Debug("LoginByPassword telphone: {0}, password: {1},", telphone, password);
        #endif
        var callback = new CBIService_LoginByPassword(this);
        agent.LoginByPassword(telphone, password, callback);
    }
    void ForgetPassword(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("telphone");
        string telphone = __temp;
        __temp = GetParam("code");
        string code = __temp;
        __temp = GetParam("password");
        string password = __temp;
        #if DEBUG
        _LOG.Debug("ForgetPassword telphone: {0}, code: {1}, password: {2},", telphone, code, password);
        #endif
        var callback = new CBIService_ForgetPassword(this);
        agent.ForgetPassword(telphone, code, password, callback);
    }
    void CenterLoginByPassword(HttpListenerContext __context)
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
        _LOG.Debug("CenterLoginByPassword name: {0}, password: {1},", name, password);
        #endif
        var callback = new CBIService_CenterLoginByPassword(this);
        agent.CenterLoginByPassword(name, password, callback);
    }
    void CenterLoginBySMSCode(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("telphone");
        string telphone = __temp;
        __temp = GetParam("code");
        string code = __temp;
        #if DEBUG
        _LOG.Debug("CenterLoginBySMSCode telphone: {0}, code: {1},", telphone, code);
        #endif
        var callback = new CBIService_CenterLoginBySMSCode(this);
        agent.CenterLoginBySMSCode(telphone, code, callback);
    }
    void UploadImage(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        EntryEngine.Network.FileUpload file = GetFile("file");
        #if DEBUG
        _LOG.Debug("UploadImage file: {0},", JsonWriter.Serialize(file));
        #endif
        var callback = new CBIService_UploadImage(this);
        agent.UploadImage(file, callback);
        file.Dispose();
    }
    void UploadFile(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        EntryEngine.Network.FileUpload file = GetFile("file");
        #if DEBUG
        _LOG.Debug("UploadFile file: {0},", JsonWriter.Serialize(file));
        #endif
        var callback = new CBIService_UploadFile(this);
        agent.UploadFile(file, callback);
        file.Dispose();
    }
    void WeChatPayCallback(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        agent.WeChatPayCallback(__context);
    }
    void AlipayCallback(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("trade_no");
        string trade_no = __temp;
        __temp = GetParam("out_trade_no");
        string out_trade_no = __temp;
        __temp = GetParam("buyer_id");
        string buyer_id = __temp;
        __temp = GetParam("buyer_logon_id");
        string buyer_logon_id = __temp;
        __temp = GetParam("trade_status");
        string trade_status = __temp;
        __temp = GetParam("total_amount");
        string total_amount = __temp;
        __temp = GetParam("gmt_payment");
        string gmt_payment = __temp;
        #if DEBUG
        _LOG.Debug("AlipayCallback trade_no: {0}, out_trade_no: {1}, buyer_id: {2}, buyer_logon_id: {3}, trade_status: {4}, total_amount: {5}, gmt_payment: {6},", trade_no, out_trade_no, buyer_id, buyer_logon_id, trade_status, total_amount, gmt_payment);
        #endif
        var callback = new CBIService_AlipayCallback(this);
        agent.AlipayCallback(trade_no, out_trade_no, buyer_id, buyer_logon_id, trade_status, total_amount, gmt_payment, callback);
    }
}
