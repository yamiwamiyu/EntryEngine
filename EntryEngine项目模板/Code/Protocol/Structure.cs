using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using EntryEngine;
using EntryEngine.Network;

/// <summary>微信JSSDK wx.config 时必要的参数</summary>
public class WXJSSDK
{
    public bool debug;
    public string appId;
    public long timestamp;
    public string nonceStr;
    public string signature;
    public string url;

    public static string ToHexString(byte[] data)
    {
        const string hex_dict = "0123456789abcdef";
        StringBuilder builder = new StringBuilder();
        foreach (var x in data)
        {
            builder.Append(hex_dict[x >> 4]);
            builder.Append(hex_dict[x & 0xf]);
        }
        return builder.ToString();
    }
    /// <param name="url">虽然官方文档说了要urlEncode，实际使用是前后端都不需要</param>
    public static WXJSSDK GenerateNew(string jsapi_ticket, string url)
    {
        WXJSSDK signature = new WXJSSDK();
        signature.timestamp = Utility.UnixTimestamp;
        signature.nonceStr = Guid.NewGuid().ToString("n");
        string str1 = string.Format("jsapi_ticket={0}&noncestr={1}&timestamp={2}&url={3}", jsapi_ticket, signature.nonceStr, signature.timestamp, url);
        using (var sha1 = SHA1.Create())
            signature.signature = ToHexString(sha1.ComputeHash(Encoding.ASCII.GetBytes(str1)));
        signature.url = url;
        return signature;
    }
}
/// <summary>微信JSSDK支付时必要的参数</summary>
public class WXPay
{
    public string appId;
    public string nonceStr;
    public string paySign;
    public string prepayid;
    public string partnerid;
    public string timeStamp;
    public string package = "Sign=WXPay";
}