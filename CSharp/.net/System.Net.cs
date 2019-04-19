using System;
using System.Collections.Generic;
using __System.IO;
using System.Text;
using __System.Net.Sockets;

namespace __System.Net
{
    public enum HttpStatusCode
    {
        Continue = 100,
        SwitchingProtocols,
        OK = 200,
        Created,
        Accepted,
        NonAuthoritativeInformation,
        NoContent,
        ResetContent,
        PartialContent,
        MultipleChoices = 300,
        Ambiguous = 300,
        MovedPermanently,
        Moved = 301,
        Found,
        Redirect = 302,
        SeeOther,
        RedirectMethod = 303,
        NotModified,
        UseProxy,
        Unused,
        TemporaryRedirect,
        RedirectKeepVerb = 307,
        BadRequest = 400,
        Unauthorized,
        PaymentRequired,
        Forbidden,
        NotFound,
        MethodNotAllowed,
        NotAcceptable,
        ProxyAuthenticationRequired,
        RequestTimeout,
        Conflict,
        Gone,
        LengthRequired,
        PreconditionFailed,
        RequestEntityTooLarge,
        RequestUriTooLong,
        UnsupportedMediaType,
        RequestedRangeNotSatisfiable,
        ExpectationFailed,
        UpgradeRequired = 426,
        InternalServerError = 500,
        NotImplemented,
        BadGateway,
        ServiceUnavailable,
        GatewayTimeout,
        HttpVersionNotSupported
    }
    public enum WebExceptionStatus
    {
        Success,
        NameResolutionFailure,
        ConnectFailure,
        ReceiveFailure,
        SendFailure,
        PipelineFailure,
        RequestCanceled,
        ProtocolError,
        ConnectionClosed,
        TrustFailure,
        SecureChannelFailure,
        ServerProtocolViolation,
        KeepAliveFailure,
        Pending,
        Timeout,
        ProxyNameResolutionFailure,
        UnknownError,
        MessageLengthLimitExceeded,
        CacheEntryNotFound,
        RequestProhibitedByCachePolicy,
        RequestProhibitedByProxy
    }
    public class WebException : InvalidOperationException
    {
        private WebExceptionStatus m_Status = WebExceptionStatus.UnknownError;
        public WebExceptionStatus Status { get { return this.m_Status; } }
        public WebException() { }
        public WebException(WebExceptionStatus status)
        {
            this.m_Status = status;
        }
    }

    public abstract class WebRequest
    {
        internal const int DefaultTimeout = 100000;
        public virtual string Method
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public virtual Uri RequestUri
        {
            get { throw new NotImplementedException(); }
        }
        public virtual long ContentLength
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public virtual string ContentType
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public virtual int Timeout
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public static WebRequest Create(string requestUriString)
        {
            if (requestUriString == null)
            {
                throw new ArgumentNullException("requestUriString");
            }
            return WebRequest.Create(new Uri(requestUriString));
        }
        public static WebRequest Create(Uri requestUri)
        {
            if (requestUri == null)
            {
                throw new ArgumentNullException("requestUri");
            }
            return new HttpWebRequest(requestUri);
        }
        protected WebRequest()
        {
        }
        public virtual IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }
        public virtual Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }
        public virtual IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }
        public virtual WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }
        public virtual void Abort()
        {
            throw new NotImplementedException();
        }
    }
    internal class SyncResult : IAsyncResult
    {
        private object state;
        public bool IsCompleted
        {
            get { return true; }
        }
        public object AsyncState
        {
            get { return state; }
        }
        public bool CompletedSynchronously
        {
            get { return true; }
        }
        public SyncResult(object state)
        {
            this.state = state;
        }
    }
    internal class AsyncResult : IAsyncResult
    {
        private object state;
        internal bool isCompleted;
        public bool IsCompleted
        {
            get { return isCompleted; }
        }
        public object AsyncState
        {
            get { return state; }
        }
        public bool CompletedSynchronously
        {
            get { return false; }
        }
        public AsyncResult(object state)
        {
            this.state = state;
        }
    }
    public partial class HttpWebRequest : WebRequest, IDisposable
    {
        private static byte[] __empty = new byte[0];
        internal object __api;
        private MemoryStream stream;
        private string contentType = "text/plain";
        private string method = "GET";
        private Uri uri;
        private int timeout = 60000;
        private Dictionary<string, string> headers = new Dictionary<string, string>();
        public override string ContentType
        {
            get { return contentType; }
            set { contentType = value; }
        }
        public override string Method
        {
            get { return method; }
            set { method = value; }
        }
        public Dictionary<string, string> Headers
        {
            get { return headers; }
        }
        public override Uri RequestUri
        {
            get { return uri; }
        }
        public override long ContentLength
        {
            get
            {
                if (stream == null)
                    return 0;
                return stream.Length;
            }
            set
            {
                if (stream == null)
                {
                    stream = new MemoryStream(new byte[value]);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }
        public override int Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }
        internal HttpWebRequest(Uri uri)
        {
            this.uri = uri;
        }
        public override void Abort()
        {
            if (__api != null)
            {
                HttpRequestAbort(__api);
            }
        }
        public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            SyncResult result = new SyncResult(state);
            if (callback != null)
            {
                callback(result);
            }
            return result;
        }
        public override Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            if (stream == null)
                stream = new MemoryStream(1024);
            return stream;
        }
        [ASystemAPI]public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            byte[] data;
            if (stream == null)
                data = __empty;
            else
                data = stream.ToArray();
            AsyncResult result = new AsyncResult(state);
            this.__api = HttpRequestAsync(ContentType, Method, uri.AbsolutePath, Timeout, headers, data,
                () =>
                {
                    result.isCompleted = true;
                    callback(result);
                });
            return result;
        }
        public override WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            return new HttpWebResponse(this);
        }
        public void Dispose()
        {
            __api = null;
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }
        }
        [ASystemAPI]private void AddHeader(string key, string value) { }

        [ASystemAPI]private static object HttpRequestAsync(string contentType, string method, string url, int timeout, Dictionary<string, string> headers, byte[] data, Action callback) { return null; }
        [ASystemAPI]private static void HttpRequestAbort(object provide) { }
    }
    public abstract class WebResponse
    {
        public virtual long ContentLength
        {
            get { throw new NotImplementedException(); }
        }
        public virtual string ContentType
        {
            get { throw new NotImplementedException(); }
        }
        public virtual void Close() { }
        public void Dispose()
        {
            try
            {
                this.Close();
                this.OnDispose();
            }
            catch
            {
            }
        }
        internal virtual void OnDispose()
        {
        }
        public abstract Stream GetResponseStream();
    }
    public partial class HttpWebResponse : WebResponse
    {
        private object __api;
        internal HttpWebRequest request;
        private MemoryStream stream;
        public HttpStatusCode StatusCode
        {
            get { return (HttpStatusCode)GetStatusCode(__api); }
        }
        public override string ContentType
        {
            get { return request.ContentType; }
        }
        public override long ContentLength
        {
            get { return GetContentLength(__api); }
        }
        public string Method
        {
            get { return request.Method; }
        }
        internal HttpWebResponse(HttpWebRequest request)
        {
            this.request = request;
            this.__api = Provide(request.__api);
        }
        public override Stream GetResponseStream()
        {
            if (stream != null)
                return stream;
            byte[] binary = GetResponse(__api);
            stream = new MemoryStream(binary);
            return stream;
        }
        public override void Close()
        {
            __api = null;
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }
            if (request != null)
            {
                request.Dispose();
                request = null;
            }
        }

        [ASystemAPI]private static object Provide(object requestPrivode) { return null; }
        [ASystemAPI]private static int GetContentLength(object provide) { return 0; }
        [ASystemAPI]private static int GetStatusCode(object provide) { return 0; }
        [ASystemAPI]private static byte[] GetResponse(object provide) { return null; }
    }

    public abstract class EndPoint
    {
        public virtual AddressFamily AddressFamily
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
    public class IPEndPoint : EndPoint
    {
        public const int MinPort = 0;
        public const int MaxPort = 65535;
        private IPAddress m_Address;
        private int m_Port;
        internal const int AnyPort = 0;
        internal static IPEndPoint Any = new IPEndPoint(IPAddress.Any, 0);
        internal static IPEndPoint IPv6Any = new IPEndPoint(IPAddress.IPv6Any, 0);
        public override AddressFamily AddressFamily
        {
            get
            {
                return this.m_Address.AddressFamily;
            }
        }
        public IPAddress Address
        {
            get
            {
                return this.m_Address;
            }
            set
            {
                this.m_Address = value;
            }
        }
        public int Port
        {
            get
            {
                return this.m_Port;
            }
            set
            {
                if (value < MinPort || value >= MaxPort)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.m_Port = value;
            }
        }
        public IPEndPoint(long address, int port)
        {
            this.Port = port;
            this.m_Address = new IPAddress(address);
        }
        public IPEndPoint(IPAddress address, int port)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            this.Port = port;
            this.m_Address = address;
        }
        public override string ToString()
        {
            string format;
            if (this.m_Address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                format = "[{0}]:{1}";
            }
            else
            {
                format = "{0}:{1}";
            }
            return string.Format(format, this.m_Address.ToString(), this.Port.ToString());
        }
        public override bool Equals(object comparand)
        {
            return comparand is IPEndPoint && ((IPEndPoint)comparand).m_Address.Equals(this.m_Address) && ((IPEndPoint)comparand).m_Port == this.m_Port;
        }
        public override int GetHashCode()
        {
            return this.m_Address.GetHashCode() ^ this.m_Port;
        }
    }
    public class IPAddress
    {
        public static readonly IPAddress Any = new IPAddress(0);
        public static readonly IPAddress Loopback = new IPAddress(16777343);
        //public static readonly IPAddress Broadcast = new IPAddress((long)((ulong)-1));
        //public static readonly IPAddress None = IPAddress.Broadcast;
        internal const long LoopbackMask = 255L;
        internal long m_Address;
        internal string m_ToString;
        public static readonly IPAddress IPv6Any = new IPAddress(new byte[16], 0L);
        public static readonly IPAddress IPv6Loopback = new IPAddress(new byte[]
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1
		}, 0L);
        public static readonly IPAddress IPv6None = new IPAddress(new byte[16], 0L);
        private AddressFamily m_Family = AddressFamily.InterNetwork;
        private ushort[] m_Numbers = new ushort[8];
        private long m_ScopeId;
        private int m_HashCode;
        internal const int IPv4AddressBytes = 4;
        internal const int IPv6AddressBytes = 16;
        internal const int NumberOfLabels = 8;
        public AddressFamily AddressFamily
        {
            get
            {
                return this.m_Family;
            }
        }
        //internal bool IsBroadcast
        //{
        //    get
        //    {
        //        return this.m_Family != AddressFamily.InterNetworkV6 && this.m_Address == IPAddress.Broadcast.m_Address;
        //    }
        //}
        public bool IsIPv6Multicast
        {
            get
            {
                return this.m_Family == AddressFamily.InterNetworkV6 && (this.m_Numbers[0] & 65280) == 65280;
            }
        }
        public bool IsIPv6LinkLocal
        {
            get
            {
                return this.m_Family == AddressFamily.InterNetworkV6 && (this.m_Numbers[0] & 65472) == 65152;
            }
        }
        public bool IsIPv6SiteLocal
        {
            get
            {
                return this.m_Family == AddressFamily.InterNetworkV6 && (this.m_Numbers[0] & 65472) == 65216;
            }
        }
        public bool IsIPv6Teredo
        {
            get
            {
                return this.m_Family == AddressFamily.InterNetworkV6 && this.m_Numbers[0] == 8193 && this.m_Numbers[1] == 0;
            }
        }
        public bool IsIPv4MappedToIPv6
        {
            get
            {
                if (this.AddressFamily != AddressFamily.InterNetworkV6)
                {
                    return false;
                }
                for (int i = 0; i < 5; i++)
                {
                    if (this.m_Numbers[i] != 0)
                    {
                        return false;
                    }
                }
                return this.m_Numbers[5] == 65535;
            }
        }
        public IPAddress(long newAddress)
        {
            this.m_Address = newAddress;
        }
        public IPAddress(byte[] address, long scopeid)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (address.Length != 16)
            {
                throw new ArgumentException("dns_bad_ip_address");
            }
            this.m_Family = AddressFamily.InterNetworkV6;
            for (int i = 0; i < 8; i++)
            {
                this.m_Numbers[i] = (ushort)((int)address[i * 2] * 256 + (int)address[i * 2 + 1]);
            }
            this.m_ScopeId = scopeid;
        }
        private IPAddress(ushort[] address, uint scopeid)
        {
            this.m_Family = AddressFamily.InterNetworkV6;
            this.m_Numbers = address;
            this.m_ScopeId = (long)((ulong)scopeid);
        }
        internal IPAddress(int newAddress)
        {
            //this.m_Address = ((long)newAddress & (long)((ulong)-1));
        }
        public static bool TryParse(string ipString, out IPAddress address)
        {
            address = IPAddress.InternalParse(ipString, true);
            return address != null;
        }
        public static IPAddress Parse(string ipString)
        {
            return IPAddress.InternalParse(ipString, false);
        }
        private static IPAddress InternalParse(string ipString, bool tryParse)
        {
            if (ipString == null)
            {
                if (tryParse)
                {
                    return null;
                }
                throw new ArgumentNullException("ipString");
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public byte[] GetAddressBytes()
        {
            byte[] array;
            if (this.m_Family == AddressFamily.InterNetworkV6)
            {
                array = new byte[16];
                int num = 0;
                for (int i = 0; i < 8; i++)
                {
                    array[num++] = (byte)(this.m_Numbers[i] >> 8 & 255);
                    array[num++] = (byte)(this.m_Numbers[i] & 255);
                }
            }
            else
            {
                array = new byte[]
				{
					(byte)this.m_Address,
					(byte)(this.m_Address >> 8),
					(byte)(this.m_Address >> 16),
					(byte)(this.m_Address >> 24)
				};
            }
            return array;
        }
        public override string ToString()
        {
            if (this.m_ToString == null)
            {
                int num2 = 15;
                char[] ptr = new char[15];
                int num3 = (int)(this.m_Address >> 24 & 255L);
                do
                {
                    ptr[--num2] = (char)(48 + num3 % 10);
                    num3 /= 10;
                }
                while (num3 > 0);
                ptr[--num2] = '.';
                num3 = (int)(this.m_Address >> 16 & 255L);
                do
                {
                    ptr[--num2] = (char)(48 + num3 % 10);
                    num3 /= 10;
                }
                while (num3 > 0);
                ptr[--num2] = '.';
                num3 = (int)(this.m_Address >> 8 & 255L);
                do
                {
                    ptr[--num2] = (char)(48 + num3 % 10);
                    num3 /= 10;
                }
                while (num3 > 0);
                ptr[--num2] = '.';
                num3 = (int)(this.m_Address & 255L);
                do
                {
                    ptr[--num2] = (char)(48 + num3 % 10);
                    num3 /= 10;
                }
                while (num3 > 0);
                this.m_ToString = new string(ptr, num2, 15 - num2);
            }
            return this.m_ToString;
        }
        public static bool IsLoopback(IPAddress address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (address.m_Family == AddressFamily.InterNetworkV6)
            {
                return address.Equals(IPAddress.IPv6Loopback);
            }
            return (address.m_Address & 255L) == (IPAddress.Loopback.m_Address & 255L);
        }
        internal bool Equals(object comparandObj, bool compareScopeId)
        {
            IPAddress iPAddress = comparandObj as IPAddress;
            if (iPAddress == null)
            {
                return false;
            }
            if (this.m_Family != iPAddress.m_Family)
            {
                return false;
            }
            return iPAddress.m_Address == this.m_Address;
        }
        public override bool Equals(object comparand)
        {
            return this.Equals(comparand, true);
        }
        public override int GetHashCode()
        {
            return (int)this.m_Address;
        }
    }
}
