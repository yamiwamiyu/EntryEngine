using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using EntryEngine;
using EntryEngine.Serialize;
using System.Security.Cryptography.X509Certificates;
using EntryEngine.Network;
using System.Net.Security;
using System.Security.Cryptography;

namespace Server
{
    public partial class _DB
    {
        // 网络请求通用方法
        public static string HttpRequest(string url, Action<string> callback, string method = "GET", Dictionary<string, string> headers = null, string postData = null, int timeout = 5000, bool keepAlive = true, bool protocalVersion10 = false, string userAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)")
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            // 基本设置
            request.Method = method;
            request.KeepAlive = keepAlive;
            request.ProtocolVersion = protocalVersion10 ? HttpVersion.Version10 : HttpVersion.Version11;
            request.UserAgent = userAgent;
            request.Timeout = timeout;
            // 头部设置
            if (headers == null)
                headers = new Dictionary<string, string>();
            if (headers.ContainsKey("Content-Type"))
            {
                request.ContentType = headers["Content-Type"];
                headers.Remove("Content-Type");
            }
            else
            {
                request.ContentType = "text/html;charset=UTF-8";
            }
            foreach (var header in headers)
                request.Headers[header.Key] = header.Value;
            // POST数据
            if (!string.IsNullOrEmpty(postData))
            {
                request.Method = "POST";
                using (var stream = request.GetRequestStream())
                {
                    byte[] data = Encoding.UTF8.GetBytes(postData);
                    stream.Write(data, 0, data.Length);
                }
            }
            // 发出请求
            if (callback == null)
            {
                var response = (HttpWebResponse)request.GetResponse();
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                    return reader.ReadToEnd();
            }
            else
            {
                request.BeginGetResponse((async) =>
                {
                    try
                    {
                        var response = request.EndGetResponse(async);
                        using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                            callback(reader.ReadToEnd());
                    }
                    catch (Exception ex)
                    {
                        _LOG.Warning("HttpRequestAsync异步请求异常：{0}\r\nStack: {1}", ex.Message, ex.StackTrace);
                    }
                }, request);
                return null;
            }
        }
        public static string HttpGet(string url, Dictionary<string, string> headers = null)
        {
            return HttpRequest(url, null, "GET", headers);
        }
        public static string HttpPost(string url, Dictionary<string, string> headers = null, string postData = null)
        {
            return HttpRequest(url, null, "POST", headers, postData);
        }
        public static IPAddress GetRemoteIP(HttpListenerContext context)
        {
            var xff = context.Request.Headers["X-Real-IP"];
            if (!string.IsNullOrWhiteSpace(xff)) return IPAddress.Parse(xff);
            else return context.Request.RemoteEndPoint.Address;
        }
        /// <summary>发送下载内容给前端</summary>
        public static void Download(HttpListenerContext __context, byte[] buffer, string filename = null, string contentType = "application/octet-stream")
        {
            __context.Response.ContentType = contentType;
            // 指定下载文件名
            __context.Response.AddHeader("Content-Disposition", string.Format("attachment; filename={0}", filename));
            __context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            __context.Response.Close();
        }
        /// <summary>发送下载内容给前端，编码格式为Response.ContentEncoding</summary>
        public static void Download(HttpListenerContext __context, string buffer, string filename = null, string contentType = "application/octet-stream")
        {
            Download(__context, __context.Response.ContentEncoding.GetBytes(buffer), filename, contentType);
        }
        /// <summary>SQL语句LIKE后使用的字符串参数，例如LIKE '李%'，就是'李'.Like(false, true)</summary>
        /// <param name="key">搜索的关键字</param>
        /// <param name="left">开头是否使用'%'通配符</param>
        /// <param name="right">结尾是否使用'%'通配符</param>
        public static string Like(this string key, bool left = true, bool right = true)
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (left) key = "%" + key;
            if (right) key = key + "%";
            return key;
        }

        // 发送验证码
        public static void SendSMSCode(T_SMSCode sms)
        {
            HttpRequest(
                string.Format("https://api.smsbao.com/sms?u=yyhlm&p=24e7bd23c11c471091ff857773b46687&m={0}&c=【知世科技】您的验证码是{1}，请不要随意告诉他人哦。",
                sms.Mobile, sms.Code), null);
        }
    }
}
