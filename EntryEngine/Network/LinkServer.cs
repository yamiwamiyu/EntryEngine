#if SERVER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using EntryEngine.Serialize;

namespace EntryEngine.Network
{
	/*
	 * 网络服务器流程
	 * 1. EntryLinkServer -> Proxy
	 * 2. Proxy -> Link -> IProxyService
	 * 3. IProxyService -> IAgent
	 * 4. Link -> Send -> Agent -> Receive -> Process
	 */
    [Code(ECode.MayBeReform)]
	public class EntryLinkServer : EntryService
	{
		private Queue<Action> actions = new Queue<Action>();
		private List<Proxy> proxies = new List<Proxy>();

        public int ProxyCount
        {
            get { return proxies.Count; }
        }
        public Proxy this[int index]
        {
            get { return proxies[index]; }
        }

		public void Post(Action action)
		{
			lock (actions)
			{
				actions.Enqueue(action);
			}
		}
		public void AddProxy(Proxy proxy)
		{
			if (proxy == null)
				throw new ArgumentNullException("proxy");
			proxies.Add(proxy);
			_LOG.Info("add proxy: Type = {0} ID = {1}", proxy.GetType().FullName, proxy.ID);
		}
        public void RemoveProxy(Proxy proxy)
        {
            proxies.Remove(proxy);
        }
		protected override void InternalUpdate()
		{
            lock (actions)
            {
                while (actions.Count > 0)
                {
                    try
                    {
                        actions.Dequeue()();
                    }
                    catch (Exception ex)
                    {
                        _LOG.Error(ex, "action error!");
                    }
                }
            }

            for (int i = 0; i < proxies.Count; i++)
            {
                proxies[i].Update(GameTime);
            }
		}
		public override void Dispose()
		{
			for (int i = 0; i < proxies.Count; i++)
			{
				proxies[i].Dispose();
			}
			actions.Clear();
			proxies.Clear();
		}
    }

	public enum EBlockReason
	{
		Unknow,
		AcceptCountOver,

		Relock = 128,
		ByGM,
		AcceptRefuse,
	}
	public class AcceptLimit
	{
		public string LimitIP;
		public ushort? LimitPort;

		public AcceptLimit()
		{
		}
		public AcceptLimit(string ip)
		{
			this.LimitIP = ip;
		}
		public AcceptLimit(ushort port)
		{
			this.LimitPort = port;
		}
		public AcceptLimit(string ip, ushort port)
		{
			this.LimitIP = ip;
			this.LimitPort = port;
		}
		public AcceptLimit(IPEndPoint endpoint)
		{
			this.LimitIP = endpoint.Address.ToString();
			this.LimitPort = (ushort)endpoint.Port;
		}

		public bool Permit(IPEndPoint endpoint)
		{
			if (!string.IsNullOrEmpty(LimitIP) &&
				LimitIP != IPAddress.Any.ToString() &&
				LimitIP != IPAddress.IPv6Any.ToString() &&
                !endpoint.Address.ToString().StartsWith(LimitIP))
			{
				return false;
			}

			if (LimitPort.HasValue &&
				LimitPort.Value != endpoint.Port)
			{
				return false;
			}

			return true;
		}
	}
	public class AcceptBlock
	{
		public AcceptLimit Block;
		public DateTime BlockTime;
		public TimeSpan? BlockDuration;
		public string Reason;

		public DateTime? ReleaseTime
		{
			get
			{
				if (BlockDuration.HasValue)
				{
					return BlockTime + BlockDuration.Value;
				}
				else
				{
					return null;
				}
			}
		}
		public bool IsRelease
		{
			get
			{
				if (BlockDuration.HasValue)
				{
					return DateTime.Now >= BlockTime + BlockDuration.Value;
				}
				else
				{
					return false;
				}
			}
		}
	}
    public class LoginResult
    {
        public EAcceptPermit Result;
        /// <summary>
        /// Agent is null: Not a new client
        /// </summary>
        public Agent Agent;
    }
    public enum EAcceptPermit
    {
        None,
        Handling,
        Block,
        Refuse,
        Permit
    }

    // proxy
	public abstract class Proxy
	{
		class NewLink : PoolItem
		{
			public TIMER Timeout;
			public Link Link;
            public IEnumerator<LoginResult> LoginProcess;

			public NewLink(Link link)
			{
				this.Link = link;
				this.Timeout.Start();
			}
		}

		private static TimeSpan SECOND = TimeSpan.FromSeconds(1);

		public event Func<AcceptBlock, IPEndPoint, EBlockReason, AcceptBlock> OnLock;
		private TIMER secondTimer = new TIMER();
		private Dictionary<IPAddress, int> loginCountPerSecondByIP = new Dictionary<IPAddress, int>();
		private Dictionary<string, Agent> clients = new Dictionary<string, Agent>();
		private Dictionary<IPAddress, AcceptBlock> blockIP = new Dictionary<IPAddress, AcceptBlock>();
		private Pool<NewLink> newLink = new Pool<NewLink>();
		private Queue<string> disconnected = new Queue<string>();

        // proxy configuration
        public int ID;
        /// <summary>限定最大接入数</summary>
        public uint? PermitAcceptCount;
        /// <summary>限定指定终端接入</summary>
        public AcceptLimit[] PermitAcceptEndPoint;
        /// <summary>连接验证包超时(ms)</summary>
        public uint LinkTimeout = 1000;
        /// <summary>限定每秒允许同一个IP的接入数</summary>
        public uint? PermitSameIPLinkPerSecord = 1;
        /// <summary>限定每秒允许同一个IP的接入数，超过则禁止该IP登录</summary>
        public uint SameIPLinkPerSecondBlock = 5;
        /// <summary>每帧Accept的最长时间</summary>
        public TimeSpan? PermitAcceptTimeout = TimeSpan.FromMilliseconds(1);
        /// <summary>每个Client接收处理数据包最长时间</summary>
        public TimeSpan? PermitReceiveTimeout = TimeSpan.FromMilliseconds(10);
        /// <summary>值为false时，OnUpdate的异常会被抛出，可能导致服务的关闭</summary>
        public bool CatchUpdate = true;

        public event Action<Link> ClientDisconnect;
        public event Action Stop;

		public Link Link
		{
            get
            {
                if (clients.Count == 0)
                    return null;
                return new LinkMultiple(clients.Values.Select(c => c.Link));
            }
		}
		protected bool Started
		{
			get;
			private set;
		}
		public ushort Port
		{
			get;
			private set;
		}
		public int ClientCount
		{
			get { return clients.Count; }
		}
		protected abstract bool HasAcceptRequest { get; }
		public Agent this[IPEndPoint endpoint]
		{
			get { return this[endpoint.ToString()]; }
		}
		public Agent this[string key]
		{
			get
			{
				Agent agent;
				clients.TryGetValue(key, out agent);
				return agent;
			}
		}

		public void Initialize(ushort port)
		{
			Initialize(IPAddress.Any, port);
		}
		public void Initialize(IPAddress address, ushort port)
		{
			Dispose();

			_LOG.Info("proxy:{2} start! address={0} port={1}", address, port, GetType().FullName);
			this.Started = true;
			this.Port = port;
			try
			{
				Launch(address, port);
			}
			catch (Exception ex)
			{
				_LOG.Error(ex, "proxy launch error!");
				Dispose();
			}
			secondTimer.StopAndStart();
		}
		protected abstract void Launch(IPAddress address, ushort port);
		protected abstract Link Accept();
		public void Update(GameTime time)
		{
			if (!Started)
				return;

			CheckAccept();

			CheckNewLink();

			CheckPackage();

			// game logic
            try
            {
                OnUpdate(time);
            }
            catch (Exception ex)
            {
                _LOG.Error(ex, "Game logic uncaught error.");
                if (!CatchUpdate)
                    throw ex;
            }

			CheckSend();

			CheckDisconnected();

			CheckBlockRelease();

			if (secondTimer.ElapsedNow.TotalSeconds > 1)
				ResetSecond();
		}
        protected abstract IEnumerator<LoginResult> Login(Link link);
        protected abstract void OnUpdate(GameTime time);
		private void CheckBlockRelease()
		{
			if (blockIP.Count > 0)
			{
				Queue<IPAddress> release = new Queue<IPAddress>();
				foreach (var item in blockIP)
				{
					AcceptBlock block = item.Value;
					if (block.IsRelease)
					{
						release.Enqueue(item.Key);
					}
				}
				while (release.Count > 0)
				{
					blockIP.Remove(release.Dequeue());
				}
			}
		}
		private void CheckDisconnected()
		{
			while (disconnected.Count > 0)
			{
				string key = disconnected.Dequeue();
                Agent agent = clients[key];
                try
                {
                    if (ClientDisconnect != null)
                        ClientDisconnect(agent.Link);
                }
                catch (Exception ex)
                {
                    _LOG.Error(ex, "{0} disconnect error!", key);
                }
                finally
                {
                    agent.Link.Close();
                    clients.Remove(key);
                }
				_LOG.Info("{0} disconnected!", key);
			}
		}
		private void CheckSend()
		{
			foreach (var item in clients)
			{
				try
				{
					byte[] flush = item.Value.Link.Flush();
				}
				catch (Exception ex)
				{
					_LOG.Error(ex, "send error! ep: {0}", item.Key);
					disconnected.Enqueue(item.Key);
				}
			}
		}
		private void CheckPackage()
		{
            foreach (var item in clients)
            {
                if (!item.Value.Link.IsConnected)
                {
                    disconnected.Enqueue(item.Key);
                    continue;
                }
                Agent agent = item.Value;
                try
                {
                    TIMER timer = TIMER.StartNew();
                    int count = 0;
                    foreach (byte[] data in agent.Receive())
                    {
                        count += data.Length;
                        agent.OnProtocol(data);
                        if (PermitReceiveTimeout.HasValue)
                        {
                            TimeSpan elapsed = timer.ElapsedNow;
                            if (elapsed > PermitReceiveTimeout.Value)
                            {
                                _LOG.Warning("{0} process timeout:{1}! byte count={2}", item.Key, elapsed.TotalMilliseconds, count);
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _LOG.Error(ex, "agent: {0} process error!", item.Key);
                    disconnected.Enqueue(item.Key);
                }
            }
		}
		private void CheckNewLink()
		{
			foreach (NewLink link in newLink)
			{
                // 在线人数过多不允许继续登录
				if (PermitAcceptCount.HasValue && clients.Count >= PermitAcceptCount.Value)
				{
                    _LOG.Info("limit user count: {0}, current user count: {1}", PermitAcceptCount.Value, clients.Count);
                    CloseNewLink(link);
					break;
				}

                if (!link.Link.IsConnected)
                {
                    _LOG.Info("New link {0} disconnected.", link.Link.EndPoint);
                    CloseNewLink(link);
                    continue;
                }

				try
				{
                    if (link.LoginProcess == null)
                    {
                        var process = Login(link.Link);
                        if (process == null)
                            throw new ArgumentNullException("LoginProcess");
                        link.LoginProcess = process;
                    }
                    bool last = !link.LoginProcess.MoveNext();
                    var result = link.LoginProcess.Current;
                    if (result != null)
                    {
                        // 防止LoginProcess中关闭Link
                        if (!link.Link.IsConnected)
                        {
                            _LOG.Info("Link has been closed in login process! ip={0}", link.Link.EndPoint);
                            CloseNewLink(link);
                            continue;
                        }

                        switch (result.Result)
                        {
                            case EAcceptPermit.Handling:
                                link.Timeout.Stop();
                                break;

                            case EAcceptPermit.Block:
                                Block(link.Link.EndPoint, EBlockReason.AcceptRefuse);
                                goto case EAcceptPermit.Refuse;

                            case EAcceptPermit.Refuse:
                                _LOG.Info("link valid false! ip={0}", link.Link.EndPoint);
                                CloseNewLink(link);
                                continue;

                            case EAcceptPermit.Permit:
                                // result.Agent为空的状况为短连接不重复算客户端
                                if (result.Agent != null)
                                {
                                    IPEndPoint endpoint = link.Link.EndPoint;
                                    _LOG.Info("{0} connected!", endpoint);
                                    OnConnect(ref result.Agent);
                                    clients.Add(endpoint.ToString(), result.Agent);
                                }
                                newLink.Remove(link);
                                continue;
                        }
                    }

                    if (last)
                    {
                        _LOG.Info("Check new link over but not connected! ip={0}", link.Link.EndPoint);
                        CloseNewLink(link);
                        continue;
                    }
				}
				catch (Exception ex)
				{
					_LOG.Error(ex, "check new link error! ep={0}", link.Link.EndPoint);
                    CloseNewLink(link);
                    continue;
				}

                // 登录时间过长不允许登录
				if (link.Timeout.Running)
				{
					if (link.Timeout.ElapsedNow.TotalMilliseconds > LinkTimeout)
					{
						_LOG.Warning("{0} login timeout", link.Link.EndPoint);
                        CloseNewLink(link);
                        continue;
					}
				}
			}
		}
        private void CloseNewLink(NewLink link)
        {
            //try
            //{
            //    OnClientDisconnect(link.Link);
            //}
            //catch (Exception ex)
            //{
            //    _LOG.Error(ex, "new link disconnect error!");
            //}
            //finally
            //{
            //    link.Link.Close();
            //    newLink.Remove(link);
            //}

            link.Link.Close();
            newLink.Remove(link);
        }
		protected virtual void OnConnect(ref Agent agent)
		{
		}
		protected virtual void OnAccept(ref Link link, int acceptCount)
		{
		}
		protected virtual void OnAcceptSuccess(Link link, int successCount)
		{
		}
		private void CheckAccept()
		{
			TIMER acceptStat = TIMER.StartNew();
			int acceptCount = 0;
			int acceptSuccessCount = 0;
			while (HasAcceptRequest)
			{
				if (acceptStat.ElapsedNow > PermitAcceptTimeout)
				{
					_LOG.Info("accept timeout! accept time: {0}ms / accept count: {1}", acceptStat.ElapsedNow.TotalMilliseconds, acceptCount);
					break;
				}

				Link client = null;
				try
				{
					client = Accept();
                    if (client == null)
                        continue;
					acceptCount++;
					OnAccept(ref client, acceptCount);
                    if (client == null)
                    {
                        _LOG.Debug("Accepted client is ignored.");
                        continue;
                    }

					_LOG.Debug("accept = {0}", client.EndPoint);

					IPEndPoint endPoint = client.EndPoint;
					string key = endPoint.ToString();
					IPAddress address = endPoint.Address;

                    // 封禁IP不能登录
					AcceptBlock block;
					if (blockIP.TryGetValue(address, out block))
					{
						_LOG.Warning("block {0} try accept!", address);
						if (OnLock != null)
						{
							blockIP[address] = OnLock(block, endPoint, EBlockReason.Relock);
						}
						client.Close();
						continue;
					}

                    // 记录每秒登录次数，若过于频繁则不允许登录甚至封禁
					if (!loginCountPerSecondByIP.ContainsKey(address))
						loginCountPerSecondByIP.Add(address, 0);
					int count = loginCountPerSecondByIP[address] + 1;
					loginCountPerSecondByIP[address] = count;
					if (count > PermitSameIPLinkPerSecord)
					{
						_LOG.Warning("{0} try connect {1} times in second!", address, count);
						client.Close();
						if (count > SameIPLinkPerSecondBlock)
						{
							_LOG.Info("block {0} because connect in second over!", address);
							Block(endPoint, EBlockReason.AcceptCountOver);
						}
						continue;
					}

                    // 只允许指定的终端登录
					if (PermitAcceptEndPoint != null)
					{
						bool permit = false;
						foreach (var item in PermitAcceptEndPoint)
						{
							if (item.Permit(endPoint))
							{
								permit = true;
								break;
							}
						}
						if (!permit)
						{
							_LOG.Warning("don't permit accept end point: {0}", key);
							client.Close();
							continue;
						}
					}

					//IAgent temp;
					//if (clients.TryGetValue(key, out temp))
					//{
					//    Log.Waring("connection repeat! end point={0}", key);
					//    temp.Link.Close();
					//    clients.Remove(key);
					//}

					newLink.Add(new NewLink(client));
					acceptSuccessCount++;
					OnAcceptSuccess(client, acceptSuccessCount);
				}
				catch (Exception ex)
				{
					_LOG.Error(ex, "accept server error!");
					if (client != null)
					{
						client.Close();
					}
				}
			}
		}
		private void ResetSecond()
		{
			secondTimer.StopAndStart();
			loginCountPerSecondByIP.Clear();
		}
		public void BlockIP(params IPAddress[] address)
		{
			foreach (IPAddress item in address)
			{
				if (!blockIP.ContainsKey(item))
				{
					_LOG.Info("block IP: {0} by gm", item);
					Block(new IPEndPoint(item, 0), EBlockReason.ByGM);
				}
			}
		}
		private void Block(IPEndPoint endpoint, EBlockReason reason)
		{
			AcceptBlock block = null;
			if (OnLock != null)
			{
				block = OnLock(null, endpoint, reason);
			}
			if (reason >= EBlockReason.Relock)
			{
				if (block == null)
				{
					block = new AcceptBlock();
					block.Block = new AcceptLimit(endpoint.Address.ToString());
					block.BlockTime = DateTime.Now;
					block.Reason = reason.ToString();
				}
			}
			if (block != null)
			{
				blockIP[endpoint.Address] = block;
                _LOG.Info("Block {0} to {1} by {2}.", endpoint, 
                    block.ReleaseTime.HasValue ? block.ReleaseTime.Value.ToString() : "forever", 
                    reason);
			}
		}
		public void ReleaseIP(params IPAddress[] address)
		{
			foreach (IPAddress item in address)
			{
				if (blockIP.Remove(item))
				{
					_LOG.Info("release IP: {0} by gm", item);
				}
			}
		}
		public void Dispose()
		{
			if (!Started)
				return;
			Started = false;

            try
            {
                _LOG.Info("proxy closed! ID: {0} Type: {1}", ID, GetType().FullName);
                InternalDispose();
            }
            catch (Exception ex)
            {
                _LOG.Error(ex, "Proxy InternalDispose Error!");
            }

			foreach (NewLink item in newLink)
			{
				item.Link.Close();
			}
			newLink.Clear();

			foreach (var item in clients)
			{
				disconnected.Enqueue(item.Key);
			}
			CheckDisconnected();
			clients.Clear();

            if (Stop != null)
                Stop();
		}
		protected abstract void InternalDispose();
	}
	public abstract class ProxyTcp : Proxy
	{
		protected Socket socket;
		public TimeSpan Heartbeat = TimeSpan.FromSeconds(60);
		public TimeSpan HeartbeatOvertime = TimeSpan.FromSeconds(90);

		protected override bool HasAcceptRequest
		{
			get { return socket.Poll(0, SelectMode.SelectRead); }
		}

		protected override void Launch(IPAddress address, ushort port)
		{
			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Bind(new IPEndPoint(address, port));
			socket.Listen(int.MaxValue);
		}
		protected override Link Accept()
		{
            return new LinkSocket(socket.Accept());
		}
		protected override void OnConnect(ref Agent agent)
		{
			AgentHeartbeat beat = new AgentHeartbeat(agent, true);
			beat.Heartbeat = Heartbeat;
			beat.HeartbeatOvertime = HeartbeatOvertime;
            agent = beat;
		}
		protected override void InternalDispose()
		{
			if (socket != null)
			{
				socket.Close();
				socket = null;
			}
		}
	}
	public abstract class ProxyTcpAsync : ProxyTcp
	{
		private Queue<Socket> accepts = new Queue<Socket>();

		protected override bool HasAcceptRequest
		{
			get { return accepts.Count > 0; }
		}

		protected override void Launch(IPAddress address, ushort port)
		{
			base.Launch(address, port);
			socket.BeginAccept(Accept, socket);
		}
		private void Accept(IAsyncResult ar)
		{
			try
			{
				Socket accept = socket.EndAccept(ar);
				lock (accepts)
				{
					accepts.Enqueue(accept);
				}
			}
			catch (Exception ex)
			{
				_LOG.Error("async accept error! msg={0}", ex.Message);
			}
			finally
			{
				if (Started)
				{
					socket.BeginAccept(Accept, socket);
				}
			}
		}
		protected override Link Accept()
		{
			lock (accepts)
			{
				return new LinkTcp(accepts.Dequeue());
			}
		}
	}
    public abstract class ProxyHttpAsync : Proxy
    {
        private HttpListener handle;
        private Queue<HttpListenerContext> contexts = new Queue<HttpListenerContext>();

        protected override bool HasAcceptRequest
        {
            get { return contexts.Count > 0; }
        }

        public string RelativeUrl(string url)
        {
            foreach (var item in handle.Prefixes)
                if (url.StartsWith(item))
                    return url.Substring(item.Length);
            return null;
        }
        public void RegistServeiceUriPrefix(string uri)
        {
            if (handle == null)
                handle = new HttpListener();
            _LOG.Info("Add Prefix: {0}", uri);
            handle.Prefixes.Add(uri);
        }
        public void Initialize()
        {
            if (handle == null)
                throw new InvalidOperationException("Must regist uri prefix.");
            Initialize(IPAddress.Any, 80);
        }
        protected override void Launch(IPAddress address, ushort port)
        {
            if (handle == null)
                handle = new HttpListener();
            handle.Prefixes.Add(string.Format("http://{0}:{1}/", address, port));
            handle.Start();
            handle.BeginGetContext(Accept, handle);
        }
        private void Accept(IAsyncResult ar)
        {
            try
            {
                HttpListenerContext context = handle.EndGetContext(ar);
                lock (contexts)
                {
                    contexts.Enqueue(context);
                }
            }
            catch (Exception ex)
            {
                _LOG.Error("async GetContext error! msg={0}", ex.Message);
            }
            finally
            {
                if (Started)
                {
                    handle.BeginGetContext(Accept, handle);
                }
            }
        }
        protected sealed override Link Accept()
        {
            HttpListenerContext context;
            IPEndPoint ep;
            string url;
            lock (contexts)
            {
                context = contexts.Dequeue();
                ep = context.Request.RemoteEndPoint;
                url = context.Request.Url.ToString();
            }

            try
            {
                return InternalAccept(context);
            }
            catch (Exception ex)
            {
                _LOG.Error(ex, "ProxyHttpAsync InternalAccept error! ip: {0}:{1} url: {2}", ep.Address.ToString(), ep.Port, url);
                context.Response.Close();
                throw ex;
            }
        }
        protected abstract Link InternalAccept(HttpListenerContext context);
        protected override void InternalDispose()
        {
            lock (contexts)
            {
                while (contexts.Count > 0)
                {
                    contexts.Dequeue().Response.Close();
                }
            }
            if (handle != null)
            {
                handle.Abort();
                handle.Close();
                handle = null;
            }
        }
    }
    public abstract class ProxyHttpShort : ProxyHttpAsync
    {
        protected override IEnumerator<LoginResult> Login(Link link)
        {
            throw new NotImplementedException();
        }
        protected override void OnUpdate(GameTime time)
        {
        }
    }
    public class ProxyHttpService : ProxyHttpShort
    {
        private Dictionary<string, AgentHttp> agents = new Dictionary<string, AgentHttp>();

        protected override Link InternalAccept(HttpListenerContext context)
        {
            string origin = context.Request.Url.OriginalString;
            string url = RelativeUrl(origin);
            if (url == null)
            {
                _LOG.Error("Invalid url: {0}", context.Request.Url.OriginalString);
                return null;
            }

            string path = context.Request.Url.LocalPath;
            foreach (var agent in agents)
            {
                foreach (var stub in agent.Value.protocols)
                {
                    int index = path.IndexOf(stub.Key);
                    if (index != -1 && path.Substring(index + 1) == agent.Key)
                    {
                        agent.Value.Context = context;
                        agent.Value.OnProtocol(null);
                        return null;
                    }
                }
            }
            return null;
        }
        public void AddAgent(string directory, AgentHttp agent)
        {
            if (agent == null)
                throw new ArgumentNullException("agent");
            agents.Add(directory, agent);
        }
    }
    [Code(ECode.BeNotTest)]
    public abstract class ProxyHttpLong : ProxyHttpAsync
    {
        class _LinkHttpResponse : LinkHttpResponse
        {
            static byte[] ID_BUFFER = new byte[1];

            public byte ID
            {
                get;
                private set;
            }
            public bool IsKeepAlive
            {
                get { return length == 0; }
            }

            public _LinkHttpResponse(HttpListenerContext context)
                : base(context)
            {
                this.length -= 2;
                if (!IsKeepAlive)
                {
                    Request.Read(ID_BUFFER, 0, 1);
                    this.ID = ID_BUFFER[0];
                    this.length -= 1;
                }
            }
        }
        class LinkLongHttpResponse : LinkQueue<_LinkHttpResponse>
        {
            class LinkFlush : LinkLink
            {
                public byte ID;
                public byte[] Flushed;
            }

            internal int PoolID;
            private Queue<Link> waitClosed = new Queue<Link>();
            private LinkFlush lastFlush = new LinkFlush();
            private IPEndPoint endPoint;

            public override bool IsConnected
            {
                get { return PoolID != -1; }
            }
            public override Link BaseLink
            {
                get { return lastFlush.BaseLink; }
                set { lastFlush.BaseLink = value; }
            }
            public override IPEndPoint EndPoint
            {
                get { return endPoint; }
            }
            public override bool CanFlush
            {
                get
                {
                    return BaseLink != null &&
                        (!BaseLink.CanRead || lastFlush.Flushed != null);
                }
            }

            public void Reconnect(IPEndPoint ep)
            {
                if (endPoint != null && ep != null)
                    throw new InvalidOperationException("Endpoint has been set.");
                endPoint = ep;
            }
            public void Enqueue(_LinkHttpResponse link)
            {
                if (link == null)
                    throw new ArgumentNullException("link");
                if (link.IsKeepAlive)
                    queue.AddLast(link);
                else
                    queue.AddFirst(link);
            }
            public override void Write(byte[] buffer, int offset, int size)
            {
                if (BaseLink == null)
                {
                    if (queue.Count == 0)
                        throw new ArgumentOutOfRangeException("没有可写的连接");
                    BaseLink = queue.First.Value;
                }
                base.Write(buffer, offset, size);
            }
            protected sealed override void FlushOver(byte[] flush)
            {
                if (!((_LinkHttpResponse)BaseLink).IsKeepAlive)
                    lastFlush.Flushed = flush;
                waitClosed.Enqueue(Dequeue());

                while (waitClosed.Count > 0)
                {
                    Link link = waitClosed.Peek();
                    if (link.IsConnected)
                    {
                        break;
                    }
                    else
                    {
                        waitClosed.Dequeue();
                    }
                }
            }
            public override byte[] Read()
            {
                _LinkHttpResponse link = null;
                try
                {
                    while (queue.Count > 0)
                    {
                        link = queue.First.Value;
                        if (link.IsConnected)
                            break;
                        Dequeue();
                    }
                    if (link == null || link.IsKeepAlive)
                        return null;
                    if (lastFlush.ID == link.ID)
                    {
                        if (lastFlush.Flushed == null)
                        {
                            _LOG.Warning("Repeated package.");
                            // 视为过期请求不受理
                            Dequeue().Close();
                        }
                        else
                        {
                            WriteBytes(lastFlush.Flushed, 0, lastFlush.Flushed.Length);
                        }
                        return null;
                    }
                    else
                    {
                        lastFlush.Flushed = null;
                        lastFlush.ID = link.ID;
                    }
                    return link.Read();
                }
                catch (HttpListenerException ex)
                {
                    // 客户端关闭了流，可能已经操作超时
                    _LOG.Debug("response queue read error! ex:{0} code={1}", ex.Message, ex.ErrorCode);
                    Dequeue().Close();
                    return null;
                }
            }
            public override void Close()
            {
                base.Close();
                while (waitClosed.Count > 0)
                    waitClosed.Dequeue().Close();
                endPoint = null;
            }
        }

        private static byte[] ID_BUFFER = new byte[2];
        private Pool<LinkLongHttpResponse> cpools = new Pool<LinkLongHttpResponse>(2048);
        public TimeSpan KeepAlive = TimeSpan.FromMinutes(5);

        public ProxyHttpLong()
        {
            this.ClientDisconnect += ProxyHttpLong_ClientDisconnect;
        }

        private void ProxyHttpLong_ClientDisconnect(Link obj)
        {
            LinkLongHttpResponse link = obj as LinkLongHttpResponse;
            if (link != null && link.PoolID != -1)
            {
                cpools.RemoveAt(link.PoolID);
                link.PoolID = -1;
            }
        }
        protected override Link InternalAccept(HttpListenerContext context)
        {
            context.Request.InputStream.Read(ID_BUFFER, 0, 2);
            ushort id = BitConverter.ToUInt16(ID_BUFFER, 0);
            _LinkHttpResponse link = new _LinkHttpResponse(context);
            if (id != 65535)
            {
                LinkLongHttpResponse client = cpools[id];
                if (client == null || client.PoolID == -1)
                {
                    _LOG.Warning("Invalid http connection id.");
                }
                else if (client.PoolID != id)
                {
                    _LOG.Warning("Incorrect http connection id.");
                }
                else if (!client.EndPoint.Address.Equals(link.EndPoint.Address))
                {
                    _LOG.Warning("Invalid http connection ip: {0}", link.EndPoint.Address.ToString());
                }
                else
                {
                    client.Enqueue(link);
                }
                return null;
            }
            else
            {
                LinkLongHttpResponse client;
                int index = cpools.Allot(out client);
                if (index == -1)
                {
                    client = new LinkLongHttpResponse();
                    index = cpools.Add(client);
                }
                if (index >= 65535)
                {
                    cpools.RemoveAt(index);
                    throw new OutOfMemoryException("Http connection pool is full.");
                }
                client.PoolID = index;

                client.Reconnect(link.EndPoint);
                //client.Write(BitConverter.GetBytes((ushort)index));
                //client.Flush();

                link.WriteBytes(BitConverter.GetBytes((ushort)index), 0, 2);
                link.Flush();   // Flush完成后会Close
                return client;
            }
        }
        protected override void OnConnect(ref Agent agent)
        {
            base.OnConnect(ref agent);
            AgentHeartbeat beat = new AgentHeartbeat(agent);
            beat.Heartbeat = KeepAlive;
            beat.HeartbeatOvertime = KeepAlive;
            agent = beat;
        }
    }
    [Code(ECode.ToBeContinue | ECode.MayBeReform | ECode.BeNotTest)]
    public abstract class ProxyUdp : Proxy
    {
        private byte[] empty = new byte[64];
        private Socket socket;

        protected override bool HasAcceptRequest
        {
            get { return socket.Poll(0, SelectMode.SelectRead); }
        }

        protected override void Launch(IPAddress address, ushort port)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(address, port));
        }
        protected override Link Accept()
        {
            if (socket.Available > empty.Length)
                empty = new byte[socket.Available];

            EndPoint endpoint = new IPEndPoint(0, 0);
            socket.ReceiveFrom(empty, SocketFlags.Peek, ref endpoint);

            IPEndPoint ep = (IPEndPoint)endpoint;

            return new LinkUdp(socket, ep);
        }
        protected override void OnConnect(ref Agent agent)
        {
            LinkUdp link = agent.Link as LinkUdp;
            if (link == null || link.Socket != socket)
                throw new InvalidOperationException("Can't change the udp client on agent.Link.");
        }
        protected override void InternalDispose()
        {
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
        }
    }

    // link
    public class LinkHttpResponse : LinkBinary
    {
        protected HttpListenerContext context;
        protected int length;
        private IPEndPoint endpoint;

        public HttpListenerContext Context
        {
            set
            {
                if (value == null)
                    throw new ArgumentNullException("context");

                if (CanRead)
                    throw new InvalidOperationException("");

                context = value;
                length = (int)context.Request.ContentLength64;
                //endpoint = new IPEndPoint(RealEndPoint.Address, 80);
                endpoint = context.Request.RemoteEndPoint;
            }
        }
        protected System.IO.Stream Request
        {
            get { return context.Request.InputStream; }
        }
        protected System.IO.Stream Response
        {
            get { return context.Response.OutputStream; }
        }
        public override bool IsConnected
        {
            get { return context != null; }
        }
        public override bool CanRead
        {
            get { return length > 0; }
        }
        protected override int DataLength
        {
            get { return length; }
        }
        public override IPEndPoint EndPoint
        {
            get { return endpoint; }
        }
        //public IPEndPoint RealEndPoint
        //{
        //    get { return context.Request.RemoteEndPoint; }
        //}
        protected override bool CanFlush
        {
            get { return base.CanFlush && length == 0; }
        }

        public LinkHttpResponse(HttpListenerContext context)
        {
            this.Context = context;
        }

        protected override int InternalRead(byte[] buffer, int offset, int size)
        {
            int read = Request.Read(buffer, offset, size);
            length -= read;
            return read;
        }
        protected override void InternalFlush(byte[] buffer)
        {
            context.Response.ContentLength64 = buffer.Length;
            context.Response.KeepAlive = false;
            //Response.Write(buffer, 0, buffer.Length);
            Response.BeginWrite(buffer, 0, buffer.Length, EndWrite, buffer);
        }
        protected void EndWrite(IAsyncResult ar)
        {
            try
            {
                Response.EndWrite(ar);
            }
            catch (WebException ex)
            {
                _LOG.Error(ex, "Response error! state={0}", ex.Status);
                if (ex.Status == WebExceptionStatus.ConnectionClosed)
                {
                    _LOG.Info("ConnectionClosed");
                    Close();
                    return;
                }
                byte[] buffer = (byte[])ar.AsyncState;
                Response.BeginWrite(buffer, 0, buffer.Length, EndWrite, buffer);
                return;
            }
            Close();
        }
        public override void Close()
        {
            if (context == null)
                return;
            base.Close();
            try
            {
                Request.Close();
                Response.Close();
            }
            catch
            {
            }
            context = null;
        }
    }
    internal abstract class LinkBinaryResponse : LinkBinary
    {
        private int read;
        private int length;
        private IPEndPoint ep;

        public override bool IsConnected
        {
            get { return true; }
        }
        public override bool CanRead
        {
            get { return length > 0; }
        }
        protected override int DataLength
        {
            get { return length; }
        }
        public override IPEndPoint EndPoint
        {
            get { return ep; }
        }

        public LinkBinaryResponse(IPEndPoint remote)
        {
            this.ep = remote;
        }

        public void SetData(byte[] data)
        {
            Array.Copy(data, 0, buffer, length, data.Length);
            length += data.Length;
        }
        protected override int InternalRead(byte[] buffer, int offset, int size)
        {
            read += size;
            if (read >= length)
            {
                length = 0;
                read = 0;
            }
            return size;
        }
    }
    internal class LinkBRUdp : LinkBinaryResponse
    {
        private Socket socket;

        public LinkBRUdp(Socket socket, IPEndPoint ep)
            : base(ep)
        {
            if (socket == null)
                throw new ArgumentNullException("socket");
            this.socket = socket;
        }

        protected override void InternalFlush(byte[] buffer)
        {
            SocketError error;
            socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, out error, null, null);
            if (error != SocketError.Success)
                throw new SocketException((int)error);
        }
    }

    // stub
    public class AgentHttp : Agent, IDisposable
    {
        protected static readonly byte[] ZERO = new byte[0];
        internal Dictionary<string, StubHttp> protocols = new Dictionary<string, StubHttp>();
        private Queue<HttpListenerContext> queue = new Queue<HttpListenerContext>();
        /// <summary>一次OnProtocol处理完一个请求时触发</summary>
        public Action OnReset;

        protected bool IsPost
        {
            get;
            private set;
        }
        protected System.Collections.Specialized.NameValueCollection Param
        {
            get;
            set;
        }
        public HttpListenerContext Context
        {
            get;
            internal set;
        }

        public AgentHttp()
        {
        }
        public AgentHttp(params StubHttp[] stubs)
        {
            foreach (var stub in stubs)
                AddAgent(stub);
        }

        public void AddAgent(StubHttp stub)
        {
            if (stub == null)
                throw new ArgumentNullException("stub");
            stub.ProtocolAgent = this;
            if (stub.Protocol == null)
                protocols.Add(stub.GetType().Name, stub);
            else
                protocols.Add(stub.Protocol, stub);
        }
        public void Enqueue(HttpListenerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            queue.Enqueue(context);
        }
        public sealed override IEnumerable<byte[]> Receive()
        {
            while (queue.Count > 0)
            {
                Context = queue.Dequeue();
                yield return ZERO;
            }
        }
        public sealed override void OnProtocol(byte[] package)
        {
            if (Context == null)
                throw new ArgumentNullException("Context");

            IsPost = Context.Request.HttpMethod == "POST";

            var segments = Context.Request.Url.Segments;
            if (segments.Length <= 1)
                throw new FormatException("URL必须包含'/'以标识接口名");

            string protocol;
            if (segments.Length == 2)
                protocol = string.Empty;
            else
            {
                string temp = segments[segments.Length - 2];
                protocol = temp.Substring(0, temp.Length - 1);
            }

            StubHttp agent;
            if (!protocols.TryGetValue(protocol, out agent))
                throw new NotImplementedException("no procotol: " + protocol);

            string stub = segments[segments.Length - 1];
            int index = stub.IndexOf('.');
            if (index != -1)
                stub = stub.Substring(0, index);

            agent[stub](Context);

            Context = null;
            Param = null;
            if (OnReset != null)
                OnReset();
        }
        internal string GetParam(string paramName)
        {
            if (IsPost)
                return ParseParamPost(paramName);
            else
                return ParseParamGet(paramName);
        }
        protected virtual string ParseParamGet(string paramName)
        {
            if (Param == null)
                Param = Context.Request.QueryString;
            return Param[paramName];
        }
        protected virtual string ParseParamPost(string paramName)
        {
            if (Param == null)
            {
                byte[] data = _IO.ReadStream(Context.Request.InputStream, (int)Context.Request.ContentLength64);
                Param = _NETWORK.ParseQueryString(_NETWORK.UrlDecode(data, Context.Request.ContentEncoding));
            }
            return Param[paramName];
        }
        public void Dispose()
        {
            while (queue.Count > 0)
                queue.Dequeue().Response.Close();
        }
    }
    public abstract class StubHttp
    {
        private Dictionary<string, Action<HttpListenerContext>> stubs = new Dictionary<string, Action<HttpListenerContext>>();
        protected internal string Protocol
        {
            get;
            protected set;
        }
        protected internal AgentHttp ProtocolAgent;
        public Link Link
        {
            get { return ProtocolAgent.Link; }
        }
        public HttpListenerContext Context
        {
            get { return ProtocolAgent.Context; }
        }
        internal Action<HttpListenerContext> this[string name]
        {
            get
            {
                Action<HttpListenerContext> result;
                if (!stubs.TryGetValue(name, out result))
                    throw new NotImplementedException("no stub: " + name);
                return result;
            }
        }
        protected StubHttp()
        {
        }
        public StubHttp(string protocol)
        {
            this.Protocol = protocol;
        }
        protected void AddMethod(string name, Action<HttpListenerContext> method)
        {
            stubs.Add(name, method);
        }
        public string GetParam(string paramName)
        {
            return ProtocolAgent.GetParam(paramName);
        }
        public void Response(object obj)
        {
            Response(JsonWriter.Serialize(obj));
        }
        public void Response(string text)
        {
            if (Context.Response.ContentEncoding == null)
                Context.Response.ContentEncoding = Context.Request.ContentEncoding;
            Response(Context.Response.ContentEncoding.GetBytes(text));
        }
        public void Response(byte[] buffer)
        {
            Response(buffer, 0, buffer.Length);
        }
        public void Response(byte[] buffer, int offset, int count)
        {
            Context.Response.OutputStream.BeginWrite(buffer, offset, count, null, Context);
        }
    }
}

#endif
