using System;
using System.Collections.Generic;
using EntryEngine;
using EntryEngine.Network;
using EntryEngine.Serialize;

using System.Linq;
using System.Text;
using System.Net;
class IServiceProxy
{
    /// <summary>请求的公共地址，例如 http://127.0.0.1:888/Action/ </summary>
    public string Host;
    /// <summary>在发送请求之前执行</summary>
    public Action<HttpRequestPost> OnSend;
    /// <summary>在回调之前执行</summary>
    public Action<string> OnCallback;
    /// <summary>接口返回HttpError实例或口返回状态码不为200(OnError有指定则仅执行OnError)时</summary>
    public Action<int, string> OnHttpError;
    /// <summary>请求抛出WebException时</summary>
    public Action<HttpRequestPost, WebException> OnError;
    /// <summary>回调函数异步还是同步执行，true为异步</summary>
    public bool IsAsync = true;
    
    private void _onError(HttpRequestPost req, WebException e, byte[] resultBytes)
    {
        AsyncData<string> result = req.Tag as AsyncData<string>;
        if (result != null) result.Error(e);
        if (OnError != null) OnError(req, e);
        else _LOG.Error(e, "发送请求错误 {0}", req.Request.RequestUri.ToString());
    }
    
    /// <summary>发送HTTP请求</summary>
    /// <param name="url">请求的地址，例如 1/GetInfo</param>
    /// <param name="data">请求的参数</param>
    /// <param name="callback">请求完成后的回调函数</param>
    private AsyncData<string> send<T>(string url, string data, Action<T> callback)
    {
        _LOG.Debug("{0} {1}", url, data);
        
        AsyncData<string> result = new AsyncData<string>();
        
        var request = new HttpRequestPost();
        request.Tag = result;
        request.Connect(Host + url);
        
        if (OnSend != null) OnSend(request);
        
        request.OnError += _onError;
        request.OnReceived += (response) =>
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string ret = _IO.ReadPreambleText(response.GetResponseStream(), Encoding.UTF8);
                
                if (ret.StartsWith("{\"errCode\""))
                {
                    HttpError err = JsonReader.Deserialize<HttpError>(ret);
                    if (OnHttpError != null)
                    {
                        OnHttpError(err.errCode, err.errMsg);
                    }
                    else
                    {
                        _LOG.Warning("未设置错误回调函数: {0} {1}", err.errCode, err.errMsg);
                    }
                    result.Error(new HttpException(err.errCode, err.errMsg));
                }
                else
                {
                    if (OnCallback != null)
                    {
                        OnCallback(ret);
                    }
                    if (callback != null)
                    {
                        T retObj = JsonReader.Deserialize<T>(ret);
                        if (IsAsync)
                        {
                            callback(retObj);
                        }
                        else
                        {
                            EntryService.Instance.Synchronize(() => callback(retObj));
                        }
                    }
                    else
                    {
                        _LOG.Warning("未设置回调函数: {0}", ret);
                    }
                    result.SetData(ret);
                }
            }
            else
            {
                result.Error(new HttpException((int)response.StatusCode, response.StatusDescription));
                if (OnHttpError != null)
                {
                    OnHttpError((int)response.StatusCode, response.StatusDescription);
                }
                else
                {
                    _LOG.Warning("未设置错误回调函数: {0} {1}", response.StatusCode, response.StatusDescription);
                }
            }
        };
        request.Send(data);
        
        return result;
    }
    
    /// <see cref="IService.SendSMSCode(string, System.Action{int})"></see>
    public AsyncData<string> SendSMSCode(string phone, System.Action<int> callback)
    {
        var parameters = new StringBuilder();
        parameters.Append("phone={0}", phone);
        return send("1/SendSMSCode", parameters.ToString(), callback);
    }
    
    /// <see cref="IService.LoginBySMSCode(string, string, System.Action{T_USER})"></see>
    public AsyncData<string> LoginBySMSCode(string phone, string code, System.Action<T_USER> callback)
    {
        var parameters = new StringBuilder();
        parameters.Append("phone={0}&", phone);
        parameters.Append("code={0}", code);
        return send("1/LoginBySMSCode", parameters.ToString(), callback);
    }
    
    /// <see cref="IService.LoginByToken(string, System.Action{T_USER})"></see>
    public AsyncData<string> LoginByToken(string token, System.Action<T_USER> callback)
    {
        var parameters = new StringBuilder();
        parameters.Append("token={0}", token);
        return send("1/LoginByToken", parameters.ToString(), callback);
    }
    
    /// <see cref="IService.LoginByPassword(string, string, System.Action{T_USER})"></see>
    public AsyncData<string> LoginByPassword(string phone, string password, System.Action<T_USER> callback)
    {
        var parameters = new StringBuilder();
        parameters.Append("phone={0}&", phone);
        parameters.Append("password={0}", password);
        return send("1/LoginByPassword", parameters.ToString(), callback);
    }
    
    /// <see cref="IService.ForgetPassword(string, string, string, System.Action{T_USER})"></see>
    public AsyncData<string> ForgetPassword(string phone, string code, string password, System.Action<T_USER> callback)
    {
        var parameters = new StringBuilder();
        parameters.Append("phone={0}&", phone);
        parameters.Append("code={0}&", code);
        parameters.Append("password={0}", password);
        return send("1/ForgetPassword", parameters.ToString(), callback);
    }
    
    /// <see cref="IService.LoginByWX(string, System.Action{T_USER})"></see>
    public AsyncData<string> LoginByWX(string code, System.Action<T_USER> callback)
    {
        var parameters = new StringBuilder();
        parameters.Append("code={0}", code);
        return send("1/LoginByWX", parameters.ToString(), callback);
    }
    
    /// <see cref="IService.ClearUserCache(int, System.Action{bool})"></see>
    public AsyncData<string> ClearUserCache(int id, System.Action<bool> callback)
    {
        var parameters = new StringBuilder();
        parameters.Append("id={0}", id);
        return send("1/ClearUserCache", parameters.ToString(), callback);
    }
    
    /// <see cref="IService.CenterLoginByPassword(string, string, System.Action{T_CENTER_USER})"></see>
    public AsyncData<string> CenterLoginByPassword(string name, string password, System.Action<T_CENTER_USER> callback)
    {
        var parameters = new StringBuilder();
        parameters.Append("name={0}&", name);
        parameters.Append("password={0}", password);
        return send("1/CenterLoginByPassword", parameters.ToString(), callback);
    }
    
    /// <see cref="IService.CenterLoginBySMSCode(string, string, System.Action{T_CENTER_USER})"></see>
    public AsyncData<string> CenterLoginBySMSCode(string phone, string code, System.Action<T_CENTER_USER> callback)
    {
        var parameters = new StringBuilder();
        parameters.Append("phone={0}&", phone);
        parameters.Append("code={0}", code);
        return send("1/CenterLoginBySMSCode", parameters.ToString(), callback);
    }
    
    /// <see cref="IService.UploadImage(EntryEngine.Network.FileUpload, System.Action{string})"></see>
    public AsyncData<string> UploadImage(EntryEngine.Network.FileUpload file, System.Action<string> callback)
    {
        var parameters = new StringBuilder();
        parameters.Append("file={0}", JsonWriter.Serialize(file));
        return send("1/UploadImage", parameters.ToString(), callback);
    }
    
    /// <see cref="IService.UploadFile(EntryEngine.Network.FileUpload, System.Action{string})"></see>
    public AsyncData<string> UploadFile(EntryEngine.Network.FileUpload file, System.Action<string> callback)
    {
        var parameters = new StringBuilder();
        parameters.Append("file={0}", JsonWriter.Serialize(file));
        return send("1/UploadFile", parameters.ToString(), callback);
    }
    
    /// <see cref="IService.WeChatPayCallback()"></see>
    public void WeChatPayCallback()
    {
        var parameters = new StringBuilder();
        send("1/WeChatPayCallback", parameters.ToString(), (Action<string>)null);
    }
    
    /// <see cref="IService.WeChatRefundCallback()"></see>
    public void WeChatRefundCallback()
    {
        var parameters = new StringBuilder();
        send("1/WeChatRefundCallback", parameters.ToString(), (Action<string>)null);
    }
    
    /// <see cref="IService.WXJSSDK(string, System.Action{WXJSSDK})"></see>
    public AsyncData<string> WXJSSDK(string url, System.Action<WXJSSDK> callback)
    {
        var parameters = new StringBuilder();
        parameters.Append("url={0}", url);
        return send("1/WXJSSDK", parameters.ToString(), callback);
    }
    
    /// <see cref="IService.AlipayCallback(string, string, string, string, string, string, string, System.Action{string})"></see>
    public AsyncData<string> AlipayCallback(string trade_no, string out_trade_no, string buyer_id, string buyer_logon_id, string trade_status, string total_amount, string gmt_payment, System.Action<string> callback)
    {
        var parameters = new StringBuilder();
        parameters.Append("trade_no={0}&", trade_no);
        parameters.Append("out_trade_no={0}&", out_trade_no);
        parameters.Append("buyer_id={0}&", buyer_id);
        parameters.Append("buyer_logon_id={0}&", buyer_logon_id);
        parameters.Append("trade_status={0}&", trade_status);
        parameters.Append("total_amount={0}&", total_amount);
        parameters.Append("gmt_payment={0}", gmt_payment);
        return send("1/AlipayCallback", parameters.ToString(), callback);
    }
    
}
