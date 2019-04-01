using System;
using System.Collections.Generic;
using __System.IO;
using System.Text;

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
                data = stream.GetBuffer();
            AsyncResult result = new AsyncResult(state);
            this.__api = HttpRequestAsync(ContentType, Method, uri.AbsolutePath, Timeout, data,
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

        [ASystemAPI]private static object HttpRequestAsync(string contentType, string method, string url, int timeout, byte[] data, Action callback) { return null; }
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

    //public abstract class EndPoint
    //{
    //    protected EndPoint() { }
    //}
    //public class IPEndPoint : EndPoint
    //{
    //    public const int MinPort = 0;
    //    public const int MaxPort = 65535;
    //    private IPAddress m_Address;
    //    private int m_Port;
    //    public IPAddress Address
    //    {
    //        get { return this.m_Address; }
    //        set { this.m_Address = value; }
    //    }
    //    public int Port
    //    {
    //        get { return this.m_Port; }
    //        set { this.m_Port = value; }
    //    }
    //    public IPEndPoint(IPAddress address, int port)
    //    {
    //        this.m_Port = port;
    //        this.m_Address = address;
    //    }
    //    public override string ToString()
    //    {
    //        return string.Format("{0}:{1}", this.m_Address.ToString(), this.Port.ToString());
    //    }
    //    public override bool Equals(object comparand)
    //    {
    //        return comparand is IPEndPoint && ((IPEndPoint)comparand).m_Address.Equals(this.m_Address) && ((IPEndPoint)comparand).m_Port == this.m_Port;
    //    }
    //    public override int GetHashCode()
    //    {
    //        return this.m_Address.GetHashCode() ^ this.m_Port;
    //    }
    //}
}
