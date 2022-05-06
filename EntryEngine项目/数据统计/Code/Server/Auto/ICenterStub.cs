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
    void ChangePassword(string oldPassword, string newPassword, CBICenter_ChangePassword callback);
    void GetGameName(CBICenter_GetGameName callback);
    void GetChannel(string gameName, CBICenter_GetChannel callback);
    void GetAnalysisGame(CBICenter_GetAnalysisGame callback);
    void GetAnalysisLabel(string gameName, CBICenter_GetAnalysisLabel callback);
    void GetAnalysis(string gameName, string channel, string label, System.DateTime startTime, System.DateTime endTime, CBICenter_GetAnalysis callback);
    void GetRetained(int page, string gameName, string channel, System.DateTime startTime, System.DateTime endTime, CBICenter_GetRetained callback);
    void GetRetained2(int page, string gameName, string channel, System.DateTime startTime, System.DateTime endTime, CBICenter_GetRetained2 callback);
    void OnlineCount(string gameName, string channel, RetOnlineUnit unit, System.DateTime startTime, System.DateTime endTime, CBICenter_OnlineCount callback);
    void GameTime(string gameName, string channel, System.DateTime startTime, System.DateTime endTime, CBICenter_GameTime callback);
    void GetGameData(int page, string gameName, string channel, System.DateTime startTime, System.DateTime endTime, CBICenter_GetGameData callback);
    void GetGameData2(int page, string gameName, string channel, System.DateTime startTime, System.DateTime endTime, CBICenter_GetGameData2 callback);
    void GetAccountList(int page, string account, CBICenter_GetAccountList callback);
    void BindGame(int identityID, string[] gameNames, CBICenter_BindGame callback);
    void ModifyAccount(int identityID, string account, string password, EAccountType type, string nickName, CBICenter_ModifyAccount callback);
    void DeleteAccount(int identityID, CBICenter_DeleteAccount callback);
    void ChangeAccountState(int identityID, CBICenter_ChangeAccountState callback);
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
public class CBICenter_ChangePassword : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBICenter_ChangePassword(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 1
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBICenter_ChangePassword {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBICenter_ChangePassword Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBICenter_GetGameName : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBICenter_GetGameName(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(System.Collections.Generic.List<string> obj) // INDEX = 2
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBICenter_GetGameName {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBICenter_GetGameName Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBICenter_GetChannel : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBICenter_GetChannel(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(System.Collections.Generic.List<string> obj) // INDEX = 3
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBICenter_GetChannel {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBICenter_GetChannel Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBICenter_GetAnalysisGame : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBICenter_GetAnalysisGame(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(System.Collections.Generic.List<string> obj) // INDEX = 4
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBICenter_GetAnalysisGame {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBICenter_GetAnalysisGame Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBICenter_GetAnalysisLabel : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBICenter_GetAnalysisLabel(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(System.Collections.Generic.List<string> obj) // INDEX = 5
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBICenter_GetAnalysisLabel {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBICenter_GetAnalysisLabel Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBICenter_GetAnalysis : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBICenter_GetAnalysis(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(System.Collections.Generic.List<RetAnalysis> obj) // INDEX = 6
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBICenter_GetAnalysis {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBICenter_GetAnalysis Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBICenter_GetRetained : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBICenter_GetRetained(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(EntryEngine.Network.PagedModel<RetRetained> obj) // INDEX = 7
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBICenter_GetRetained {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBICenter_GetRetained Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBICenter_GetRetained2 : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBICenter_GetRetained2(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(EntryEngine.Network.PagedModel<RetRetained> obj) // INDEX = 8
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBICenter_GetRetained2 {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBICenter_GetRetained2 Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBICenter_OnlineCount : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBICenter_OnlineCount(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(RetOnline obj) // INDEX = 9
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBICenter_OnlineCount {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBICenter_OnlineCount Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBICenter_GameTime : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBICenter_GameTime(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(RetGameTime obj) // INDEX = 10
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBICenter_GameTime {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBICenter_GameTime Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBICenter_GetGameData : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBICenter_GetGameData(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(RetGameData obj) // INDEX = 11
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBICenter_GetGameData {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBICenter_GetGameData Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBICenter_GetGameData2 : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBICenter_GetGameData2(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(EntryEngine.Network.PagedModel<GameDataItem> obj) // INDEX = 12
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBICenter_GetGameData2 {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBICenter_GetGameData2 Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBICenter_GetAccountList : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBICenter_GetAccountList(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(EntryEngine.Network.PagedModel<RetAccount> obj) // INDEX = 13
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBICenter_GetAccountList {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBICenter_GetAccountList Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBICenter_BindGame : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBICenter_BindGame(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 14
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBICenter_BindGame {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBICenter_BindGame Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBICenter_ModifyAccount : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBICenter_ModifyAccount(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 15
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBICenter_ModifyAccount {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBICenter_ModifyAccount Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBICenter_DeleteAccount : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBICenter_DeleteAccount(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 16
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBICenter_DeleteAccount {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBICenter_DeleteAccount Error ret={0} msg={1}", ret, msg);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Dispose()
    {
        if (!IsCallback) Error(-2, "no callback");
    }
}
public class CBICenter_ChangeAccountState : IDisposable
{
    internal HttpListenerContext __context { get; private set; }
    internal StubHttp __link { get; private set; }
    internal bool IsCallback { get; private set; }
    public CBICenter_ChangeAccountState(StubHttp link)
    {
        this.__link = link;
        this.__context = link.Context;
    }
    public void Callback(bool obj) // INDEX = 17
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(obj);
        #if DEBUG
        _LOG.Debug("CBICenter_ChangeAccountState {0}", __ret);
        #endif
        __link.Response(__context, __ret);
        IsCallback = true;
    }
    public void Error(int ret, string msg)
    {
        if (IsCallback) return;
        string __ret = JsonWriter.Serialize(new HttpError(ret, msg));
        #if DEBUG
        _LOG.Debug("CBICenter_ChangeAccountState Error ret={0} msg={1}", ret, msg);
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
        this.Protocol = "5";
        AddMethod("GetUserInfo", GetUserInfo);
        AddMethod("ChangePassword", ChangePassword);
        AddMethod("GetGameName", GetGameName);
        AddMethod("GetChannel", GetChannel);
        AddMethod("GetAnalysisGame", GetAnalysisGame);
        AddMethod("GetAnalysisLabel", GetAnalysisLabel);
        AddMethod("GetAnalysis", GetAnalysis);
        AddMethod("GetRetained", GetRetained);
        AddMethod("GetRetained2", GetRetained2);
        AddMethod("OnlineCount", OnlineCount);
        AddMethod("GameTime", GameTime);
        AddMethod("GetGameData", GetGameData);
        AddMethod("GetGameData2", GetGameData2);
        AddMethod("GetAccountList", GetAccountList);
        AddMethod("BindGame", BindGame);
        AddMethod("ModifyAccount", ModifyAccount);
        AddMethod("DeleteAccount", DeleteAccount);
        AddMethod("ChangeAccountState", ChangeAccountState);
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
    void ChangePassword(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("oldPassword");
        string oldPassword = __temp;
        __temp = GetParam("newPassword");
        string newPassword = __temp;
        #if DEBUG
        _LOG.Debug("ChangePassword oldPassword: {0}, newPassword: {1},", oldPassword, newPassword);
        #endif
        var callback = new CBICenter_ChangePassword(this);
        agent.ChangePassword(oldPassword, newPassword, callback);
    }
    void GetGameName(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        #if DEBUG
        _LOG.Debug("GetGameName");
        #endif
        var callback = new CBICenter_GetGameName(this);
        agent.GetGameName(callback);
    }
    void GetChannel(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("gameName");
        string gameName = __temp;
        #if DEBUG
        _LOG.Debug("GetChannel gameName: {0},", gameName);
        #endif
        var callback = new CBICenter_GetChannel(this);
        agent.GetChannel(gameName, callback);
    }
    void GetAnalysisGame(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        #if DEBUG
        _LOG.Debug("GetAnalysisGame");
        #endif
        var callback = new CBICenter_GetAnalysisGame(this);
        agent.GetAnalysisGame(callback);
    }
    void GetAnalysisLabel(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("gameName");
        string gameName = __temp;
        #if DEBUG
        _LOG.Debug("GetAnalysisLabel gameName: {0},", gameName);
        #endif
        var callback = new CBICenter_GetAnalysisLabel(this);
        agent.GetAnalysisLabel(gameName, callback);
    }
    void GetAnalysis(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("gameName");
        string gameName = __temp;
        __temp = GetParam("channel");
        string channel = __temp;
        __temp = GetParam("label");
        string label = __temp;
        __temp = GetParam("startTime");
        System.DateTime startTime = string.IsNullOrEmpty(__temp) ? default(System.DateTime) : Utility.ToTime(long.Parse(__temp));
        __temp = GetParam("endTime");
        System.DateTime endTime = string.IsNullOrEmpty(__temp) ? default(System.DateTime) : Utility.ToTime(long.Parse(__temp));
        #if DEBUG
        _LOG.Debug("GetAnalysis gameName: {0}, channel: {1}, label: {2}, startTime: {3}, endTime: {4},", gameName, channel, label, JsonWriter.Serialize(startTime), JsonWriter.Serialize(endTime));
        #endif
        var callback = new CBICenter_GetAnalysis(this);
        agent.GetAnalysis(gameName, channel, label, startTime, endTime, callback);
    }
    void GetRetained(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("page");
        int page = string.IsNullOrEmpty(__temp) ? default(int) : int.Parse(__temp);
        __temp = GetParam("gameName");
        string gameName = __temp;
        __temp = GetParam("channel");
        string channel = __temp;
        __temp = GetParam("startTime");
        System.DateTime startTime = string.IsNullOrEmpty(__temp) ? default(System.DateTime) : Utility.ToTime(long.Parse(__temp));
        __temp = GetParam("endTime");
        System.DateTime endTime = string.IsNullOrEmpty(__temp) ? default(System.DateTime) : Utility.ToTime(long.Parse(__temp));
        #if DEBUG
        _LOG.Debug("GetRetained page: {0}, gameName: {1}, channel: {2}, startTime: {3}, endTime: {4},", page, gameName, channel, JsonWriter.Serialize(startTime), JsonWriter.Serialize(endTime));
        #endif
        var callback = new CBICenter_GetRetained(this);
        agent.GetRetained(page, gameName, channel, startTime, endTime, callback);
    }
    void GetRetained2(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("page");
        int page = string.IsNullOrEmpty(__temp) ? default(int) : int.Parse(__temp);
        __temp = GetParam("gameName");
        string gameName = __temp;
        __temp = GetParam("channel");
        string channel = __temp;
        __temp = GetParam("startTime");
        System.DateTime startTime = string.IsNullOrEmpty(__temp) ? default(System.DateTime) : Utility.ToTime(long.Parse(__temp));
        __temp = GetParam("endTime");
        System.DateTime endTime = string.IsNullOrEmpty(__temp) ? default(System.DateTime) : Utility.ToTime(long.Parse(__temp));
        #if DEBUG
        _LOG.Debug("GetRetained2 page: {0}, gameName: {1}, channel: {2}, startTime: {3}, endTime: {4},", page, gameName, channel, JsonWriter.Serialize(startTime), JsonWriter.Serialize(endTime));
        #endif
        var callback = new CBICenter_GetRetained2(this);
        agent.GetRetained2(page, gameName, channel, startTime, endTime, callback);
    }
    void OnlineCount(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("gameName");
        string gameName = __temp;
        __temp = GetParam("channel");
        string channel = __temp;
        __temp = GetParam("unit");
        RetOnlineUnit unit = string.IsNullOrEmpty(__temp) ? default(RetOnlineUnit) : (RetOnlineUnit)int.Parse(__temp);
        __temp = GetParam("startTime");
        System.DateTime startTime = string.IsNullOrEmpty(__temp) ? default(System.DateTime) : Utility.ToTime(long.Parse(__temp));
        __temp = GetParam("endTime");
        System.DateTime endTime = string.IsNullOrEmpty(__temp) ? default(System.DateTime) : Utility.ToTime(long.Parse(__temp));
        #if DEBUG
        _LOG.Debug("OnlineCount gameName: {0}, channel: {1}, unit: {2}, startTime: {3}, endTime: {4},", gameName, channel, unit, JsonWriter.Serialize(startTime), JsonWriter.Serialize(endTime));
        #endif
        var callback = new CBICenter_OnlineCount(this);
        agent.OnlineCount(gameName, channel, unit, startTime, endTime, callback);
    }
    void GameTime(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("gameName");
        string gameName = __temp;
        __temp = GetParam("channel");
        string channel = __temp;
        __temp = GetParam("startTime");
        System.DateTime startTime = string.IsNullOrEmpty(__temp) ? default(System.DateTime) : Utility.ToTime(long.Parse(__temp));
        __temp = GetParam("endTime");
        System.DateTime endTime = string.IsNullOrEmpty(__temp) ? default(System.DateTime) : Utility.ToTime(long.Parse(__temp));
        #if DEBUG
        _LOG.Debug("GameTime gameName: {0}, channel: {1}, startTime: {2}, endTime: {3},", gameName, channel, JsonWriter.Serialize(startTime), JsonWriter.Serialize(endTime));
        #endif
        var callback = new CBICenter_GameTime(this);
        agent.GameTime(gameName, channel, startTime, endTime, callback);
    }
    void GetGameData(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("page");
        int page = string.IsNullOrEmpty(__temp) ? default(int) : int.Parse(__temp);
        __temp = GetParam("gameName");
        string gameName = __temp;
        __temp = GetParam("channel");
        string channel = __temp;
        __temp = GetParam("startTime");
        System.DateTime startTime = string.IsNullOrEmpty(__temp) ? default(System.DateTime) : Utility.ToTime(long.Parse(__temp));
        __temp = GetParam("endTime");
        System.DateTime endTime = string.IsNullOrEmpty(__temp) ? default(System.DateTime) : Utility.ToTime(long.Parse(__temp));
        #if DEBUG
        _LOG.Debug("GetGameData page: {0}, gameName: {1}, channel: {2}, startTime: {3}, endTime: {4},", page, gameName, channel, JsonWriter.Serialize(startTime), JsonWriter.Serialize(endTime));
        #endif
        var callback = new CBICenter_GetGameData(this);
        agent.GetGameData(page, gameName, channel, startTime, endTime, callback);
    }
    void GetGameData2(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("page");
        int page = string.IsNullOrEmpty(__temp) ? default(int) : int.Parse(__temp);
        __temp = GetParam("gameName");
        string gameName = __temp;
        __temp = GetParam("channel");
        string channel = __temp;
        __temp = GetParam("startTime");
        System.DateTime startTime = string.IsNullOrEmpty(__temp) ? default(System.DateTime) : Utility.ToTime(long.Parse(__temp));
        __temp = GetParam("endTime");
        System.DateTime endTime = string.IsNullOrEmpty(__temp) ? default(System.DateTime) : Utility.ToTime(long.Parse(__temp));
        #if DEBUG
        _LOG.Debug("GetGameData2 page: {0}, gameName: {1}, channel: {2}, startTime: {3}, endTime: {4},", page, gameName, channel, JsonWriter.Serialize(startTime), JsonWriter.Serialize(endTime));
        #endif
        var callback = new CBICenter_GetGameData2(this);
        agent.GetGameData2(page, gameName, channel, startTime, endTime, callback);
    }
    void GetAccountList(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("page");
        int page = string.IsNullOrEmpty(__temp) ? default(int) : int.Parse(__temp);
        __temp = GetParam("account");
        string account = __temp;
        #if DEBUG
        _LOG.Debug("GetAccountList page: {0}, account: {1},", page, account);
        #endif
        var callback = new CBICenter_GetAccountList(this);
        agent.GetAccountList(page, account, callback);
    }
    void BindGame(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("identityID");
        int identityID = string.IsNullOrEmpty(__temp) ? default(int) : int.Parse(__temp);
        __temp = GetParam("gameNames");
        string[] gameNames = string.IsNullOrEmpty(__temp) ? default(string[]) : JsonReader.Deserialize<string[]>(__temp);
        #if DEBUG
        _LOG.Debug("BindGame identityID: {0}, gameNames: {1},", identityID, JsonWriter.Serialize(gameNames));
        #endif
        var callback = new CBICenter_BindGame(this);
        agent.BindGame(identityID, gameNames, callback);
    }
    void ModifyAccount(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("identityID");
        int identityID = string.IsNullOrEmpty(__temp) ? default(int) : int.Parse(__temp);
        __temp = GetParam("account");
        string account = __temp;
        __temp = GetParam("password");
        string password = __temp;
        __temp = GetParam("type");
        EAccountType type = string.IsNullOrEmpty(__temp) ? default(EAccountType) : (EAccountType)byte.Parse(__temp);
        __temp = GetParam("nickName");
        string nickName = __temp;
        #if DEBUG
        _LOG.Debug("ModifyAccount identityID: {0}, account: {1}, password: {2}, type: {3}, nickName: {4},", identityID, account, password, type, nickName);
        #endif
        var callback = new CBICenter_ModifyAccount(this);
        agent.ModifyAccount(identityID, account, password, type, nickName, callback);
    }
    void DeleteAccount(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("identityID");
        int identityID = string.IsNullOrEmpty(__temp) ? default(int) : int.Parse(__temp);
        #if DEBUG
        _LOG.Debug("DeleteAccount identityID: {0},", identityID);
        #endif
        var callback = new CBICenter_DeleteAccount(this);
        agent.DeleteAccount(identityID, callback);
    }
    void ChangeAccountState(HttpListenerContext __context)
    {
        var agent = __Agent;
        if (__GetAgent != null) { var temp = __GetAgent(); if (temp != null) agent = temp; }
        if (__ReadAgent != null) { var temp = __ReadAgent(__context); if (temp != null) agent = temp; }
        string __temp;
        __temp = GetParam("identityID");
        int identityID = string.IsNullOrEmpty(__temp) ? default(int) : int.Parse(__temp);
        #if DEBUG
        _LOG.Debug("ChangeAccountState identityID: {0},", identityID);
        #endif
        var callback = new CBICenter_ChangeAccountState(this);
        agent.ChangeAccountState(identityID, callback);
    }
}
