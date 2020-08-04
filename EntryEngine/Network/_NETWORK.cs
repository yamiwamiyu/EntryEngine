#if SERVER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.IO;
using System.Collections.Specialized;
using System.Globalization;
using System.Collections;

namespace EntryEngine.Network
{
    public static class _NETWORK
    {
        public readonly static string HostIP = Resolve();

        public static bool NetworkAvailable
        {
            get { return NetworkInterface.GetIsNetworkAvailable(); }
        }
        public static IPAddress Host
        {
            get { return IPAddress.Parse(HostIP); }
        }

        public static bool IsPort(int port)
        {
            return port >= IPEndPoint.MinPort && port <= IPEndPoint.MaxPort;
        }
        public static bool IsLan(IPAddress ip)
        {
            byte[] ips = ip.GetAddressBytes();
            if (ips[0] == 192 && ips[1] == 168)
                return true;
            else if (ips[0] == 10)
                return true;
            else if (ips[0] == 172 && ips[1] >= 16 && ips[1] <= 31)
                return true;
            else
                return false;
        }
        public static string Resolve()
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            string host = string.Empty;
            for (int i = 0; i < hostEntry.AddressList.Length; i++)
            {
                if (hostEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    host = hostEntry.AddressList[i].ToString();
                }
            }
            return host;
        }
        public static PhysicalAddress GetLocalMac()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var item in interfaces)
            {
                // 本地连接
                if (item.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    return item.GetPhysicalAddress();
                }
            }
            return null;
        }
        public static string ValidMD5toBase64(string text)
        {
            byte[] encrypt = Encoding.UTF8.GetBytes(text);
            using (var md5 = new MD5CryptoServiceProvider())
            {
                encrypt = md5.ComputeHash(encrypt);
            }
            return Convert.ToBase64String(encrypt);
        }
        /// <summary>更新花生壳域名绑定的外网IP</summary>
        /// <param name="username">花生壳账号名</param>
        /// <param name="password">花生壳密码</param>
        /// <param name="hostname">花生壳域名</param>
        /// <param name="result">成功时返回IP，否则参见http://service.oray.com/question/3820.html</param>
        /// <returns>是否更新成功</returns>
        public static bool OrayHostNameIPUpdate(string username, string password, string hostname, out string result)
        {
            string secret = string.Format("{0}:{1}", username, password);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                string.Format("http://{0}@ddns.oray.com/ph/update?hostname={1}", secret, hostname));
            request.Method = "GET";
            request.KeepAlive = true;
            request.UserAgent = "Oray";
            request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(secret));

            var response = (HttpWebResponse)request.GetResponse();
            using (var stream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                    if (result.StartsWith("good") || result.StartsWith("nochg"))
                    {
                        int index = result.IndexOf(' ') + 1;
                        result = result.Substring(index);
                        return true;
                    }
                }
            }

            return false;
        }



        // http: copy from System.Web.HttpUtility
        private class HttpValueCollection : NameValueCollection
        {
            internal HttpValueCollection(string str, bool readOnly, bool urlencoded, Encoding encoding)
                : base(StringComparer.OrdinalIgnoreCase)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    this.FillFromString(str, urlencoded, encoding);
                }
                base.IsReadOnly = readOnly;
            }
            internal void MakeReadOnly()
            {
                base.IsReadOnly = true;
            }
            internal void MakeReadWrite()
            {
                base.IsReadOnly = false;
            }
            internal void FillFromString(string s)
            {
                this.FillFromString(s, false, null);
            }
            internal void FillFromString(string s, bool urlencoded, Encoding encoding)
            {
                int num = (s != null) ? s.Length : 0;
                for (int i = 0; i < num; i++)
                {
                    int num2 = i;
                    int num3 = -1;
                    while (i < num)
                    {
                        char c = s[i];
                        if (c == '=')
                        {
                            if (num3 < 0)
                            {
                                num3 = i;
                            }
                        }
                        else
                        {
                            if (c == '&')
                            {
                                break;
                            }
                        }
                        i++;
                    }
                    string text = null;
                    string text2;
                    if (num3 >= 0)
                    {
                        text = s.Substring(num2, num3 - num2);
                        text2 = s.Substring(num3 + 1, i - num3 - 1);
                    }
                    else
                    {
                        text2 = s.Substring(num2, i - num2);
                    }
                    if (urlencoded)
                    {
                        base.Add(UrlDecode(text, encoding), UrlDecode(text2, encoding));
                    }
                    else
                    {
                        base.Add(text, text2);
                    }
                    if (i == num - 1 && s[i] == '&')
                    {
                        base.Add(null, string.Empty);
                    }
                }
            }
            internal void Reset()
            {
                base.Clear();
            }
            public override string ToString()
            {
                return this.ToString(true);
            }
            internal virtual string ToString(bool urlencoded)
            {
                return this.ToString(urlencoded, null);
            }
            internal virtual string ToString(bool urlencoded, IDictionary excludeKeys)
            {
                int count = this.Count;
                if (count == 0)
                {
                    return string.Empty;
                }
                StringBuilder stringBuilder = new StringBuilder();
                bool flag = excludeKeys != null && excludeKeys["__VIEWSTATE"] != null;
                for (int i = 0; i < count; i++)
                {
                    string text = this.GetKey(i);
                    if ((!flag || text == null || !text.StartsWith("__VIEWSTATE", StringComparison.Ordinal)) && (excludeKeys == null || text == null || excludeKeys[text] == null))
                    {
                        if (urlencoded)
                        {
                            text = UrlEncodeUnicode(text);
                        }
                        string value = (!string.IsNullOrEmpty(text)) ? (text + "=") : string.Empty;
                        ArrayList arrayList = (ArrayList)base.BaseGet(i);
                        int num = (arrayList != null) ? arrayList.Count : 0;
                        if (stringBuilder.Length > 0)
                        {
                            stringBuilder.Append('&');
                        }
                        if (num == 1)
                        {
                            stringBuilder.Append(value);
                            string text2 = (string)arrayList[0];
                            if (urlencoded)
                            {
                                text2 = UrlEncodeUnicode(text2);
                            }
                            stringBuilder.Append(text2);
                        }
                        else
                        {
                            if (num == 0)
                            {
                                stringBuilder.Append(value);
                            }
                            else
                            {
                                for (int j = 0; j < num; j++)
                                {
                                    if (j > 0)
                                    {
                                        stringBuilder.Append('&');
                                    }
                                    stringBuilder.Append(value);
                                    string text2 = (string)arrayList[j];
                                    if (urlencoded)
                                    {
                                        text2 = UrlEncodeUnicode(text2);
                                    }
                                    stringBuilder.Append(text2);
                                }
                            }
                        }
                    }
                }
                return stringBuilder.ToString();
            }
        }
        private class UrlDecoder
        {
            private int _bufferSize;
            private int _numChars;
            private char[] _charBuffer;
            private int _numBytes;
            private byte[] _byteBuffer;
            private Encoding _encoding;
            private void FlushBytes()
            {
                if (this._numBytes > 0)
                {
                    this._numChars += this._encoding.GetChars(this._byteBuffer, 0, this._numBytes, this._charBuffer, this._numChars);
                    this._numBytes = 0;
                }
            }
            internal UrlDecoder(int bufferSize, Encoding encoding)
            {
                this._bufferSize = bufferSize;
                this._encoding = encoding;
                this._charBuffer = new char[bufferSize];
            }
            internal void AddChar(char ch)
            {
                if (this._numBytes > 0)
                {
                    this.FlushBytes();
                }
                this._charBuffer[this._numChars++] = ch;
            }
            internal void AddByte(byte b)
            {
                if (this._byteBuffer == null)
                {
                    this._byteBuffer = new byte[this._bufferSize];
                }
                this._byteBuffer[this._numBytes++] = b;
            }
            internal string GetString()
            {
                if (this._numBytes > 0)
                {
                    this.FlushBytes();
                }
                if (this._numChars > 0)
                {
                    return new string(this._charBuffer, 0, this._numChars);
                }
                return string.Empty;
            }
        }
        public static NameValueCollection ParseQueryString(string query)
        {
            return ParseQueryString(query, Encoding.UTF8);
        }
        public static NameValueCollection ParseQueryString(string query, Encoding encoding)
        {
            return ParseQueryString(query, true, encoding);
        }
        public static NameValueCollection ParseQueryString(string query, bool urlencoded, Encoding encoding)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (query.Length > 0 && query[0] == '?')
            {
                query = query.Substring(1);
            }
            return new HttpValueCollection(query, false, urlencoded, encoding);
        }
        public static string UrlEncode(string str)
        {
            if (str == null)
            {
                return null;
            }
            return UrlEncode(str, Encoding.UTF8);
        }
        public static string UrlPathEncode(string str)
        {
            if (str == null)
            {
                return null;
            }
            int num = str.IndexOf('?');
            if (num >= 0)
            {
                return UrlPathEncode(str.Substring(0, num)) + str.Substring(num);
            }
            return UrlEncodeSpaces(UrlEncodeNonAscii(str, Encoding.UTF8));
        }
        public static string UrlEncode(string str, Encoding e)
        {
            if (str == null)
            {
                return null;
            }
            return Encoding.ASCII.GetString(UrlEncodeToBytes(str, e));
        }
        internal static string UrlEncodeNonAscii(string str, Encoding e)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            if (e == null)
            {
                e = Encoding.UTF8;
            }
            byte[] array = e.GetBytes(str);
            array = UrlEncodeBytesToBytesInternalNonAscii(array, 0, array.Length, false);
            return Encoding.ASCII.GetString(array);
        }
        internal static string UrlEncodeSpaces(string str)
        {
            if (str != null && str.IndexOf(' ') >= 0)
            {
                str = str.Replace(" ", "%20");
            }
            return str;
        }
        public static string UrlEncode(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            return Encoding.ASCII.GetString(UrlEncodeToBytes(bytes));
        }
        public static string UrlEncode(byte[] bytes, int offset, int count)
        {
            if (bytes == null)
            {
                return null;
            }
            return Encoding.ASCII.GetString(UrlEncodeToBytes(bytes, offset, count));
        }
        public static byte[] UrlEncodeToBytes(string str)
        {
            if (str == null)
            {
                return null;
            }
            return UrlEncodeToBytes(str, Encoding.UTF8);
        }
        public static byte[] UrlEncodeToBytes(string str, Encoding e)
        {
            if (str == null)
            {
                return null;
            }
            byte[] bytes = e.GetBytes(str);
            return UrlEncodeBytesToBytesInternal(bytes, 0, bytes.Length, false);
        }
        public static byte[] UrlEncodeToBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            return UrlEncodeToBytes(bytes, 0, bytes.Length);
        }
        public static byte[] UrlEncodeToBytes(byte[] bytes, int offset, int count)
        {
            if (bytes == null && count == 0)
            {
                return null;
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (offset < 0 || offset > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || offset + count > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            return UrlEncodeBytesToBytesInternal(bytes, offset, count, true);
        }
        public static string UrlEncodeUnicode(string str)
        {
            if (str == null)
            {
                return null;
            }
            return UrlEncodeUnicodeStringToStringInternal(str, false);
        }
        public static byte[] UrlEncodeUnicodeToBytes(string str)
        {
            if (str == null)
            {
                return null;
            }
            return Encoding.ASCII.GetBytes(UrlEncodeUnicode(str));
        }
        public static string UrlDecode(string str)
        {
            if (str == null)
            {
                return null;
            }
            return UrlDecode(str, Encoding.UTF8);
        }
        public static string UrlDecode(string str, Encoding e)
        {
            if (str == null)
            {
                return null;
            }
            return UrlDecodeStringFromStringInternal(str, e);
        }
        public static string UrlDecode(byte[] bytes, Encoding e)
        {
            if (bytes == null)
            {
                return null;
            }
            return UrlDecode(bytes, 0, bytes.Length, e);
        }
        public static string UrlDecode(byte[] bytes, int offset, int count, Encoding e)
        {
            if (bytes == null && count == 0)
            {
                return null;
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (offset < 0 || offset > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || offset + count > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            return UrlDecodeStringFromBytesInternal(bytes, offset, count, e);
        }
        public static byte[] UrlDecodeToBytes(string str)
        {
            if (str == null)
            {
                return null;
            }
            return UrlDecodeToBytes(str, Encoding.UTF8);
        }
        public static byte[] UrlDecodeToBytes(string str, Encoding e)
        {
            if (str == null)
            {
                return null;
            }
            return UrlDecodeToBytes(e.GetBytes(str));
        }
        public static byte[] UrlDecodeToBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            return UrlDecodeToBytes(bytes, 0, (bytes != null) ? bytes.Length : 0);
        }
        public static byte[] UrlDecodeToBytes(byte[] bytes, int offset, int count)
        {
            if (bytes == null && count == 0)
            {
                return null;
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (offset < 0 || offset > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || offset + count > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            return UrlDecodeBytesFromBytesInternal(bytes, offset, count);
        }
        private static byte[] UrlEncodeBytesToBytesInternal(byte[] bytes, int offset, int count, bool alwaysCreateReturnValue)
        {
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < count; i++)
            {
                char c = (char)bytes[offset + i];
                if (c == ' ')
                {
                    num++;
                }
                else
                {
                    if (!IsSafe(c))
                    {
                        num2++;
                    }
                }
            }
            if (!alwaysCreateReturnValue && num == 0 && num2 == 0)
            {
                return bytes;
            }
            byte[] array = new byte[count + num2 * 2];
            int num3 = 0;
            for (int j = 0; j < count; j++)
            {
                byte b = bytes[offset + j];
                char c2 = (char)b;
                if (IsSafe(c2))
                {
                    array[num3++] = b;
                }
                else
                {
                    if (c2 == ' ')
                    {
                        array[num3++] = 43;
                    }
                    else
                    {
                        array[num3++] = 37;
                        array[num3++] = (byte)IntToHex(b >> 4 & 15);
                        array[num3++] = (byte)IntToHex((int)(b & 15));
                    }
                }
            }
            return array;
        }
        private static bool IsNonAsciiByte(byte b)
        {
            return b >= 127 || b < 32;
        }
        private static byte[] UrlEncodeBytesToBytesInternalNonAscii(byte[] bytes, int offset, int count, bool alwaysCreateReturnValue)
        {
            int num = 0;
            for (int i = 0; i < count; i++)
            {
                if (IsNonAsciiByte(bytes[offset + i]))
                {
                    num++;
                }
            }
            if (!alwaysCreateReturnValue && num == 0)
            {
                return bytes;
            }
            byte[] array = new byte[count + num * 2];
            int num2 = 0;
            for (int j = 0; j < count; j++)
            {
                byte b = bytes[offset + j];
                if (IsNonAsciiByte(b))
                {
                    array[num2++] = 37;
                    array[num2++] = (byte)IntToHex(b >> 4 & 15);
                    array[num2++] = (byte)IntToHex((int)(b & 15));
                }
                else
                {
                    array[num2++] = b;
                }
            }
            return array;
        }
        private static string UrlEncodeUnicodeStringToStringInternal(string s, bool ignoreAscii)
        {
            int length = s.Length;
            StringBuilder stringBuilder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                char c = s[i];
                if ((c & 'ﾀ') == '\0')
                {
                    if (ignoreAscii || IsSafe(c))
                    {
                        stringBuilder.Append(c);
                    }
                    else
                    {
                        if (c == ' ')
                        {
                            stringBuilder.Append('+');
                        }
                        else
                        {
                            stringBuilder.Append('%');
                            stringBuilder.Append(IntToHex((int)(c >> 4 & '\u000f')));
                            stringBuilder.Append(IntToHex((int)(c & '\u000f')));
                        }
                    }
                }
                else
                {
                    stringBuilder.Append("%u");
                    stringBuilder.Append(IntToHex((int)(c >> 12 & '\u000f')));
                    stringBuilder.Append(IntToHex((int)(c >> 8 & '\u000f')));
                    stringBuilder.Append(IntToHex((int)(c >> 4 & '\u000f')));
                    stringBuilder.Append(IntToHex((int)(c & '\u000f')));
                }
            }
            return stringBuilder.ToString();
        }
        private static string UrlDecodeStringFromStringInternal(string s, Encoding e)
        {
            int length = s.Length;
            UrlDecoder urlDecoder = new UrlDecoder(length, e);
            for (int i = 0; i < length; i++)
            {
                char c = s[i];
                if (c == '+')
                {
                    c = ' ';
                }
                else if (c != '%' || i >= length - 2)
                {
                }
                else if (s[i + 1] == 'u' && i < length - 5)
                {
                    int num = HexToInt(s[i + 2]);
                    int num2 = HexToInt(s[i + 3]);
                    int num3 = HexToInt(s[i + 4]);
                    int num4 = HexToInt(s[i + 5]);
                    if (num < 0 || num2 < 0 || num3 < 0 || num4 < 0)
                    {
                    }
                    else
                    {
                        c = (char)(num << 12 | num2 << 8 | num3 << 4 | num4);
                        i += 5;
                        urlDecoder.AddChar(c);
                    }
                }
                else
                {
                    int num5 = HexToInt(s[i + 1]);
                    int num6 = HexToInt(s[i + 2]);
                    if (num5 < 0 || num6 < 0)
                    {
                    }
                    else
                    {
                        byte b = (byte)(num5 << 4 | num6);
                        i += 2;
                        urlDecoder.AddByte(b);
                        continue;
                    }
                }
                if ((c & 'ﾀ') == '\0')
                {
                    urlDecoder.AddByte((byte)c);
                    continue;
                }
                urlDecoder.AddChar(c);
            }
            return urlDecoder.GetString();
        }
        private static string UrlDecodeStringFromBytesInternal(byte[] buf, int offset, int count, Encoding e)
        {
            UrlDecoder urlDecoder = new UrlDecoder(count, e);
            for (int i = 0; i < count; i++)
            {
                int num = offset + i;
                byte b = buf[num];
                if (b == 43)
                {
                    b = 32;
                }
                else if (b != 37 || i >= count - 2)
                {
                }
                else if (buf[num + 1] == 117 && i < count - 5)
                {
                    int num2 = HexToInt((char)buf[num + 2]);
                    int num3 = HexToInt((char)buf[num + 3]);
                    int num4 = HexToInt((char)buf[num + 4]);
                    int num5 = HexToInt((char)buf[num + 5]);
                    if (num2 < 0 || num3 < 0 || num4 < 0 || num5 < 0)
                    {
                    }
                    else
                    {
                        char ch = (char)(num2 << 12 | num3 << 8 | num4 << 4 | num5);
                        i += 5;
                        urlDecoder.AddChar(ch);
                    }
                }
                else
                {
                    int num6 = HexToInt((char)buf[num + 1]);
                    int num7 = HexToInt((char)buf[num + 2]);
                    if (num6 >= 0 && num7 >= 0)
                    {
                        b = (byte)(num6 << 4 | num7);
                        i += 2;
                    }
                }
                urlDecoder.AddByte(b);
            }
            return urlDecoder.GetString();
        }
        private static byte[] UrlDecodeBytesFromBytesInternal(byte[] buf, int offset, int count)
        {
            int num = 0;
            byte[] array = new byte[count];
            for (int i = 0; i < count; i++)
            {
                int num2 = offset + i;
                byte b = buf[num2];
                if (b == 43)
                {
                    b = 32;
                }
                else
                {
                    if (b == 37 && i < count - 2)
                    {
                        int num3 = HexToInt((char)buf[num2 + 1]);
                        int num4 = HexToInt((char)buf[num2 + 2]);
                        if (num3 >= 0 && num4 >= 0)
                        {
                            b = (byte)(num3 << 4 | num4);
                            i += 2;
                        }
                    }
                }
                array[num++] = b;
            }
            if (num < array.Length)
            {
                byte[] array2 = new byte[num];
                Array.Copy(array, array2, num);
                array = array2;
            }
            return array;
        }
        private static int HexToInt(char h)
        {
            if (h >= '0' && h <= '9')
            {
                return (int)(h - '0');
            }
            if (h >= 'a' && h <= 'f')
            {
                return (int)(h - 'a' + '\n');
            }
            if (h < 'A' || h > 'F')
            {
                return -1;
            }
            return (int)(h - 'A' + '\n');
        }
        internal static char IntToHex(int n)
        {
            if (n <= 9)
            {
                return (char)(n + 48);
            }
            return (char)(n - 10 + 97);
        }
        internal static bool IsSafe(char ch)
        {
            if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9'))
            {
                return true;
            }
            if (ch != '!')
            {
                switch (ch)
                {
                    case '\'':
                    case '(':
                    case ')':
                    case '*':
                    case '-':
                    case '.':
                        return true;
                    case '+':
                    case ',':
                        break;
                    default:
                        if (ch == '_')
                        {
                            return true;
                        }
                        break;
                }
                return false;
            }
            return true;
        }
    }
}

#endif