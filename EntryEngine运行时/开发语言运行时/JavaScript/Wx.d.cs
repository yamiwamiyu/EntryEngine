#if WX

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class RequestTask
{
    public abstract void abort();
}
public class RequestObject
{
    public string url;
    public object data;
    public object header;
    public string method = "GET";
    public string dataType;
    public string responseType = "text";
    public Action<RequestObjectRes> success;
    public Action<RequestObjectRes> fail;
    public Action<RequestObjectRes> complete;
}
public class RequestObjectRes
{
    public object data;
    public int statusCode;
    public object header;
}
public static class wx
{
    public extern static RequestTask request(RequestObject param);
    public extern static void setStorageSync(string key, object data);
    public extern static void removeStorageSync(string key);
    public extern static object getStorageSync(string key);
    public extern static void clearStorageSync();
}

#endif