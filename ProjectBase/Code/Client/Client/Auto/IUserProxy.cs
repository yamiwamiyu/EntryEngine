using System;
using System.Collections.Generic;
using EntryEngine;
using EntryEngine.Network;
using EntryEngine.Serialize;

using System.Linq;
using System.Text;
using System.Net;
class IUserProxy
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
    
    /// <see cref="IUser.GetUserInfo(System.Action{T_USER})"></see>
    public AsyncData<string> GetUserInfo(System.Action<T_USER> callback)
    {
        var parameters = new StringBuilder();
        return send("2/GetUserInfo", parameters.ToString(), callback);
    }
    
}
