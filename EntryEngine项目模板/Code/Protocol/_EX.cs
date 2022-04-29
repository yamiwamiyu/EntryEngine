using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using EntryEngine.Network;

/// <summary>扩展方法</summary>
public static class _EX
{
    public static void Check(this string message, bool isThrow)
    {
        if (isThrow)
            throw new HttpException(400, message);
    }
    public static string Mask(this string str)
    {
        return Mask(str, 3, 4);
    }
    public static string Mask(this string str, int prevLen, int lastLen)
    {
        if (string.IsNullOrEmpty(str))
            return str;
        else
        {
            int len = prevLen + lastLen;
            if (str.Length <= len)
                return str;
            StringBuilder builder = new StringBuilder();
            builder.Append(str.Substring(0, prevLen));
            for (int i = 0, n = str.Length - lastLen - prevLen; i < n; i++)
                builder.Append('*');
            builder.Append(str.Substring(str.Length - lastLen));
            return builder.ToString();
        }
    }
    public static IPAddress ToIP(this int ip)
    {
        return new IPAddress(BitConverter.GetBytes(ip));
    }
    public static int ToIP(this string ip)
    {
        return BitConverter.ToInt32(IPAddress.Parse(ip).GetAddressBytes(), 0);
    }
    public static U TryAdd<T, U>(this Dictionary<T, U> dic, T key, Action<U> setKey) where U : new()
    {
        U result;
        if (!dic.TryGetValue(key, out result))
        {
            result = new U();
            if (setKey != null)
                setKey(result);
            dic.Add(key, result);
        }
        return result;
    }
    public static void Add<T, U>(this Dictionary<T, List<U>> dic, Func<U, T> v2k, U item)
    {
        List<U> temp;
        T key = v2k(item);
        if (!dic.TryGetValue(key, out temp))
        {
            temp = new List<U>();
            dic.Add(key, temp);
        }
        temp.Add(item);
    }
    public static void Add<T, U>(this Dictionary<T, List<U>> dic, Func<U, T> v2k, params U[] items)
    {
        Add(dic, v2k, (IEnumerable<U>)items);
    }
    public static void Add<T, U>(this Dictionary<T, List<U>> dic, Func<U, T> v2k, IEnumerable<U> items)
    {
        List<U> temp;
        foreach (var item in items)
        {
            T key = v2k(item);
            if (!dic.TryGetValue(key, out temp))
            {
                temp = new List<U>();
                dic.Add(key, temp);
            }
            temp.Add(item);
        }
    }
}

