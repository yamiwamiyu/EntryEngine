using System;
using System.Text;
using System.Collections.Generic;

public partial class @_float
{
    //public const float NaN = Number.NaN;
    public static bool IsNaN(float f)
    {
        return window.isNaN(f);
    }
}
public partial class @_double
{
    public static bool IsNaN(double f)
    {
        return window.isNaN(f);
    }
}
public partial class @_object : Object_ { }
public partial class @_string : String
{
    public extern int Length { get; }
    public char this[int index] { get { return (char)this.charCodeAt(index); } }
    //public int IndexOf(string value)
    //{
    //    return indexOf(value);
    //}
    //public int IndexOf(string value, int startIndex)
    //{
    //    return indexOf(value, startIndex);
    //}
    public string Substring(int startIndex, int length)
    {
        return this.substring(startIndex, startIndex + length);
    }
    //public string[] Split(params char[] separator)
    //{
    //    return this.split(new string(separator));
    //}
    //public string[] Split(char[] separator, StringSplitOptions options)
    //{
    //    string[] values = this.split(new string(separator));
    //    if (options == StringSplitOptions.RemoveEmptyEntries)
    //    {
    //        int emptyCount = 0;
    //        int len = values.Length;
    //        for (int i = 0; i < len; i++)
    //            if (string.IsNullOrEmpty(values[i]))
    //                emptyCount++;
    //        if (emptyCount == 0)
    //            return values;
    //        if (emptyCount == len)
    //            return new string[0];
    //        string[] result = new string[len - emptyCount];
    //        int index = 0;
    //        for (int i = 0; i < len; i++)
    //            if (!string.IsNullOrEmpty(values[i]))
    //                result[index++] = values[i];
    //        return result;
    //    }
    //    return values;
    //}
    // JS的正则表达式替换\\会出错，替换'\\'需要写成new RegExp("\\\\", "gm")
    public string Replace(string oldValue, string newValue)
    {
        //return replace(new RegExp(oldValue, "gm"), newValue);
        return replace(new RegExp(oldValue, "g"), newValue);
    }
    //internal static string Create(char[] chars)
    //{
    //    // 处理成n的倍数次n长字符串，余数部分一个一个+=
    //    int n = chars.Length;
    //    int count = n / 5;
    //    int count2 = n % 5;
    //    string[] strs = new string[count + count2];
    //    int index = 0;
    //    for (int i = 0; i < n; i += 5)
    //        strs[index++] = String.fromCharCode(chars[i], chars[i + 1], chars[i + 2], chars[i + 3], chars[i + 4]);
    //    for (int i = count * 5; i < n; i++)
    //        strs[index] = String.fromCharCode(chars[i]);
    //    return strs.join("");

    //    //string str = "";
    //    //for (int i = 0, n = chars.Length; i < n; i += 3)
    //    //{
    //    //    if (i + 3 <= n)
    //    //    {
    //    //        str += String.fromCharCode(chars[i], chars[i + 1], chars[i + 2]);
    //    //    }
    //    //    else
    //    //    {
    //    //        if (i + 2 <= n)
    //    //        {
    //    //            str += String.fromCharCode(chars[i], chars[i + 1]);
    //    //        }
    //    //        else
    //    //        {
    //    //            str += String.fromCharCode(chars[i]);
    //    //        }
    //    //    }
    //    //}
    //    //return str;
    //}

    public extern int IndexOf(string str);
    // IndexOf改名为JS类型的indexOf
    public extern int IndexOf(string str, int index);
    public extern override string ToString();
#if !DEBUG
    public static string Join(string separator, string[] values)
    {
        return values.join(separator);
    }
#endif
}

namespace __System
{
    public partial struct DateTime
    {
        private static long __offset = 0;
        public static long DateTimeNow
        {
            get
            {
                if (__offset == 0)
                {
                    var now = new Date(0);
                    // 例如中国，new Date(0)的时间打印出来就是1970-1-1 8:00:00
                    now.setHours(0);
                    __offset = 62135596800000L - (now.getTime() - new Date(0).getTime());
                }
                return Date.now() + __offset;
            }
        }
        public static long DateTimeUtcNow
        {
            get
            {
                var date = new Date();
                return Date.UTC(date.getFullYear(), date.getMonth(), date.getDate(), date.getHours(), date.getMinutes(), date.getSeconds(), date.getMilliseconds());
            }
        }
    }
    // 直接将方法名重命名了，不需要再声明方法
    //public partial class Math : window.Math
    //{
    //    public static double Sqrt(double d) { return sqrt(d); }
    //    public static double Acos(double d) { return acos(d); }
    //    public static double Asin(double d) { return asin(d); }
    //    public static double Atan(double d) { return atan(d); }
    //    public static double Atan2(double y, double x) { return atan2(y, x); }
    //    public static double Tan(double d) { return tan(d); }
    //    public static double Sin(double d) { return sin(d); }
    //    public static double Cos(double d) { return cos(d); }
    //    public static double Pow(double x, double y) { return pow(x, y); }
    //}
    public static partial class Convert
    {
        public static object ChangeType(object value, Type type)
        {
            if (value is string)
            {
                string str = (string)value;
                if (type == typeof(bool)) return bool.Parse(str);
                else if (type == typeof(string)) return str;
                else return double.Parse(str);
            }
            else
            {
                if (type == typeof(string))
                {
                    if (value == null)
                        return null;
                    else
                        return value.ToString();
                }
            }
            return value;
        }
    }
    public abstract partial class @_Array : Array_
    {
        // 代码生成时，直接将Length属性改为了JS代码的length字段
        //public int Length { get { return this.length; } }
    }
    public partial class Exception : Error
    {
        private string msg;
        public virtual string Message
        {
            get { return string.IsNullOrEmpty(msg) ? this.message : msg; }
            private set { msg = value; }
        }
        public string StackTrace { get { return this.stack; } }
    }
}

namespace __System.IO
{
    public static partial class Path
    {
        public static string GetFullPath(string path) { return path; }
    }
    public static partial class File
    {
        public static bool Exists(string path) { return window.localStorage.getItem(path) != null; }
        public static byte[] ReadAllBytes(string path) { return SingleEncoding.Single.GetBytes(window.localStorage.getItem(path)); }
        public static void WriteAllBytes(string path, byte[] bytes) { window.localStorage.setItem(path, SingleEncoding.Single.GetString(bytes)); }
    }
}

namespace __System.Net
{
    public partial class HttpWebRequest
    {
        private static object HttpRequestAsync(string contentType, string method, string url, int timeout, Dictionary<string, string> headers, byte[] data, Action callback)
        {
            var request = new XMLHttpRequest();
            request.timeout = timeout;
            request.onreadystatechange = () =>
            {
                if (request.readyState == 4)
                {
                    callback();
                }
            };
            request.responseType = "arraybuffer";
            request.open(method, url, true);
            //request.setRequestHeader("content-length", data.Length.ToString());
            if (headers.Count > 0)
                foreach (var item in headers)
                    request.setRequestHeader(item.Key, item.Value);
            request.send(SingleEncoding.Single.GetString(data));
            return request;
        }
        private static void HttpRequestAbort(object provide)
        {
            ((XMLHttpRequest)provide).abort();
        }
    }
    public partial class HttpWebResponse
    {
        public static object Provide(object requestPrivode) { return requestPrivode; }
        public static int GetContentLength(object provide)
        {
            return ((ArrayBuffer)((XMLHttpRequest)provide).response).byteLength;
            //return ((XMLHttpRequest)provide).responseText.Length;
        }
        public static int GetStatusCode(object provide)
        {
            return ((XMLHttpRequest)provide).status;
        }
        public static byte[] GetResponse(object provide)
        {
            ArrayBuffer buffer = ((ArrayBuffer)((XMLHttpRequest)provide).response);
            Uint8Array array = new Uint8Array(buffer);
            byte[] ret = new byte[buffer.byteLength];
            for (int i = 0; i < buffer.byteLength; i++)
                ret[i] = array[i];
            return ret;
            //return SingleEncoding.Single.GetBytes(((XMLHttpRequest)provide).responseText);
        }
    }
}

/// <summary>char[]与byte[]直接转换，char>255则抛出异常</summary>
public class SingleEncoding : Encoding
{
    private static SingleEncoding single;
    public static SingleEncoding Single
    {
        get
        {
            if (single == null)
                single = new SingleEncoding();
            return single;
        }
    }
    public override bool IsSingleByte
    {
        get { return true; }
    }
    public override int GetByteCount(char[] chars, int index, int count)
    {
        int bc = 0;
        for (int i = index, n = index + count; i < n; i++)
        {
            if (chars[i] > byte.MaxValue)
            {
                throw new ArgumentException();
            }
            else
            {
                bc++;
            }
        }
        return bc;
    }
    public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
    {
        for (int i = 0; i < charCount; i++)
            bytes[byteIndex + i] = (byte)(chars[charIndex + i]);
        return charCount;
    }
    public override int GetCharCount(byte[] bytes, int index, int count)
    {
        return count;
    }
    public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
    {
        for (int i = 0; i < byteCount; i++)
            chars[charIndex + i] = (char)bytes[byteIndex + i];
        return byteCount;
    }
    public override int GetMaxByteCount(int charCount)
    {
        return 1;
    }
    public override int GetMaxCharCount(int byteCount)
    {
        return 1;
    }
}