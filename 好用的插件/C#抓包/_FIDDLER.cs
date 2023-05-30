using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fiddler;
using System.Reflection;
using Microsoft.Win32;
using System.Diagnostics;

public static class _FIDDLER
{
    public static bool TrustRootCert()
    {
        //CertMaker.removeFiddlerGeneratedCerts();
        bool exists = CertMaker.rootCertExists();
        //bool trust = CertMaker.rootCertIsMachineTrusted();
        bool trust = CertMaker.rootCertIsTrusted();
        if (!exists)
            exists = CertMaker.createRootCert();
        if (!trust)
            trust = CertMaker.trustRootCert();
        return exists && trust;
    }

    static FieldInfo BeforeRequest;
    static FieldInfo BeforeResponse;
    public static void SetListenEvent(SessionStateHandler request, SessionStateHandler response)
    {
        if (BeforeRequest == null)
        {
            var fields = typeof(FiddlerApplication).GetFields(BindingFlags.NonPublic | BindingFlags.Static);
            BeforeRequest = fields[5];
            BeforeResponse = fields[6];
        }
        BeforeRequest.SetValue(null, null);
        BeforeResponse.SetValue(null, null);
        if (request != null)
            FiddlerApplication.BeforeRequest += request;
        if (response != null)
            FiddlerApplication.BeforeResponse += response;
    }
    public static void SetListenEventAndStart(SessionStateHandler request, SessionStateHandler response)
    {
        SetListenEvent(request, response);
        if (FiddlerApplication.IsStarted())
            FiddlerApplication.Shutdown();
        FiddlerApplication.Startup(8888, true, true, false);
    }

    public static SessionStateHandler FiddlerApplication_NoImage(string urlKeyword)
    {
        return (session) =>
        {
            if (session.url.Contains(urlKeyword))
            {
                session.oRequest.FailSession(404, string.Empty, string.Empty);
            }
        };
    }
    public static SessionStateHandler FiddlerApplication_ReplaceCookie(string token, string uid)
    {
        return (oSession) =>
        {
            if (oSession.url.StartsWith("mobile.yangkeduo.com/order_checkout.html")
                //|| oSession.url.StartsWith("mobile.yangkeduo.com/chat_detail.html")
                || oSession.url.StartsWith("mobile.yangkeduo.com/goods")
            )
            {
                string cookie = oSession.oRequest.headers["COOKIE"];
                bool uid = false;
                bool token = false;
                Dictionary<string, string> dic = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(cookie))
                {
                    string[] cookies = cookie.Split(';');

                    for (int i = 0; i < cookies.Length; i++)
                    {
                        string[] acookie = cookies[i].Trim().Split('=');
                        if (acookie[0] == "pdd_user_id")
                        {
                            uid = true;
                            dic[acookie[0]] = uid;
                        }
                        else if (acookie[0] == "PDDAccessToken")
                        {
                            token = true;
                            dic[acookie[0]] = token;
                        }
                        else
                        {
                            dic[acookie[0]] = acookie[1];
                        }
                    }
                }
                if (!uid) dic.Add("pdd_user_id", uid);
                if (!token) dic.Add("PDDAccessToken", token);

                StringBuilder builder = new StringBuilder();
                foreach (var item in dic)
                    builder.AppendFormat("{0}={1};", item.Key, item.Value);
                oSession.oRequest.headers["COOKIE"] = builder.ToString();
            }
        };

    }
    public static Process StartChrome(string url)
    {
        Process chrome = null;
        try
        {
            // 64位注册表路径
            var openKey = @"SOFTWARE\Wow6432Node\Google\Chrome";
            if (IntPtr.Size == 4)
            {
                // 32位注册表路径
                openKey = @"SOFTWARE\Google\Chrome";
            }
            RegistryKey appPath = Registry.LocalMachine.OpenSubKey(openKey);
            chrome = Process.GetProcesses().FirstOrDefault(p => p.ProcessName == "chrome");
            var temp = Process.Start("chrome.exe", url);
            if (chrome == null) chrome = temp;
        }
        catch (Exception ex)
        {
            _LOG.Error(ex, "未能正确打开chrome浏览器");
            return null;
        }
        return chrome;
    }
}
