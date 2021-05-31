using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using EntryEngine.Serialize;
using System.IO;
using System.Linq;
using System.Text;

namespace EntryEngine.Network
{
#if SERVER

    // socket
    public class LinkSocket : LinkBinary, IConnector
    {
        public Socket Socket
        {
            get;
            private set;
        }
        public override bool IsConnected
        {
            get
            {
                if (Socket != null && Socket.Connected)
                {
                    if (Socket.Poll(0, SelectMode.SelectRead) && Socket.Available == 0)
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            }
        }
        public override bool CanRead
        {
            get { return Socket.Available >= MAX_BUFFER_SIZE; }
        }
        protected override int DataLength
        {
            get { return Socket.Available; }
        }
        public override IPEndPoint EndPoint
        {
            get { return Socket == null ? null : (IPEndPoint)Socket.RemoteEndPoint; }
        }

        public LinkSocket()
        {
        }
        public LinkSocket(Socket socket)
        {
            if (socket == null)
                throw new ArgumentNullException("socket");
            this.Socket = socket;
            socket.SendBufferSize = MaxBuffer;
            socket.ReceiveBufferSize = MaxBuffer;
        }

        protected override int PeekSize(byte[] buffer, out int peek)
        {
            peek = 0;
            return Socket.Receive(buffer, 0, MAX_BUFFER_SIZE, SocketFlags.Peek);
        }
        protected override int InternalRead(byte[] buffer, int offset, int size)
        {
            SocketError error;
            int read = Socket.Receive(buffer, offset, size, SocketFlags.None, out error);
            if (error != SocketError.Success)
                throw new SocketException((int)error);
            return read;
        }
        protected override void InternalFlush(byte[] buffer)
        {
            SocketError error;
            Socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, out error, null, null);
            //Socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, out error, SendCallback, buffer);
            if (error != SocketError.Success)
                throw new SocketException((int)error);
        }
        //private void SendCallback(IAsyncResult ar)
        //{
        //    SocketError error;
        //    int sent = Socket.EndSend(ar, out error);
        //}
        public override void Close()
        {
            base.Close();
            if (Socket != null)
            {
                if (Socket.Connected)
                {
                    Socket.Shutdown(SocketShutdown.Both);
                }
                Socket.Close();
                Socket = null;
            }
        }
        protected virtual Socket BuildSocket()
        {
            throw new NotImplementedException();
        }
        protected void BuildConnection()
        {
            if (IsConnected)
                throw new InvalidOperationException("try connect while socket has been connected.");
            Close();
            Socket = BuildSocket();
            Socket.SendBufferSize = MaxBuffer;
            Socket.ReceiveBufferSize = MaxBuffer;
        }
        public void ConnectSync(string host, ushort port)
        {
            BuildConnection();
            Socket.Connect(host, port);
        }
        public virtual AsyncData<Link> Connect(string host, ushort port)
        {
            BuildConnection();
            AsyncData<Link> async = new AsyncData<Link>();
            async.Run();
            Socket.BeginConnect(host, port, ar =>
            {
                try
                {
                    Socket.EndConnect(ar);
                    if (Socket.Connected)
                        async.SetData(this);
                    else
                        async.Cancel();
                }
                catch (Exception ex)
                {
                    async.Error(ex);
                }
            }, Socket);
            return async;
        }
    }
	public class LinkTcp : LinkSocket
	{
		public LinkTcp()
		{
		}
        public LinkTcp(Socket socket) : base(socket)
		{
		}

        protected override Socket BuildSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

		public static LinkTcp Connect(string host, ushort port, Action<Socket> onConnect)
		{
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			if (onConnect != null)
				onConnect(socket);
			socket.Connect(host, port);
			return new LinkTcp(socket);
		}
        public static AsyncData<LinkTcp> ConnectAsync(string host, ushort port, Action<Socket> onConnect)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (onConnect != null)
                onConnect(socket);
            AsyncData<LinkTcp> async = new AsyncData<LinkTcp>();
            socket.BeginConnect(host, port,
                e =>
                {
                    try
                    {
                        socket.EndConnect(e);
                        async.SetData(new LinkTcp(socket));
                    }
                    catch (Exception ex)
                    {
                        async.Error(ex);
                    }
                }, socket);
            return async;
        }
    }
    [Code(ECode.ToBeContinue | ECode.MayBeReform)]
    public class LinkUdp : LinkSocket
    {
        private IPEndPoint endPoint;
       
        public override IPEndPoint EndPoint
        {
            get { return endPoint == null ? base.EndPoint : endPoint; }
        }
        public override bool IsConnected
        {
            get
            {
                return endPoint != null || base.IsConnected;
            }
        }

        public LinkUdp()
		{
		}
        //public LinkUdp(Socket socket) : base(socket)
        //{
        //}
        public LinkUdp(Socket socket, IPEndPoint endPoint) : base(socket)
        {
            if (endPoint == null)
                throw new ArgumentNullException("endPoint");
            this.endPoint = endPoint;
            //Connect(endPoint.Address.ToString(), (ushort)endPoint.Port);
        }

        public void Bind(string ip, ushort port)
        {
            Bind(new IPEndPoint(IPAddress.Parse(ip), port));
        }
        public void Bind(IPEndPoint ep)
        {
            if (ep == null)
                throw new ArgumentNullException("ep");
            if (Socket != null)
                throw new InvalidOperationException("can't bind because socket is not null");
            BuildConnection();
            Socket.Bind(ep);
        }
        protected override int InternalRead(byte[] buffer, int offset, int size)
        {
            EndPoint ep = new IPEndPoint(0, 0);
            int read = Socket.ReceiveFrom(buffer, SocketFlags.None, ref ep);
            return read;
        }
        protected override int PeekSize(byte[] buffer, out int peek)
        {
            EndPoint ep = new IPEndPoint(0, 0);
            peek = 0;
            peek = Socket.ReceiveFrom(buffer, SocketFlags.Peek, ref ep);
            return peek;
        }
        protected override Socket BuildSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }
        public override AsyncData<Link> Connect(string host, ushort port)
        {
            BuildConnection();
            endPoint = new IPEndPoint(IPAddress.Parse(host), port);
            var sync = new AsyncData<Link>();
            sync.Run();
            sync.SetData(this);
            return sync;
        }
        protected override void InternalFlush(byte[] buffer)
        {
            if (endPoint == null)
                base.InternalFlush(buffer);
            else
                Socket.BeginSendTo(buffer, 0, buffer.Length, SocketFlags.None, endPoint, null, null);
        }
    }

#endif

    // agent and stub
    public abstract class Agent
    {
        public virtual Link Link { get; set; }
        public virtual IEnumerable<byte[]> Receive()
        {
            while (Link.CanRead)
            {
                byte[] package = Link.Read();
                if (package != null)
                    yield return package;
                else
                    yield break;
            }
        }
        public abstract void OnProtocol(byte[] package);
        public virtual void Bridge()
        {
            /*
             * Link是包协议
             * 由Link发送和读取的内容是数据协议
             * 数据的写入格式自定义
             * Agent的OnProtocol则是对数据的读取和自定义处理
             */
            if (Link != null && Link.IsConnected)
            {
                /* 每帧需要手动发出客户端的请求数据 */
                Link.Flush();
                foreach (var item in Receive())
                {
                    OnProtocol(item);
                }
            }
        }
    }
    public abstract class Agent_Link : Agent
    {
        public virtual EntryEngine.Network.Agent Base { get; set; }
        public override EntryEngine.Network.Link Link
        {
            get { return Base.Link; }
            set { Base.Link = value; }
        }

        public Agent_Link() { }
        public Agent_Link(EntryEngine.Network.Agent Base) { this.Base = Base; }

        public override System.Collections.Generic.IEnumerable<byte[]> Receive()
        {
            return Base.Receive();
        }
        public override void OnProtocol(byte[] package)
        {
            Base.OnProtocol(package);
        }
        public override void Bridge()
        {
            Base.Bridge();
        }
    }
    public class AgentProtocolStub : Agent
    {
        private Dictionary<byte, Stub> protocols = new Dictionary<byte, Stub>();

        public AgentProtocolStub()
        {
        }
        public AgentProtocolStub(Link link)
        {
            this.Link = link;
        }
        public AgentProtocolStub(params Stub[] stubs)
        {
            foreach (var stub in stubs)
                AddAgent(stub);
        }
        public AgentProtocolStub(Link link, params Stub[] stubs)
        {
            this.Link = link;
            foreach (var stub in stubs)
                AddAgent(stub);
        }

        public void AddAgent(Stub stub)
        {
            if (stub == null)
                throw new ArgumentNullException("agent");
            stub.ProtocolAgent = this;
            protocols.Add(stub.Protocol, stub);
        }
        public override void OnProtocol(byte[] data)
        {
            ByteReader reader = new ByteReader(data);
            while (true)
            {
                int position = reader.Position;
                byte protocol;
                ushort index;
                reader.Read(out protocol);
                reader.Read(out index);

                Stub agent;
                if (!protocols.TryGetValue(protocol, out agent))
                    throw new NotImplementedException("no procotol: " + protocol);

                agent[index](reader);
                if (position == reader.Position || reader.IsEnd)
                    break;
            }
        }
    }
    // be used to generate code
    public abstract class Stub
    {
        private Dictionary<ushort, Action<ByteReader>> stubs = new Dictionary<ushort, Action<ByteReader>>();
        protected internal byte Protocol
        {
            get;
            protected set;
        }
        protected internal AgentProtocolStub ProtocolAgent;
        public Link Link
        {
            get { return ProtocolAgent.Link; }
        }
        internal Action<ByteReader> this[ushort index]
        {
            get
            {
                Action<ByteReader> stub;
                if (!stubs.TryGetValue(index, out stub))
                    throw new ArgumentOutOfRangeException(string.Format("protocol: {0} method stub {1} not find!", Protocol, index));
                return stub;
            }
        }
        protected Stub()
        {
        }
        public Stub(byte protocol)
        {
            this.Protocol = protocol;
        }
        protected void AddMethod(Action<ByteReader> method)
        {
            AddMethod((ushort)stubs.Count, method);
        }
        protected void AddMethod(ushort id, Action<ByteReader> method)
        {
            if (method == null)
                throw new ArgumentNullException("method");
            stubs.Add(id, method);
        }
    }
    public abstract class StubClientAsync : Stub
    {
        public class AsyncWaitCallback : ICoroutine
        {
            private StubClientAsync stub;
            public event Action<sbyte, string> OnError;

            public bool IsEnd
            {
                get;
                internal set;
            }
            public byte ID
            {
                get;
                private set;
            }
            public sbyte Ret
            {
                get;
                private set;
            }
            public string Msg
            {
                get;
                private set;
            }
            public Delegate Function
            {
                get;
                private set;
            }

            internal AsyncWaitCallback(StubClientAsync stub, Delegate function, byte id)
            {
                if (stub == null)
                    throw new ArgumentNullException("stub");
                this.stub = stub;
                this.Function = function;
                this.ID = id;
            }

            internal void Error(sbyte ret, string msg)
            {
                this.Ret = ret;
                this.Msg = msg;
                if (OnError != null)
                    OnError(ret, msg);
                this.IsEnd = true;
            }
            public void Update(float time)
            {
            }
        }

        private Dictionary<byte, AsyncWaitCallback> requests = new Dictionary<byte, AsyncWaitCallback>();
        private byte id;
        public event Action<ushort, sbyte, string> OnAsyncCallbackError;
        public event Action OnAsyncRequestFull;
        public event Action<Exception> OnAgentError;

        protected bool HasRequest
        {
            get { return requests.Count > 0; }
        }
        public bool IsAsyncRequestFull
        {
            get
            {
                byte next = (byte)(id + 1);
                return requests.ContainsKey(next);
            }
        }

        protected AsyncWaitCallback Push(Delegate method)
        {
            if (requests.ContainsKey(id))
            {
                if (OnAsyncRequestFull == null)
                {
                    throw new OutOfMemoryException("stack full");
                }
                else
                {
                    OnAsyncRequestFull();
                    return null;
                }
            }
            var async = new AsyncWaitCallback(this, method, id);
            requests.Add(id, async);
            id++;
            return async;
        }
        protected AsyncWaitCallback Pop(byte id)
        {
            AsyncWaitCallback async;
            if (requests.TryGetValue(id, out async))
            {
                async.IsEnd = true;
                requests.Remove(id);
            }
            else
                throw new KeyNotFoundException(id.ToString());
            return async;
        }
        protected void Error(AsyncWaitCallback async, ushort key, sbyte ret, string msg)
        {
            async.Error(ret, msg);
            if (OnAsyncCallbackError != null)
                OnAsyncCallbackError(key, ret, msg);
        }
        public void Update(int packageCount)
        {
            Update(null, packageCount);
        }
        public void Update(Agent agent, int packageCount)
        {
            Link link = Link;
            if (agent == null)
            {
                agent = ProtocolAgent;
                link = Link;
            }
            if (agent != null && link != null && link.IsConnected)
            {
                link.Flush();
                foreach (var item in agent.Receive())
                {
                    try
                    {
                        agent.OnProtocol(item);
                        if (--packageCount == 0)
                            break;
                    }
                    catch (Exception ex)
                    {
                        if (OnAgentError == null)
                            throw ex;
                        else
                            OnAgentError(ex);
                    }
                }
            }
        }
    }

    // web socket
    public class AgentProtocolStubJson : AgentProtocolStub
    {
        private Dictionary<byte, StubJson> protocols = new Dictionary<byte, StubJson>();

        public AgentProtocolStubJson()
        {
        }
        public AgentProtocolStubJson(Link link)
        {
            this.Link = link;
        }
        public AgentProtocolStubJson(params StubJson[] stubs)
        {
            foreach (var stub in stubs)
                AddAgent(stub);
        }
        public AgentProtocolStubJson(Link link, params StubJson[] stubs)
        {
            this.Link = link;
            foreach (var stub in stubs)
                AddAgent(stub);
        }

        public void AddAgent(StubJson stub)
        {
            if (stub == null)
                throw new ArgumentNullException("agent");
            stub.ProtocolAgent = this;
            protocols.Add(stub.Protocol, stub);
        }
        public override void OnProtocol(byte[] data)
        {
            //var pack = JsonReader.Deserialize<ProtocolCall>(Encoding.UTF8.GetString(data));
            try
            {
                var pack = JsonReader.Deserialize<ProtocolCall>(Encoding.UTF8.GetString(data));

                StubJson agent;
                if (!protocols.TryGetValue(pack.Protocol, out agent))
                    throw new NotImplementedException("no procotol: " + pack.Protocol);

                agent[pack.Stub](pack.JO);
            }
            catch (Exception ex)
            {
                _LOG.Error(ex, "协议处理错误！");
                try
                {
                    int err = 500;
                    HttpException ex2 = ex as HttpException;
                    if (ex2 != null)
                    {
                        err = (int)ex2.StatusCode;
                    }
                    Link.Write(Encoding.UTF8.GetBytes(JsonWriter.Serialize(new HttpError(err, ex.Message))));
                }
                catch (Exception exInner)
                {
                    _LOG.Error(exInner, "协议异常回调错误！");
                }
            }
        }
    }
    public abstract class StubJson
    {
        private Dictionary<ushort, Action<string>> stubs = new Dictionary<ushort, Action<string>>();
        protected internal byte Protocol
        {
            get;
            protected set;
        }
        protected internal AgentProtocolStub ProtocolAgent;
        public Link Link
        {
            get { return ProtocolAgent.Link; }
        }
        internal Action<string> this[ushort index]
        {
            get
            {
                Action<string> stub;
                if (!stubs.TryGetValue(index, out stub))
                    throw new ArgumentOutOfRangeException(string.Format("protocol: {0} method stub {1} not find!", Protocol, index));
                return stub;
            }
        }
        protected StubJson()
        {
        }
        public StubJson(byte protocol)
        {
            this.Protocol = protocol;
        }
        protected void AddMethod(Action<string> method)
        {
            AddMethod((ushort)stubs.Count, method);
        }
        protected void AddMethod(ushort id, Action<string> method)
        {
            if (method == null)
                throw new ArgumentNullException("method");
            stubs.Add(id, method);
        }
    }
    public abstract class ProtocolPack
    {
        public byte Protocol;
        public ushort Stub;
    }
    public class ProtocolCall : ProtocolPack
    {
        /// <summary>Json Object String</summary>
        public string JO;
        public ProtocolCall() { }
        public ProtocolCall(byte protocol, ushort stub)
        {
            this.Protocol = protocol;
            this.Stub = stub;
        }
    }

    // http
    public class LinkHttpRequestShort : LinkBinary
    {
        public string Uri;
        public TimeSpan Timeout = TimeSpan.FromSeconds(5);
        public int ReconnectCount = 3;
        protected LinkedList<HttpRequestPost> requests = new LinkedList<HttpRequestPost>();
        private int dataLength = -1;
        public event Action<HttpRequestPost> OnRequestPrepare;

        public override bool IsConnected
        {
            get { return true; }
        }

        protected HttpRequestPost Peek
        {
            get { return requests.First.Value; }
        }
        protected HttpRequestPost Dequeue
        {
            get
            {
                var link = requests.First.Value;
                dataLength = -1;
                requests.RemoveFirst();
                return link;
            }
        }
        public override bool CanRead
        {
            get
            {
                if (dataLength <= 0)
                {
                    if (requests.Count > 0 && Peek.HasResponse)
                    {
                        dataLength = (int)Peek.Response.ContentLength;
                    }
                }
                return dataLength > 0;
            }
        }
        protected override int DataLength
        {
            get { return dataLength; }
        }

        public LinkHttpRequestShort()
        {
        }
        public LinkHttpRequestShort(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                throw new ArgumentNullException("uri");
            this.Uri = uri;
        }

        protected override int InternalRead(byte[] buffer, int offset, int size)
        {
            try
            {
                int read = 0;
                int __size = size;
                while (read < size)
                {
                    int tempRead = Peek.Response.GetResponseStream().Read(buffer, offset, __size);
                    read += tempRead;
                    offset += tempRead;
                    __size -= tempRead;
                    if (tempRead == 0)
                        break;
                }
                dataLength -= read;
                if (dataLength == 0)
                    requests.RemoveFirst();
                return read;
            }
            catch (WebException ex)
            {
                _LOG.Error(ex, "Read http data error. ErrorCode: {0}", ex.Status);
                throw ex;
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected override void InternalFlush(byte[] buffer)
        {
            GetRequestLink().Send(buffer);
        }
        protected virtual void PrepareRequest(HttpRequestPost request)
        {
            request.OnConnect += RequestOnConnect;
            if (OnRequestPrepare != null)
                OnRequestPrepare(request);
            requests.AddFirst(request);
        }
        protected HttpRequestPost GetRequestLink()
        {
            HttpRequestPost request = new HttpRequestPost();
            PrepareRequest(request);
            int reconnect = 0;
            request.OnError += (r, ex, buffer) =>
            {
                if (ex.Status == WebExceptionStatus.Timeout)
                {
                    _LOG.Debug("请求超时:byte[{0}], Timeout:{1}ms", buffer.Length, Timeout);
                    reconnect++;
                    if (ReconnectCount < 0 || reconnect <= ReconnectCount)
                    {
                        request.Reconnect();
                        request.Send(buffer);
                        return;
                    }
                    else
                    {
                        _LOG.Debug("放弃重连");
                    }
                }
                else if (ex.Status == WebExceptionStatus.RequestCanceled)
                {
                    _LOG.Debug("request canceled");
                }
                else if (ex.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    _LOG.Debug("服务器不存在");
                }
                else
                {
                    _LOG.Debug("request error! msg={0} code={1}", ex.Message, ex.Status);
                }
                Dequeue.Dispose();
            };
            request.Connect(Uri);
            return request;
        }
        private void RequestOnConnect(HttpWebRequest obj)
        {
            obj.Timeout = (int)Timeout.TotalMilliseconds;
        }
        public override void Close()
        {
            base.Close();
            dataLength = -1;
            while (requests.Count > 0)
                Dequeue.Dispose();
        }
    }
    public class LinkHttpRequest : LinkHttpRequestShort, IConnector
    {
        private static byte[] REQUEST_ID = { 255, 255 };

        private byte[] idBuffer;
        /// <summary>保持一个给服务端用于推送消息的长连接时间，小于等于0则不保持长连接</summary>
        public TimeSpan KeepAlive = TimeSpan.FromMinutes(60);
        private bool toKeepAlive;

        public ushort ID
        {
            get;
            private set;
        }
        public override bool IsConnected
        {
            get { return idBuffer != null; }
        }

        public override byte[] Read()
        {
            byte[] data = base.Read();
            // 保持一个长连接
            if (requests.Count == 0 && KeepAlive.Ticks > 0)
            {
                toKeepAlive = true;
                GetRequestLink().Send(null);
            }
            return data;
        }
        protected override void PrepareRequest(HttpRequestPost request)
        {
            if (toKeepAlive)
            {
                request.OnSend += FlushWithID;
                request.OnConnect += KeepAliveOnConnect;
                requests.AddLast(request);
            }
            else
            {
                request.OnSend += FlushWithPID;
                request.OnConnect += RequestOnConnect;
                requests.AddFirst(request);
            }
        }
        private void KeepAliveOnConnect(HttpWebRequest obj)
        {
            obj.Timeout = (int)KeepAlive.TotalMilliseconds;
        }
        private void RequestOnConnect(HttpWebRequest obj)
        {
            obj.Timeout = (int)Timeout.TotalMilliseconds;
        }
        private void FlushWithID(HttpWebRequest request, Stream obj)
        {
            // 客户端ID
            obj.Write(idBuffer, 0, 2);
        }
        private void FlushWithPID(HttpWebRequest request, Stream obj)
        {
            // 客户端ID
            obj.Write(idBuffer, 0, 2);
            // 数据包ID
            idBuffer[2]++;
            obj.Write(idBuffer, 2, 1);
        }
        public AsyncData<Link> Connect(string host, ushort port)
        {
            if (!string.IsNullOrEmpty(Uri))
                throw new InvalidOperationException("Http has connected.");

            AsyncData<Link> async = new AsyncData<Link>();

            HttpRequestPost request = new HttpRequestPost();
            //request.OnSend += RequestID;
            request.OnReceived += (response) =>
            {
                byte[] buffer = new byte[3];
                using (var stream = response.GetResponseStream())
                    stream.Read(buffer, 0, 2);
                request.Dispose();

                this.Uri = host;
                this.ID = BitConverter.ToUInt16(buffer, 0);
                this.idBuffer = buffer;
                async.SetData(this);
            };
            request.Connect(host);
            request.Send(REQUEST_ID);

            return async;
        }
    }

    public interface IConnector
    {
        //void Connect(string host, ushort port);
        AsyncData<Link> Connect(string host, ushort port);
    }
    public class Connector
    {
        private int connectCount;
        /// <summary>重连间隔，单位ms</summary>
        public ushort ReconnectInterval = 2000;
        /// <summary>等待服务器回应登录数据的时间，单位ms</summary>
        public ushort WaitResponse = 5000;
        /// <summary>写入连接数据；是否断线重连；返回null则断开连接</summary>
        public event Func<bool, byte[]> OnConnectData;
        /// <summary>连接成功，通过返回的数据构建Agent；是否断线重连</summary>
        public event Func<bool, byte[], Agent> OnConnectSuccess;
        /// <summary>连接失败；是否断线重连，重连次数，返回是否继续重连</summary>
        public event Func<bool, int, bool> OnConnectFault;
        /// <summary>网络交互协议异常</summary>
        public event Action<Exception> OnAgentError;
        public bool Running { get; private set; }
        public bool IsConnected
        {
            get { return Agent != null && Agent.Link != null && Agent.Link.IsConnected; }
        }
        public Agent Agent { get; private set; }
        public string Host { get; private set; }
        public ushort Port { get; private set; }
        public COROUTINE Coroutine { get; private set; }
        public IConnector NetConnector { get; private set; }
        public IEnumerable<ICoroutine> Connect(EntryService entry, IConnector connector, string host, ushort port)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentNullException("ip");
            if (connector == null)
                throw new ArgumentNullException("connector");

            if (Running)
                Close();
            this.Running = true;
            this.Host = host;
            this.Port = port;
            this.NetConnector = connector;

            IEnumerable<ICoroutine> coroutine = Connect();
            if (entry != null)
            {
                Coroutine = entry.SetCoroutine(coroutine);
            }
            else
            {
                Coroutine = null;
            }
            return coroutine;
        }
        private IEnumerable<ICoroutine> Connect()
        {
            while (Running)
            {
                if (!IsConnected)
                {
                    bool reconnect = Agent != null;
                    if (reconnect)
                    {
                        _LOG.Info("正在尝试断线重连");
                    }
                    connectCount++;

                    #region 连接网络

                    // 连接网络
                    AsyncData<Link> async;
                    try
                    {
                        async = NetConnector.Connect(Host, Port);
                    }
                    catch (Exception ex)
                    {
                        _LOG.Error(ex, "创建网络连接失败 Connector={0} IP={1} Port={2}", NetConnector.GetType().FullName, Host, Port);
                        async = null;
                    }
                    if (async == null)
                    {
                        // 等待后重连
                        yield return new TIME(ReconnectInterval);
                        continue;
                    }
                    if (!async.IsEnd) yield return async;
                    if (async.IsSuccess)
                    {
                        Link link = async.Data;
                        if (OnConnectData != null)
                        {
                            try
                            {
                                byte[] data = OnConnectData(reconnect);
                                if (data != null)
                                {
                                    link.Write(data);
                                    link.Flush();
                                }
                            }
                            catch (Exception ex)
                            {
                                _LOG.Error(ex, "写入连接数据异常");
                                link.Close();
                            }
                            if (!link.IsConnected)
                            {
                                // 等待后重连
                                yield return new TIME(ReconnectInterval);
                                continue;
                            }
                        }
                        TIME over = new TIME(WaitResponse);
                        while (link.IsConnected)
                        {
                            byte[] buffer = link.Read();
                            if (buffer == null)
                            {
                                over.Update(GameTime.Time);
                                if (over.IsEnd)
                                {
                                    _LOG.Error("未能在时间内收到服务器的响应，关闭本次连接");
                                    link.Close();
                                    break;
                                }
                                else
                                {
                                    yield return null;
                                }
                            }
                            else
                            {
                                // 接收登录数据成功登录
                                if (OnConnectSuccess != null)
                                {
                                    Agent = OnConnectSuccess(reconnect, buffer);
                                    if (Agent == null)
                                    {
                                        //throw new ArgumentNullException("Agent");
                                        Close();
                                    }
                                    else
                                    {
                                        Agent.Link = link;
                                    }
                                }
                                else
                                    throw new ArgumentNullException("OnConnectSuccess");
                                break;
                            }
                        }
                    }
                    else
                    {
                        // 连接异常
                        if (async.IsFaulted)
                            _LOG.Error(async.FaultedReason, "连接网络失败");
                        else
                            _LOG.Warning("连接网络失败");
                    }

                    if (IsConnected)
                    {
                        // 连接成功
                        _LOG.Info("连接服务器成功 IP={0} Port={1}", Host, Port);
                    }
                    else
                    {
                        // 连接失败
                        if (OnConnectFault != null && !OnConnectFault(reconnect, connectCount))
                        {
                            _LOG.Info("停止重连服务器");
                            Close();
                        }
                        else
                        {
                            if (ReconnectInterval == 0)
                                yield return null;
                            else
                                yield return new TIME(ReconnectInterval);
                        }
                    }

                    #endregion
                }
                else
                {
                    connectCount = 0;
                    // 网络交互处理
                    try
                    {
                        Agent.Bridge();
                    }
                    catch (Exception ex)
                    {
                        _LOG.Error(ex, "网络交互异常");
                        if (OnAgentError != null)
                        {
                            OnAgentError(ex);
                        }
                    }
                    yield return null;
                }
            }
        }
        public void Close()
        {
            _LOG.Info("断开网络连接");
            connectCount = 0;
            if (Agent != null && Agent.Link != null)
            {
                Agent.Link.Close();
                Agent = null;
            }
            if (Coroutine != null)
            {
                Coroutine.Dispose();
                Coroutine = null;
            }
            Running = false;
        }
    }
    public abstract class Link : IDisposable
    {
        public static ushort MaxBuffer = 8192;
        public const int MAX_BUFFER_SIZE = sizeof(int);

        public abstract bool IsConnected { get; }
        public abstract bool CanRead { get; }
        public virtual IPEndPoint EndPoint { get { throw new NotImplementedException(); } }

        public void Write(byte[] data)
        {
            Write(data, 0, data.Length);
        }
        public abstract void Write(byte[] buffer, int offset, int size);
        protected internal abstract void WriteBytes(byte[] buffer, int offset, int size);
        public abstract byte[] Read();
        public abstract byte[] Flush();
        public abstract void Close();
        void IDisposable.Dispose()
        {
            Close();
        }
    }
    public class LinkMultiple : Link
    {
        internal IEnumerable<Link> Links;
        private CorEnumerator<byte[]> read;

        internal LinkMultiple()
        {
        }
        public LinkMultiple(IEnumerable<Link> links)
        {
            if (links == null)
                throw new ArgumentNullException("links");
            this.Links = links;
        }

        public override bool IsConnected
        {
            get
            {
                foreach (var link in Links)
                    if (link.IsConnected)
                        return true;
                return false;
            }
        }
        public override bool CanRead
        {
            get
            {
                foreach (var link in Links)
                    if (link.CanRead)
                        return true;
                return false;
            }
        }
        public override void Write(byte[] buffer, int offset, int size)
        {
            foreach (Link link in Links)
            {
                link.Write(buffer, offset, size);
            }
        }
        protected internal override void WriteBytes(byte[] buffer, int offset, int size)
        {
            foreach (Link link in Links)
            {
                link.WriteBytes(buffer, offset, size);
            }
        }
        public override byte[] Read()
        {
            if (read == null)
                read = new CorEnumerator<byte[]>(Links.SelectMany(l => ReadAll(l)));

            if (read.IsEnd)
            {
                read = null;
                return null;
            }

            byte[] data;
            read.Update(out data);
            return data;
        }
        private IEnumerable<byte[]> ReadAll(Link link)
        {
            while (true)
            {
                if (link.CanRead)
                {
                    byte[] data = link.Read();
                    if (data != null)
                        yield return data;
                    else
                        yield break;
                }
                else
                    yield break;
            }
        }
        public override byte[] Flush()
        {
            foreach (Link link in Links)
            {
                link.Flush();
            }
            return null;
        }
        public override void Close()
        {
            foreach (Link link in Links)
            {
                link.Close();
            }
        }
    }
    public abstract class LinkLink : Link
    {
        public virtual Link BaseLink
        {
            get;
            set;
        }
        public override bool IsConnected
        {
            get { return BaseLink.IsConnected; }
        }
        public override bool CanRead
        {
            get { return BaseLink.CanRead; }
        }
        public override IPEndPoint EndPoint
        {
            get { return BaseLink.EndPoint; }
        }

        public LinkLink()
        {
        }
        public LinkLink(Link baseLink)
        {
            this.BaseLink = baseLink;
        }

        public override void Write(byte[] buffer, int offset, int size)
        {
            BaseLink.Write(buffer, offset, size);
        }
        protected internal override void WriteBytes(byte[] buffer, int offset, int size)
        {
            BaseLink.WriteBytes(buffer, offset, size);
        }
        public override byte[] Read()
        {
            return BaseLink.Read();
        }
        public override byte[] Flush()
        {
            return BaseLink.Flush();
        }
        public override void Close()
        {
            BaseLink.Close();
        }
    }
    public class ExceptionCRC : Exception
    {
        public uint Get { get; private set; }
        public uint Calc { get; private set; }
        public ExceptionCRC(uint get, uint calc)
            : base("crc invalid!")
        {
            this.Get = get;
            this.Calc = calc;
        }
    }
    public class ExceptionHeartbeatStop : Exception { }
    public abstract class LinkBinary : Link
    {
        public static uint MAX_SIZE = 1024 * 1024 * 1024;
        private static byte[] HeartbeatProtocol = new byte[0];
        protected byte[] buffer = new byte[MaxBuffer];
        protected ByteWriter writer = new ByteWriter(MaxBuffer);
        private int bigData = -1;
        /// <summary>
        /// <para>大于0. 到时间会发送心跳包，包发不出去将结束心跳</para>
        /// <para>等于0. 不关心心跳</para>
        /// <para>小于0. 到时间(绝对值)会直接视为断线（抛出ExceptionHeartbeatStop异常）</para>
        /// </summary>
        public TimeSpan Heartbeat;
        // 读取到数据后重置心跳时间
        private bool beat = true;
        private DateTime lastBeat;
        public bool ValidateCRC = true;

        protected abstract int DataLength { get; }
        public bool HasBigData
        {
            get { return bigData >= 0; }
        }
        protected virtual bool CanFlush { get { return writer.Position > 0; } }

        public void Beat()
        {
            beat = true;
        }
        public override void Write(byte[] buffer, int offset, int size)
        {
            int count = size + MAX_BUFFER_SIZE;
            if (size != 0 && ValidateCRC)
                count += 4;
            //if (count > MaxBuffer)
            //    throw new ArgumentOutOfRangeException("package size");
            // cache full, flush the cache
            //if (writer.Position + count > MaxBuffer)
            //    Flush();
            // write package size
            writer.Write(count);
            // write package content
            writer.WriteBytes(buffer, offset, size);
            // write crc valid while package has length
            if (size != 0 && ValidateCRC)
                writer.Write(_IO.Crc32(buffer, offset, size));
            //if (writer.Position > MaxBuffer)
            //{
            //    // big data
            //    Flush();
            //    writer = new ByteWriter(MaxBuffer);
            //}
        }
        protected internal sealed override void WriteBytes(byte[] buffer, int offset, int size)
        {
            writer.WriteBytes(buffer, offset, size);
        }
        public override byte[] Read()
        {
            int available = DataLength;
            if (available >= MAX_BUFFER_SIZE)
            {
                beat = true;

                if (HasBigData)
                    return ReadBigData(ref available);

                int peek;
                int receive = PeekSize(buffer, out peek);
                if (receive == 0)
                {
                    // no package
                    return null;
                }
                int size = BitConverter.ToInt32(buffer, 0);
                if (size < MAX_BUFFER_SIZE)
                {
                    throw new ArgumentOutOfRangeException("size");
                }
                if (size == MAX_BUFFER_SIZE)
                {
                    if (peek < receive)
                    {
                        InternalRead(buffer, peek, receive);
                    }
                    // 心跳包：忽略心跳包，读取下一个包
                    // 不主动发心跳包的需要回应心跳包
                    if (Heartbeat.Ticks <= 0)
                        Write(HeartbeatProtocol);
                    return Read();
                }
                if (size > buffer.Length)
                {
                    if (size > MAX_SIZE)
                    {
                        _LOG.Error("数据包过大: {0}", size);
                        throw new InvalidOperationException("数据包过大");
                    }
                    // buffer为最终数据包
                    if (ValidateCRC)
                        buffer = new byte[size - MAX_BUFFER_SIZE - 4];
                    else
                        buffer = new byte[size - MAX_BUFFER_SIZE];
                    bigData = 0;
                    // 去掉包头
                    if (peek < receive)
                        InternalRead(buffer, peek, receive);
                    available -= receive;
                    return ReadBigData(ref available);
                }
                if (size <= available)
                {
                    int packageSize = size - peek;
                    receive = InternalRead(buffer, peek, packageSize);
                    if (receive != packageSize)
                    {
                        throw new ArgumentException("receive size invalid!");
                    }
                    if (ValidateCRC)
                    {
                        packageSize = size - MAX_BUFFER_SIZE - 4;
                        uint getCrc = BitConverter.ToUInt32(buffer, size - 4);
                        uint calcCrc = _IO.Crc32(buffer, MAX_BUFFER_SIZE, packageSize);
                        if (getCrc != calcCrc)
                        {
                            throw new ExceptionCRC(getCrc, calcCrc);
                        }
                    }
                    else
                    {
                        packageSize = size - MAX_BUFFER_SIZE;
                    }

                    byte[] package = new byte[packageSize];
                    Utility.Copy(buffer, MAX_BUFFER_SIZE, package, 0, packageSize);
                    return package;
                }
                // stream has not received all data.
            }
            return null;
        }
        private byte[] ReadBigData(ref int available)
        {
            // 读取大数据
            if (bigData < buffer.Length)
            {
                int canReceive = _MATH.Min(buffer.Length - bigData, available);
                InternalRead(buffer, bigData, canReceive);
                bigData += canReceive;
                available -= canReceive;
            }
            // 读取包尾crc验证
            if (bigData == buffer.Length && (!ValidateCRC || available >= 4))
            {
                if (ValidateCRC)
                {
                    byte[] crc = new byte[4];
                    InternalRead(crc, 0, 4);
                    uint getCrc = BitConverter.ToUInt32(crc, 0);
                    uint calcCrc = _IO.Crc32(buffer, 0, buffer.Length);
                    if (getCrc != calcCrc)
                        throw new ExceptionCRC(getCrc, calcCrc);
                }
                bigData = -1;
                byte[] bigPackage = buffer;
                buffer = new byte[MaxBuffer];
                return bigPackage;
            }
            // stream has not received all data.
            return null;
        }
        public sealed override byte[] Flush()
        {
            long tick = Heartbeat.Ticks;
            if (tick != 0)
            {
                // 定时发送心跳包
                if (beat)
                {
                    // 重置心跳时间
                    lastBeat = DateTime.Now;
                    beat = false;
                }
                else
                {
                    if (tick > 0)
                    {
                        // 检查是否该心跳了
                        var now = DateTime.Now;
                        if (now - lastBeat > Heartbeat)
                        {
                            lastBeat = now;
                            Write(HeartbeatProtocol);
                        }
                    }
                    else
                    {
                        // 心跳到时关闭连接
                        var now = DateTime.Now;
                        if ((now - lastBeat).Ticks > -tick)
                        {
                            throw new ExceptionHeartbeatStop();
                        }
                    }
                }
            }
            if (!CanFlush)
                return null;
            byte[] buffer = writer.GetBuffer();
            writer.Reset();
            if (IsConnected)
                InternalFlush(buffer);
            if (writer.Position > MaxBuffer)
                writer = new ByteWriter(MaxBuffer);
            return buffer;
        }
        protected virtual int PeekSize(byte[] buffer, out int peek)
        {
            int read = InternalRead(buffer, 0, MAX_BUFFER_SIZE);
            peek = read;
            return read;
        }
        protected abstract int InternalRead(byte[] buffer, int offset, int size);
        protected abstract void InternalFlush(byte[] buffer);
        public override void Close()
        {
            writer.Reset();
            bigData = -1;
        }
    }
    /// <summary>将本地写入的数据进行读取</summary>
    public class LinkBinaryLocal : LinkBinary
    {
        private int read;

        public override bool IsConnected
        {
            get { return true; }
        }
        public override bool CanRead
        {
            get { return read < writer.Position; }
        }
        protected override int DataLength
        {
            get { return writer.Position; }
        }

        protected override int InternalRead(byte[] buffer, int offset, int size)
        {
            read += size;
            Array.Copy(writer.Buffer, 0, buffer, offset, size);
            return size;
        }
        protected override void InternalFlush(byte[] buffer)
        {
            read = 0;
        }
    }
    /// <summary>保护短时间内不重复发相同的数据</summary>
    public sealed class LinkRepeatPreventor : LinkLink
    {
        private class Package : PoolItem
        {
            public byte[] Buffer;
            public TIMER Timer;
        }

        private Pool<Package> packages = new Pool<Package>();
        public TimeSpan PreventTime = TimeSpan.FromSeconds(1);

        public override void Write(byte[] buffer, int offset, int size)
        {
            byte[] target = new byte[size];
            buffer.Copy(0, target, offset, size);
            foreach (Package package in packages)
            {
                if (package.Timer.ElapsedNow >= PreventTime)
                {
                    packages.RemoveAt(package);
                }
                else
                {
                    if (Utility.Equals(package.Buffer, target))
                    {
                        return;
                    }
                }
            }
            Package newP = new Package();
            newP.Buffer = target;
            newP.Timer = TIMER.StartNew();
            packages.Add(newP);
            base.Write(buffer, offset, size);
        }
    }
    /// <summary>一段时间内若有其他包则一起写入后发送</summary>
    public sealed class LinkBatchDelay : LinkLink
    {
        private TIMER time = new TIMER();
        public TimeSpan Delay = TimeSpan.FromMilliseconds(250);

        private void ResetTime()
        {
            time.Start();
        }
        public override void Write(byte[] buffer, int offset, int size)
        {
            ResetTime();
            base.Write(buffer, offset, size);
        }
        protected internal override void WriteBytes(byte[] buffer, int offset, int size)
        {
            ResetTime();
            base.WriteBytes(buffer, offset, size);
        }
        public override byte[] Flush()
        {
            if (time.Running)
            {
                if (time.ElapsedNow < Delay)
                    return null;
                time.Stop();
                return base.Flush();
            }
            else
                return null;
        }
    }
    
    //public class LinkHttpRequest : LinkBinary
    //{
    //    /// <summary>超时时间(ms)，小于0则不超时</summary>
    //    public int Timeout = 5000;
    //    /// <summary>重连次数，小于0则无限重连</summary>
    //    public int ReconnectCount = 3;
    //    private int reconnect;
    //    private int length;
    //    public HttpRequestPost Http
    //    {
    //        get;
    //        private set;
    //    }
    //    protected override int DataLength
    //    {
    //        get { return length; }
    //    }
    //    public override bool IsConnected
    //    {
    //        get { return Http.IsConnected; }
    //    }
    //    public override bool CanRead
    //    {
    //        get
    //        {
    //            //if (request == null && Ex != null)
    //            //{
    //            //    throw Ex;
    //            //}
    //            return Http.HasResponse && length > 0;
    //        }
    //    }
    //    public WebException Ex
    //    {
    //        get;
    //        private set;
    //    }

    //    public LinkHttpRequest()
    //    {
    //        Http = new HttpRequestPost();
    //        Http.OnConnect += Http_OnConnect;
    //        Http.OnError += Http_OnError;
    //        Http.OnReceived += Http_OnReceived;
    //    }

    //    void Http_OnReceived(HttpWebResponse obj)
    //    {
    //        length = (int)obj.ContentLength;
    //    }
    //    void Http_OnError(HttpRequestPost request, WebException ex, byte[] buffer)
    //    {
    //        Ex = ex;
    //        if (ex.Status == WebExceptionStatus.Timeout)
    //        {
    //            _LOG.Debug("请求超时:byte[{0}], Timeout:{1}ms", buffer.Length, Timeout);
    //            reconnect++;
    //            if (ReconnectCount < 0 || reconnect <= ReconnectCount)
    //            {
    //                Http.Reconnect();
    //                InternalFlush(buffer);
    //            }
    //        }
    //        else if (ex.Status == WebExceptionStatus.RequestCanceled)
    //        {
    //            _LOG.Debug("request canceled");
    //        }
    //        else if (ex.Status == WebExceptionStatus.NameResolutionFailure)
    //        {
    //            _LOG.Debug("服务器不存在");
    //        }
    //        else
    //        {
    //            _LOG.Debug("request error! msg={0} code={1}", ex.Message, ex.Status);
    //        }
    //    }
    //    void Http_OnConnect(HttpWebRequest obj)
    //    {
    //        obj.Timeout = Timeout;
    //    }

    //    protected override void InternalFlush(byte[] buffer)
    //    {
    //        Http.Send(buffer);
    //    }
    //    protected override int InternalRead(byte[] buffer, int offset, int size)
    //    {
    //        try
    //        {
    //            int read = Http.Response.GetResponseStream().Read(buffer, offset, size);
    //            length -= read;
    //            return read;
    //        }
    //        catch (WebException ex)
    //        {
    //            Ex = ex;
    //            _LOG.Error(ex, "Read error!");
    //            throw ex;
    //        }
    //    }
    //    public void Connect(string uri)
    //    {
    //        Http.Connect(uri);
    //    }
    //    public override void Close()
    //    {
    //        base.Close();
    //        Http.Dispose();
    //    }
    //}
    public class HttpRequestPost : IDisposable
    {
        public event Action<HttpWebRequest> OnConnect;
        /// <summary>异步</summary>
        public event Action<HttpWebRequest, Stream> OnSend;
        /// <summary>异步</summary>
        public event Action<HttpWebRequest, Stream> OnSent;
        /// <summary>异步</summary>
        public event Action<HttpWebResponse> OnReceived;
        /// <summary>异步</summary>
        public event Action<HttpRequestPost, WebException, byte[]> OnError;
        public object Tag;
        private Uri uri;

        public bool IsConnected
        {
            get { return Request != null; }
        }
        public HttpWebRequest Request
        {
            get;
            private set;
        }
        public bool HasResponse
        {
            get { return Response != null; }
        }
        public HttpWebResponse Response
        {
            get;
            private set;
        }

        public void Reconnect()
        {
            Uri uri = Request == null ? this.uri : Request.RequestUri;
            Dispose();
            Connect(uri);
        }
        public void Connect(Uri uri)
        {
            if (Request != null)
                throw new InvalidOperationException();

            if (uri == null)
                throw new ArgumentNullException("uri");

            this.uri = uri;
            Request = (HttpWebRequest)HttpWebRequest.Create(uri);
            Request.ContentType = "application/x-www-form-urlencoded";
            Request.Method = "POST";
            if (OnConnect != null)
                OnConnect(Request);
        }
        public void Connect(string uri)
        {
            Connect(new Uri(uri));
        }
        public void Send(byte[] buffer)
        {
            Request.BeginGetRequestStream(
                ar =>
                {
                    Stream connection = null;
                    try
                    {
                        // .net3.5: RequestStream不关闭，GetResponse不能发出请求
                        // .net4.0: RequestStream不关闭，GetResponse可以发出请求，此时KeepAlive=true
                        connection = Request.EndGetRequestStream(ar);
                        if (OnSend != null)
                            OnSend(Request, connection);
                        if (buffer != null && buffer.Length > 0)
                            connection.Write(buffer, 0, buffer.Length);
                        if (OnSent != null)
                            OnSent(Request, connection);
                    }
                    catch (WebException ex)
                    {
                        Dispose();
                        if (OnError != null)
                            OnError(this, ex, buffer);
                    }
                    catch (Exception ex)
                    {
                        Dispose();
                        _LOG.Error(ex, "HttpRequestPost write stream data error!");
                    }
                    finally
                    {
                        if (connection != null)
                        {
                            connection.Close();
                        }
                    }

                    if (connection == null) return;

                    Request.BeginGetResponse((ar2) =>
                    {
                        try
                        {
                            var response = (HttpWebResponse)Request.EndGetResponse(ar2);
                            this.Response = response;
                            if (OnReceived != null)
                                OnReceived(response);
                        }
                        catch (WebException ex)
                        {
                            Dispose();
                            if (OnError != null)
                                OnError(this, ex, buffer);
                        }
                        catch (Exception ex)
                        {
                            Dispose();
                            _LOG.Error(ex, "HttpRequestPost send error!");
                        }
                    }, Request);

                }, Request);
        }
        public void Dispose()
        {
            if (Request != null)
            {
                Request.Abort();
                Request = null;
            }
            if (Response != null)
            {
                Response.Close();
                Response = null;
            }
        }
    }
    //public abstract class LinkQueue<T> : LinkLink where T : Link
    //{
    //    protected LinkedList<T> queue
    //    {
    //        get;
    //        private set;
    //    }
    //    public sealed override bool CanRead
    //    {
    //        get
    //        {
    //            if (BaseLink != null)
    //                return base.CanRead;
    //            if (queue.Count == 0)
    //                return false;
    //            var link = queue.First.Value;
    //            if (!link.IsConnected)
    //                //throw new WebException("Can't receive any data.", WebExceptionStatus.Timeout);
    //                return true;
    //            return link.CanRead;
    //        }
    //    }
    //    public virtual bool CanFlush
    //    {
    //        get { return BaseLink != null && !BaseLink.CanRead; }
    //    }

    //    public LinkQueue()
    //    {
    //        queue = new LinkedList<T>();
    //    }

    //    protected T Dequeue()
    //    {
    //        var link = queue.First.Value;
    //        queue.RemoveFirst();
    //        return link;
    //    }
    //    public sealed override byte[] Flush()
    //    {
    //        if (!CanFlush)
    //            return null;
    //        byte[] flush = base.Flush();
    //        if (flush == null)
    //            return flush;
    //        FlushOver(flush);
    //        BaseLink = null;
    //        return flush;
    //    }
    //    protected abstract void FlushOver(byte[] flush);
    //    public override void Close()
    //    {
    //        if (BaseLink != null)
    //            base.Close();
    //        if (queue == null)
    //            return;
    //        while (queue.Count > 0)
    //            Dequeue().Close();
    //        queue = null;
    //    }
    //}
    //public class LinkLongHttpRequest : LinkQueue<LinkHttpRequest>, IConnector
    //{
    //    private static byte[] REQUEST_ID;

    //    private byte[] idBuffer;
    //    public TimeSpan Timeout = TimeSpan.FromSeconds(5);
    //    public int ReconnectCount = 3;
    //    public TimeSpan KeepAlive = TimeSpan.FromMinutes(60);

    //    public ushort ID
    //    {
    //        get;
    //        private set;
    //    }
    //    public string Uri
    //    {
    //        get;
    //        private set;
    //    }
    //    public override bool IsConnected
    //    {
    //        get { return idBuffer != null; }
    //    }

    //    private LinkHttpRequest GetRequestLink(bool keepAlive)
    //    {
    //        LinkHttpRequest request = new LinkHttpRequest();
    //        if (keepAlive)
    //        {
    //            request.Http.OnSend += FlushWithID;
    //            request.Timeout = (int)KeepAlive.TotalMilliseconds;
    //            request.ReconnectCount = -1;
    //        }
    //        else
    //        {
    //            request.Http.OnSend += FlushWithPID;
    //            request.Timeout = (int)Timeout.TotalMilliseconds;
    //            request.ReconnectCount = ReconnectCount;
    //        }
    //        request.Connect(Uri);
    //        return request;
    //    }
    //    private void FlushWithID(HttpWebRequest request, Stream obj)
    //    {
    //        // 客户端ID
    //        obj.Write(idBuffer, 0, 2);
    //    }
    //    private void FlushWithPID(HttpWebRequest request, Stream obj)
    //    {
    //        // 客户端ID
    //        obj.Write(idBuffer, 0, 2);
    //        // 数据包ID
    //        idBuffer[2]++;
    //        obj.Write(idBuffer, 2, 1);
    //    }
    //    public Async Connect(string host, ushort port)
    //    {
    //        if (!string.IsNullOrEmpty(Uri))
    //            throw new InvalidOperationException("Http has connected.");

    //        AsyncData<LinkLongHttpRequest> async = new AsyncData<LinkLongHttpRequest>();

    //        HttpRequestPost request = new HttpRequestPost();
    //        //request.OnSend += RequestID;
    //        request.OnReceived += (response) =>
    //        {
    //            byte[] buffer = new byte[3];
    //            using (var stream = response.GetResponseStream())
    //                stream.Read(buffer, 0, 2);
    //            request.Dispose();

    //            this.Uri = host;
    //            this.ID = BitConverter.ToUInt16(buffer, 0);
    //            this.idBuffer = buffer;
    //            async.SetData(this);
    //        };
    //        request.Connect(host);
    //        request.Send(REQUEST_ID);

    //        return async;
    //    }
    //    public override void Write(byte[] buffer, int offset, int size)
    //    {
    //        if (BaseLink == null)
    //            BaseLink = GetRequestLink(false);
    //        base.Write(buffer, offset, size);
    //    }
    //    protected override void FlushOver(byte[] flush)
    //    {
    //        queue.AddFirst((LinkHttpRequest)BaseLink);
    //    }
    //    public override byte[] Read()
    //    {
    //        byte[] data;
    //        var link = queue.First.Value;
    //        if (link.IsConnected)
    //        {
    //            data = link.Read();
    //            if (data == null)
    //                return null;
    //            // 已经读取完所有数据
    //            if (!link.CanRead)
    //                Dequeue().Close();
    //        }
    //        else
    //        {
    //            Dequeue();
    //            data = null;
    //        }
    //        // 保持一个长连接
    //        if (queue.Count == 0)
    //        {
    //            LinkHttpRequest _keep = GetRequestLink(true);
    //            _keep.Http.Send(null);
    //            queue.AddLast(_keep);
    //        }
    //        return data;
    //    }

    //    static LinkLongHttpRequest()
    //    {
    //        REQUEST_ID = BitConverter.GetBytes(ushort.MaxValue);
    //    }
    //    //static void RequestID(HttpWebRequest request, Stream obj)
    //    //{
    //    //    obj.Write(REQUEST_ID, 0, 2);
    //    //}
    //}
    public class HttpException : Exception
    {
        public HttpStatusCode StatusCode
        {
            get;
            private set;
        }
        public HttpException(HttpStatusCode code, string msg) : base(msg)
        {
            this.StatusCode = code;
        }
        public HttpException(int code, string msg) : base(msg)
        {
            this.StatusCode = (HttpStatusCode)code;
        }
    }

    public class HttpError
    {
        public int errCode;
        public string errMsg;
        public HttpError() { }
        public HttpError(int code, string msg)
        {
            this.errCode = code;
            this.errMsg = msg;
        }
    }

    [AttributeUsage(AttributeTargets.Interface)]
    public class ProtocolStubAttribute : Attribute
    {
        private byte protocol;
        private Type callback;
        private string agentType;
        public byte Protocol { get { return protocol; } }
        public Type Callback { get { return callback; } }
        public string AgentType { get { return agentType; } }
        public ProtocolStubAttribute(byte protocol, Type callback)
        {
            this.protocol = protocol;
            this.callback = callback;
        }
        public ProtocolStubAttribute(byte protocol, Type callback, string agentType)
        {
            this.protocol = protocol;
            this.callback = callback;
            this.agentType = agentType;
        }
    }
}
