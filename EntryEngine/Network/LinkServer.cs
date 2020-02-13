#if SERVER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using EntryEngine.Serialize;
using System.Text;
using System.IO;
using System.Threading;
using System.Reflection;

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
    public enum EAcceptClose
    {
        /// <summary>服务器连接数过多</summary>
        LinkCountOver,
        /// <summary>登录流程中客户端关闭了连接</summary>
        ClientClose,
        /// <summary>登录流程中服务器关闭了连接</summary>
        ServerClose,
        /// <summary>登录被拒绝</summary>
        Refuse,
        /// <summary>登录流程结束也没能正确登录</summary>
        CannotLogin,
        /// <summary>登录流程发生了异常</summary>
        Exception,
        /// <summary>登录超时</summary>
        Timeout,
        /// <summary>登录完毕</summary>
        Login,
    }

    // proxy
	public abstract class Proxy
	{
		class NewLink : PoolItem
		{
			public TIMER Timeout;
			public Link Link;
            public IPEndPoint EndPoint;
            public IEnumerator<LoginResult> LoginProcess;

			public NewLink(Link link)
			{
				this.Link = link;
                this.EndPoint = link.EndPoint;
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
        public TimeSpan? PermitAcceptTimeout = TimeSpan.FromMilliseconds(10);
        /// <summary>每个Client接收处理数据包最长时间</summary>
        public TimeSpan? PermitReceiveTimeout = TimeSpan.FromMilliseconds(10);
        /// <summary>值为false时，OnUpdate的异常会被抛出，可能导致服务的关闭</summary>
        public bool CatchUpdate = true;
        /// <summary>
        /// <para>大于0. 到时间会发送心跳包，包发不出去将结束心跳</para>
        /// <para>等于0. 不关心心跳</para>
        /// <para>小于0. 到时间(绝对值)会直接视为断线</para>
        /// </summary>
        public TimeSpan Heartbeat = TimeSpan.FromSeconds(-60);

        public event Action<Link> ClientDisconnect;
        public event Func<Link, EAcceptClose, bool> AcceptDisconnect;
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
                    RemoveNewLink(link, true, EAcceptClose.LinkCountOver);
					break;
				}

                if (!link.Link.IsConnected)
                {
                    _LOG.Info("New link {0} disconnected.", link.Link.EndPoint);
                    RemoveNewLink(link, true, EAcceptClose.ClientClose);
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
                            RemoveNewLink(link, true, EAcceptClose.ServerClose);
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
                                RemoveNewLink(link, true, EAcceptClose.Refuse);
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
                                RemoveNewLink(link, false, EAcceptClose.Login);
                                continue;
                        }
                    }

                    if (last)
                    {
                        _LOG.Info("Check new link over but not connected! ip={0}", link.Link.EndPoint);
                        RemoveNewLink(link, true, EAcceptClose.CannotLogin);
                        continue;
                    }
				}
				catch (Exception ex)
				{
					_LOG.Error(ex, "check new link error! ep={0}", link.Link.EndPoint);
                    RemoveNewLink(link, true, EAcceptClose.Exception);
                    continue;
				}

                // 登录时间过长不允许登录
				if (link.Timeout.Running)
				{
					if (link.Timeout.ElapsedNow.TotalMilliseconds > LinkTimeout)
					{
						_LOG.Warning("{0} login timeout", link.Link.EndPoint);
                        RemoveNewLink(link, true, EAcceptClose.Timeout);
                        continue;
					}
				}
			}
		}
        private void RemoveNewLink(NewLink link, bool close, EAcceptClose status)
        {
            if (close)
                link.Link.Close();
            if (!newLink.Remove(link))
                return;
            if (AcceptDisconnect != null)
            {
                if (AcceptDisconnect(link.Link, status))
                {
                    _LOG.Info("AcceptDisconnect锁定用户 EndPoint={0}:{1} 关闭步骤={2}", link.EndPoint.Address.ToString(), link.EndPoint.Port, status);
                    Block(link.EndPoint, EBlockReason.AcceptRefuse);
                }
            }
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

                    LinkBinary heartbeatLink = client as LinkBinary;
                    if (heartbeatLink != null)
                        heartbeatLink.Heartbeat = Heartbeat;
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
        protected Queue<HttpListenerContext> Contexts { get { return contexts; } }

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
            Initialize(IPAddress.Any, 80);
        }
        protected override void Launch(IPAddress address, ushort port)
        {
            if (handle == null)
                handle = new HttpListener();
            if (handle.Prefixes.Count == 0)
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
        protected override Link Accept()
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
    public class ProxyHttpService2 : ProxyHttpAsync
    {
        private LinkHttpResponseShort link;
        public AgentProtocolStub Agent { get; private set; }

        public ProxyHttpService2()
        {
            link = new LinkHttpResponseShort();
            Agent = new AgentProtocolStub(link);
        }

        protected override IEnumerator<LoginResult> Login(Link link)
        {
            throw new NotImplementedException();
        }
        protected override Link InternalAccept(HttpListenerContext context)
        {
            string url = null;
            try
            {
                url = context.Request.RawUrl;
                // 函数路由
                link.Enqueue(context);
                /* 每帧需要手动发出客户端的请求数据 */
                while (link.CanRead)
                {
                    byte[] package = link.Read();
                    if (package != null)
                        Agent.OnProtocol(package);
                    else
                        break;
                }
                link.Flush();
            }
            catch (Exception ex)
            {
                lock (_LOG._Logger)
                    _LOG.Error(ex, "处理请求异常 URL: {0}", url);
            }
            return null;
        }
        protected override void OnUpdate(GameTime time) { }
    }
    public class ProxyHttpService : ProxyHttpAsync
    {
        public AgentHttp Agent;

        protected override IEnumerator<LoginResult> Login(Link link)
        {
            throw new NotImplementedException();
        }
        protected override Link InternalAccept(HttpListenerContext context)
        {
            if (Agent != null)
            {
                Agent.Context = context;
                Agent.OnProtocol(null);
            }
            return null;
        }
        protected override void OnUpdate(GameTime time) { }
    }
    public abstract class ProxyHttpLong : ProxyHttpAsync
    {
        private static byte[] ID_BUFFER = new byte[2];
        private Pool<LinkHttpResponse> cpools = new Pool<LinkHttpResponse>(2048);

        protected class LinkHttpResponse : LinkHttpResponseShort
        {
            public int PoolID = -1;
            private byte[] pushCached = new byte[8192];
            private int pushCacheIndex = 0;

            public override bool IsConnected
            {
                get { return PoolID != -1; }
            }
            public override bool CanRead
            {
                get
                {
                    if (dataLength <= 0)
                    {
                        if (requests.Count > 0)
                        {
                            dataLength = (int)Peek.Request.ContentLength64;
                            dataLength -= 2;
                            if (dataLength > 0)
                                dataLength--;
                        }
                    }
                    return dataLength > 0;
                }
            }

            public override void Enqueue(HttpListenerContext context)
            {
                Beat();
                int dataLength = (int)context.Request.ContentLength64;
                // 用户ID
                dataLength -= 2;
                if (dataLength == 0)
                {
                    // 保持的长连接
                    requests.AddLast(context);
                }
                else
                {
                    // 还有一位PackageID，暂时没用
                    int packageID = context.Request.InputStream.ReadByte();
                    requests.AddFirst(context);
                }
            }
        }

        public ProxyHttpLong()
        {
            this.ClientDisconnect += ProxyHttpLong_ClientDisconnect;
        }

        private void ProxyHttpLong_ClientDisconnect(Link obj)
        {
            LinkHttpResponse link = obj as LinkHttpResponse;
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
            if (id != 65535)
            {
                LinkHttpResponse client = cpools[id];
                if (client == null || client.PoolID == -1)
                {
                    _LOG.Warning("Invalid http connection id.");
                }
                else if (client.PoolID != id)
                {
                    _LOG.Warning("Incorrect http connection id.");
                }
                else if (!client.EndPoint.Address.Equals(context.Request.RemoteEndPoint.Address))
                {
                    _LOG.Warning("Invalid http connection ip: {0}", context.Request.RemoteEndPoint.Address.ToString());
                }
                else
                {
                    client.Enqueue(context);
                }
                return null;
            }
            else
            {
                LinkHttpResponse client;
                int index = cpools.Allot(out client);
                if (index == -1)
                {
                    client = new LinkHttpResponse();
                    client.endpoint = context.Request.RemoteEndPoint;
                    index = cpools.Add(client);
                }
                if (index >= 65535)
                {
                    cpools.RemoveAt(index);
                    throw new OutOfMemoryException("Http connection pool is full.");
                }
                client.PoolID = index;

                //client.Reconnect(link.EndPoint);
                //client.Write(BitConverter.GetBytes((ushort)index));
                //client.Flush();

                client.Enqueue(context);
                client.WriteBytes(BitConverter.GetBytes((ushort)index), 0, 2);
                client.Flush();
                return client;
            }
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
            int receive = socket.ReceiveFrom(empty, SocketFlags.None, ref endpoint);

            IPEndPoint ep = (IPEndPoint)endpoint;

            //LinkUdp link = new LinkUdp(socket, ep);
            LinkUdp link = new LinkUdp();
            link.Connect(ep.Address.ToString(), (ushort)ep.Port);
            link.WriteBytes(empty, 0, receive);
            link.Flush();

            return link;
        }
        protected override void OnConnect(ref Agent agent)
        {
            //LinkUdp link = agent.Link as LinkUdp;
            //if (link == null || link.Socket != socket)
            //    throw new InvalidOperationException("Can't change the udp client on agent.Link.");
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

    public class LinkHttpResponseShort : LinkBinary
    {
        /// <summary>由于短连接的特性，未能由服务器主动推送出去的数据将会被缓存，超过此尺寸时，缓存将被清空，负数则允许无限增大</summary>
        public static int MAX_PUSH_CACHE_SIZE = -1;

        protected LinkedList<HttpListenerContext> requests = new LinkedList<HttpListenerContext>();
        protected int dataLength = -1;
        internal IPEndPoint endpoint;
        private byte[] pushCached = new byte[8192];
        private int pushCacheIndex = 0;

        protected HttpListenerContext Peek
        {
            get { return requests.First.Value; }
        }
        protected HttpListenerContext Dequeue
        {
            get
            {
                var link = requests.First.Value;
                dataLength = -1;
                requests.RemoveFirst();
                return link;
            }
        }
        public override bool IsConnected
        {
            get { return true; }
        }
        public override bool CanRead
        {
            get
            {
                if (dataLength <= 0)
                {
                    if (requests.Count > 0)
                    {
                        dataLength = (int)Peek.Request.ContentLength64;
                    }
                }
                return dataLength > 0;
            }
        }
        protected override int DataLength
        {
            get { return dataLength; }
        }
        public override IPEndPoint EndPoint
        {
            get
            {
                if (requests.Count == 0)
                    return endpoint;
                else
                    return Peek.Request.RemoteEndPoint;
            }
        }
        //public IPEndPoint RealEndPoint
        //{
        //    get { return context.Request.RemoteEndPoint; }
        //}
        protected override bool CanFlush
        {
            get { return base.CanFlush && requests.Count > 0; }
        }

        public virtual void Enqueue(HttpListenerContext context)
        {
            Beat();
            int dataLength = (int)context.Request.ContentLength64;
            requests.AddFirst(context);
        }
        public override byte[] Read()
        {
            try
            {
                return base.Read();
            }
            catch (Exception ex)
            {
                dataLength = -1;
                requests.RemoveFirst();
                _LOG.Error(ex, "Read Error");
                throw ex;
            }
        }
        protected override int InternalRead(byte[] buffer, int offset, int size)
        {
            try
            {
                int read = 0;
                int __size = size;
                while (read < size)
                {
                    int tempRead = Peek.Request.InputStream.Read(buffer, offset, __size);
                    read += tempRead;
                    offset += tempRead;
                    __size -= tempRead;
                    if (tempRead == 0)
                        break;
                }
                dataLength -= read;
                //if (dataLength == 0)
                //    requests.RemoveFirst();
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
            if (requests.Count == 0)
            {
                // 没能推送成功时暂时缓存需要推送的数据
                SetPushCache(buffer, false);
                _LOG.Warning("No can response stream.");
                return;
            }
            int length = buffer.Length;
            if (pushCacheIndex > 0)
            {
                // 有之前没推送出去的数据时一并推送
                SetPushCache(buffer, true);
                buffer = pushCached;
                length = pushCacheIndex;
                pushCacheIndex = 0;
            }
            var response = Dequeue;
            response.Response.ContentLength64 = length;
            response.Response.KeepAlive = false;
            //response.Response.OutputStream.BeginWrite(buffer, 0, length, WriteOver, response);
            response.Response.OutputStream.BeginWrite(buffer, 0, length, null, null);
        }
        // 不关闭貌似写入没法断开连接
        //void WriteOver(IAsyncResult ar)
        //{
        //    HttpListenerContext context = (HttpListenerContext)ar.AsyncState;
        //    try
        //    {
        //        context.Response.OutputStream.EndWrite(ar);
        //    }
        //    catch (Exception ex)
        //    {
        //        _LOG.Error(ex, "回调写入错误");
        //    }
        //    finally
        //    {
        //        context.Response.Close();
        //    }
        //}

        private void SetPushCache(byte[] buffer, bool flush)
        {
            if (pushCacheIndex + buffer.Length > pushCached.Length)
            {
                int capcity = (int)(pushCached.Length * 1.5f + buffer.Length);
                // 非最终推送消息需要检查缓冲区大小，超过尺寸则清空之前的所有内容
                if (!flush && MAX_PUSH_CACHE_SIZE > 0 && capcity > MAX_PUSH_CACHE_SIZE)
                {
                    pushCacheIndex = 0;
                }
                else
                {
                    Array.Resize(ref pushCached, (int)(pushCached.Length * 1.5f + buffer.Length));
                }
            }
            Array.Copy(buffer, 0, pushCached, pushCacheIndex, buffer.Length);
            pushCacheIndex += buffer.Length;
        }
        public override void Close()
        {
            while (requests.Count > 0)
                Dequeue.Response.Close();
        }
    }

    public class FileUpload
    {
        public string Filename;
        public string ContentType;
        public byte[] Content;
    }
    // link
    //public class LinkHttpResponse : LinkBinary
    //{
    //    protected HttpListenerContext context;
    //    protected int length;
    //    private IPEndPoint endpoint;

    //    public HttpListenerContext Context
    //    {
    //        set
    //        {
    //            if (value == null)
    //                throw new ArgumentNullException("context");

    //            if (CanRead)
    //                throw new InvalidOperationException("");

    //            context = value;
    //            length = (int)context.Request.ContentLength64;
    //            //endpoint = new IPEndPoint(RealEndPoint.Address, 80);
    //            endpoint = context.Request.RemoteEndPoint;
    //        }
    //    }
    //    protected System.IO.Stream Request
    //    {
    //        get { return context.Request.InputStream; }
    //    }
    //    protected System.IO.Stream Response
    //    {
    //        get { return context.Response.OutputStream; }
    //    }
    //    public override bool IsConnected
    //    {
    //        get { return context != null; }
    //    }
    //    public override bool CanRead
    //    {
    //        get { return length > 0; }
    //    }
    //    protected override int DataLength
    //    {
    //        get { return length; }
    //    }
    //    public override IPEndPoint EndPoint
    //    {
    //        get { return endpoint; }
    //    }
    //    //public IPEndPoint RealEndPoint
    //    //{
    //    //    get { return context.Request.RemoteEndPoint; }
    //    //}
    //    protected override bool CanFlush
    //    {
    //        get { return base.CanFlush && length == 0; }
    //    }

    //    public LinkHttpResponse(HttpListenerContext context)
    //    {
    //        this.Context = context;
    //    }

    //    protected override int InternalRead(byte[] buffer, int offset, int size)
    //    {
    //        int read = Request.Read(buffer, offset, size);
    //        length -= read;
    //        return read;
    //    }
    //    protected override void InternalFlush(byte[] buffer)
    //    {
    //        context.Response.ContentLength64 = buffer.Length;
    //        context.Response.KeepAlive = false;
    //        //Response.Write(buffer, 0, buffer.Length);
    //        Response.BeginWrite(buffer, 0, buffer.Length, EndWrite, buffer);
    //    }
    //    protected void EndWrite(IAsyncResult ar)
    //    {
    //        try
    //        {
    //            Response.EndWrite(ar);
    //        }
    //        catch (WebException ex)
    //        {
    //            _LOG.Error(ex, "Response error! state={0}", ex.Status);
    //            if (ex.Status == WebExceptionStatus.ConnectionClosed)
    //            {
    //                _LOG.Info("ConnectionClosed");
    //                Close();
    //                return;
    //            }
    //            byte[] buffer = (byte[])ar.AsyncState;
    //            Response.BeginWrite(buffer, 0, buffer.Length, EndWrite, buffer);
    //            return;
    //        }
    //        Close();
    //    }
    //    public override void Close()
    //    {
    //        if (context == null)
    //            return;
    //        base.Close();
    //        try
    //        {
    //            Request.Close();
    //            Response.Close();
    //        }
    //        catch
    //        {
    //        }
    //        context = null;
    //    }
    //}
    //internal abstract class LinkBinaryResponse : LinkBinary
    //{
    //    private int read;
    //    private int length;
    //    private IPEndPoint ep;

    //    public override bool IsConnected
    //    {
    //        get { return true; }
    //    }
    //    public override bool CanRead
    //    {
    //        get { return length > 0; }
    //    }
    //    protected override int DataLength
    //    {
    //        get { return length; }
    //    }
    //    public override IPEndPoint EndPoint
    //    {
    //        get { return ep; }
    //    }

    //    public LinkBinaryResponse(IPEndPoint remote)
    //    {
    //        this.ep = remote;
    //    }

    //    public void SetData(byte[] data)
    //    {
    //        Array.Copy(data, 0, buffer, length, data.Length);
    //        length += data.Length;
    //    }
    //    protected override int InternalRead(byte[] buffer, int offset, int size)
    //    {
    //        read += size;
    //        if (read >= length)
    //        {
    //            length = 0;
    //            read = 0;
    //        }
    //        return size;
    //    }
    //}

    // stub
    public class AgentHttp : Agent, IDisposable
    {
        protected static readonly byte[] ZERO = new byte[0];
        internal Dictionary<string, StubHttp> protocols = new Dictionary<string, StubHttp>();
        private Queue<HttpListenerContext> queue = new Queue<HttpListenerContext>();
        /// <summary>一次OnProtocol处理完一个请求时触发</summary>
        public Action OnReset;
        private HttpListenerContext context;
        public Action<HttpListenerContext> OnChangeContext;
        public HttpListenerContext Context
        {
            get { return context; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("context");
                context = value;
                Param = null;
                if (OnChangeContext != null)
                    OnChangeContext(value);
            }
        }

        protected bool IsPost { get { return Context.Request.HttpMethod == "POST"; } }
        protected System.Collections.Specialized.NameValueCollection Param
        {
            get;
            set;
        }

        public AgentHttp()
        {
        }
        public AgentHttp(params StubHttp[] stubs)
        {
            foreach (var stub in stubs)
                AddAgent(stub);
        }

        public static void ResponseCrossDomainAndCharset(HttpListenerContext context)
        {
            context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            context.Response.ContentType = "text/plain; charset=utf-8";
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

            StubHttp agent = null;
            try
            {
                int splitIndex = -1;
                string localPath = Context.Request.Url.LocalPath;
                bool flag = false;
                for (int i = localPath.Length - 1; i >= 0; i--)
                {
                    if (localPath[i] == '/')
                    {
                        if (flag)
                        {
                            splitIndex = i;
                            break;
                        }
                        else
                            flag = true;
                    }
                }
                if (splitIndex == -1)
                {
                    Context.Response.StatusCode = 404;
                    throw new ArgumentException("错误的请求地址");
                }
                string path = Context.Request.Url.LocalPath.Substring(splitIndex + 1);

                foreach (var protocol in protocols)
                {
                    if (path.StartsWith(protocol.Key))
                    {
                        agent = protocol.Value;
                        break;
                    }
                }

                if (agent == null)
                {
                    Context.Response.StatusCode = 404;
                    throw new ArgumentException(string.Format("No agent! URL: {0}", path));
                }

                string stub = path.Substring(agent.Protocol.Length + 1);
                int paramIndex = stub.IndexOf('?');
                if (paramIndex != -1)
                    stub = stub.Substring(0, paramIndex);

                agent[stub](Context);
            }
            catch (Exception ex)
            {
                _LOG.Error(ex, "协议处理错误！URL: {0}", Context.Request.Url.LocalPath);
                try
                {
                    int err = Context.Response.StatusCode;
                    HttpException ex2 = ex as HttpException;
                    if (ex2 != null)
                    {
                        err = (int)ex2.StatusCode;
                    }
                    else
                    {
                        if (err == 200)
                        {
                            err = 500;
                        }
                    }
                    if (agent != null)
                    {
                        agent.Response(new HttpError(err, ex.Message));
                    }
                    else
                    {
                        Context.Response.StatusCode = err;
                        Context.Response.StatusDescription = ex.Message;
                        Context.Response.Close();
                    }
                }
                catch (Exception exInner)
                {
                    _LOG.Error(exInner, "协议异常回调错误！URL: {0}", Context.Request.Url.LocalPath);
                }
            }
            finally
            {
                context = null;
                Param = null;
                if (OnReset != null)
                    OnReset();
            }
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
            {
                Param = _NETWORK.ParseQueryString(_NETWORK.UrlDecode(Context.Request.Url.Query, Encoding.UTF8));
                //Param = Context.Request.QueryString;
            }
            return Param[paramName];
        }
        protected virtual string ParseParamPost(string paramName)
        {
            if (Param == null)
            {
                byte[] data = _IO.ReadStream(Context.Request.InputStream, (int)Context.Request.ContentLength64);
                string ctype = context.Request.ContentType;
                if (ctype.Contains("multipart/form-data;"))
                {
                    Param = new System.Collections.Specialized.NameValueCollection();
                    string bondary = "--" + ctype.Substring(ctype.IndexOf("boundary=") + 9);
                    string str = SingleEncoding.Single.GetString(data);
                    StringStreamReader reader = new StringStreamReader(str);
                    while (!reader.IsEnd)
                    {
                        reader.EatAfterSign(bondary);
                        // 最后的--\n
                        if (reader.Pos + 4 >= reader.str.Length)
                            break;
                        reader.EatAfterSign("form-data; name=\"");
                        string name = reader.NextToSignAfter("\"");
                        if (reader.PeekChar == ';')
                        {
                            // 文件; filename="白.txt"
                            reader.ToPosition(reader.Pos + 2);
                            if (reader.PeekNext("=") != "filename")
                                throw new ArgumentException("filename");
                            Param.Add(name, reader.NextToSign(bondary));
                        }
                        else
                        {
                            // 普通表单数据
                            reader.EatLine();
                            while (true)
                            {
                                reader.EatLine();
                                if (reader.PeekChar == '\r')
                                {
                                    // 空行之后是数据
                                    reader.EatLine();
                                    break;
                                }
                            }
                            Param.Add(name, _NETWORK.UrlDecode(SingleEncoding.Single.GetBytes(reader.Next("\r\n")), Encoding.UTF8));
                        }
                    }
                }
                else
                {
                    Param = _NETWORK.ParseQueryString(_NETWORK.UrlDecode(data, Context.Request.ContentEncoding));
                }
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
        private bool hasResponsed;
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
                hasResponsed = false;
                Action<HttpListenerContext> result;
                if (!stubs.TryGetValue(name, out result))
                    throw new NotImplementedException("no stub: " + name);
                return result;
            }
        }
        public bool HasResponsed
        {
            get { return hasResponsed; }
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
        public static FileUpload GetFile(string data)
        {
            FileUpload ret = new FileUpload();
            StringStreamReader reader = new StringStreamReader(data);
            reader.EatAfterSign("filename=\"");
            ret.Filename = reader.NextToSignAfter("\"");
            reader.EatAfterSign("Content-Type: ");
            ret.ContentType = reader.Next("\r\n");
            reader.EatLine();
            while (true)
            {
                reader.EatLine();
                if (reader.PeekChar == '\r')
                {
                    // 空行之后是数据
                    reader.EatLine();
                    break;
                }
            }
            ret.Content = SingleEncoding.Single.GetBytes(reader.Tail);
            return ret;
        }
        public void Response(object obj)
        {
            Response(JsonWriter.Serialize(obj));
        }
        public void Response(string text)
        {
            if (Context.Response.ContentEncoding == null)
            {
                Context.Response.ContentEncoding = Context.Request.ContentEncoding;
                if (Context.Response.ContentEncoding == null)
                    Context.Response.ContentEncoding = Encoding.UTF8;
            }
            Response(Context.Response.ContentEncoding.GetBytes(text));
        }
        public void Response(byte[] buffer)
        {
            Response(buffer, 0, buffer.Length);
        }
        public void Response(byte[] buffer, int offset, int count)
        {
            if (hasResponsed) return;
            hasResponsed = true;
            Context.Response.OutputStream.BeginWrite(buffer, offset, count, EndResponse, Context);
        }
        public void EndResponse(IAsyncResult ar)
        {
            var context = (HttpListenerContext)ar.AsyncState;
            try
            {
                context.Response.OutputStream.EndWrite(ar);
                context.Response.Close();
            }
            catch (Exception ex)
            {
                _LOG.Error(ex, "Error EndResponse");
            }
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

    public class FileService
    {
        Dictionary<string, byte[]> Cache;
        HttpListener listener;
        string localPath;

        public Action<HttpListenerContext> OnAccept;

        public bool UseCache
        {
            get { return Cache != null; }
            set
            {
                if (UseCache != value)
                {
                    if (value)
                    {
                        Cache = new Dictionary<string, byte[]>();
                    }
                    else
                    {
                        Cache = null;
                    }
                }
            }
        }
        public HttpListener Listener
        {
            get { return listener; }
        }
        public bool IsStarted
        {
            get { return listener != null; }
        }
        public string LocalPath
        {
            get { return localPath; }
            set
            {
                if (string.IsNullOrEmpty(value) || value.EndsWith("/") || value.EndsWith("\\"))
                {
                    localPath = string.Empty;
                }
                else
                {
                    localPath = value + "/";
                }
            }
        }
        public ushort Port
        {
            get;
            private set;
        }

        public void Start(ushort port)
        {
            listener = new HttpListener();
            listener.Prefixes.Add(string.Format("http://*:{0}/", port));
            listener.Start();
            listener.BeginGetContext(Accept, listener);
            this.Port = port;
        }
        void Accept(IAsyncResult ar)
        {
            HttpListener handle = (HttpListener)ar.AsyncState;
            try
            {
                HttpListenerContext context = handle.EndGetContext(ar);
                handle.BeginGetContext(Accept, handle);
                context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
                context.Response.ContentType = "application/octet-stream;charset=ISO-8859-1";
                if (OnAccept != null)
                    OnAccept(context);
                string path = context.Request.Url.LocalPath.Substring(1);
                _LOG.Debug(path);
                string fullPath = localPath + path;
                if (!File.Exists(fullPath))
                {
                    context.Response.StatusCode = 404;
                }
                else
                {
                    if (path.Contains(".html"))
                        context.Response.ContentType = "text/html";
                    byte[] bytes;
                    if (UseCache)
                    {
                        bool cache;
                        lock (Cache)
                            cache = Cache.TryGetValue(path, out bytes);
                        if (!cache)
                        {
                            bytes = File.ReadAllBytes(path);
                            lock (Cache)
                                Cache[path] = bytes;
                        }
                    }
                    else
                    {
                        bytes = File.ReadAllBytes(fullPath);
                    }
                    context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                }
                context.Response.Close();
            }
            catch (Exception ex)
            {
                _LOG.Debug("async GetContext error! msg={0}", ex.Message);
            }
            //finally
            //{

            //}
        }
        public void Stop()
        {
            if (listener != null)
            {
                listener.Stop();
                listener = null;
            }
        }
    }

    public class ParallelRouter<T>
    {
        class Router
        {
            public Func<T, int> RouteToken;
            public ParallelQueue<T>[] Workers;
            public Router(Func<T, int> getToken, ParallelQueue<T>[] workers)
            {
                this.RouteToken = getToken;
                this.Workers = workers;
            }
        }
        private List<Router> routers = new List<Router>();

        /// <param name="getToken">返回值必须大于等于0时分配工作队列，相同的返回值会分配到相同的工作队列；返回值小于0时不工作</param>
        public bool AddRouter(Func<T, int> getToken, ParallelQueue<T>[] workers)
        {
            if (workers == null || workers.Length == 0)
                throw new ArgumentException("Worker's count must bigger than 0.");
            for (int i = 0; i < workers.Length; i++)
                if (workers[i] == null)
                    throw new ArgumentNullException("worker");
            for (int i = 0; i < routers.Count; i++)
                if (routers[i].RouteToken == getToken)
                    return false;
            routers.Add(new Router(getToken, workers));
            return true;
        }
        /// <summary>队列工作，用于例如注册接口，检测之前是否有A账号，之后又要插入A账号，若并行处理则可能会插入多个相同账号</summary>
        /// <param name="getToken">true:工作 / false:不工作</param>
        public bool AddRouter(Func<T, bool> getToken, ParallelQueue<T> worker)
        {
            if (worker == null)
                throw new ArgumentNullException("worker");
            routers.Add(new Router(t => getToken(t) ? 0 : -1, new ParallelQueue<T>[] { worker }));
            return true;
        }
        public bool RemoveRouter(Func<T, int> getToken)
        {
            for (int i = 0; i < routers.Count; i++)
                if (routers[i].RouteToken == getToken)
                {
                    routers.RemoveAt(i);
                    return true;
                }
            return false;
        }
        public void ClearRouter()
        {
            routers.Clear();
        }
        public void Route(T data)
        {
            if (routers == null) return;
            for (int i = 0; i < routers.Count; i++)
            {
                int token;
                if (routers[i].RouteToken == null) token = 0;
                else token = routers[i].RouteToken(data);
                if (token >= 0)
                {
                    var workers = routers[i].Workers;
                    var temp = workers.FirstOrDefault(ob => ob.Check(token));
                    // 相同Token的请求则放入同一个线程队列
                    if (temp != null)
                    {
                        temp.Request(data, token);
                    }
                    else
                    {
                        // 分配一个工作最少的线程
                        temp = workers.MaxMin(ob => ob.QueueCount, true);
                        temp.Request(data, token);
                    }
                    break;
                }
            }
        }
    }
    public abstract class ParallelQueue<T>
    {
        struct Job
        {
            public T Context;
            public int Token;
            public Job(T context, int token)
            {
                this.Context = context;
                this.Token = token;
            }
        }
        private Thread thread;
        private EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private bool running = true;
        private Queue<Job> jobs = new Queue<Job>();
        private Job request;
        /// <summary>key:某个角色 / value:工作数量</summary>
        private Dictionary<int, int> tokenJobs = new Dictionary<int, int>();
        public int QueueCount { get { return jobs.Count; } }
        public bool Idle { get; private set; }

        public ParallelQueue()
        {
            thread = new Thread(MainProcess);
            thread.IsBackground = true;
            thread.Start();
        }
        void MainProcess()
        {
            while (running)
            {
                Idle = true;
                OnIdle();
                handle.Reset();
                handle.WaitOne();
                Idle = false;
                OnWork();

                while (true)
                {
                    lock (jobs)
                    {
                        if (jobs.Count > 0)
                            request = jobs.Dequeue();
                        else
                            break;
                    }

                    try
                    {
                        Work(request.Context);
                    }
                    catch (Exception ex)
                    {
                        lock (_LOG._Logger)
                            _LOG.Error(ex, "并行[{0}]异常, ", typeof(T).Name);
                    }

                    lock (tokenJobs)
                    {
                        int count;
                        if (tokenJobs.TryGetValue(request.Token, out count))
                        {
                            count--;
                            if (count == 0)
                                tokenJobs.Remove(request.Token);
                            else
                                tokenJobs[request.Token] = count;
                        }
                    }
                }
            }
        }
        protected abstract void Work(T data);
        protected virtual void OnIdle() { }
        protected virtual void OnWork() { }
        protected T GetContext()
        {
            return request.Context;
        }
        public void Request(T context, int token)
        {
            lock (jobs)
                jobs.Enqueue(new Job(context, token));
            lock (tokenJobs)
            {
                int count;
                if (tokenJobs.TryGetValue(token, out count))
                    tokenJobs[token] = count + 1;
                else
                    tokenJobs.Add(token, 1);
            }
            handle.Set();
        }
        public bool Check(int token)
        {
            lock (tokenJobs)
            {
                return tokenJobs.ContainsKey(token);
            }
        }
        public void Stop()
        {
            running = false;
        }
    }
    public class ParallelJsonHttpService : ParallelQueue<HttpListenerContext>
    {
        public AgentHttp Agent { get; protected set; }
        public LinkHttpResponseShort Link { get; protected set; }
        private AgentHttp hotFixAgent;

        public ParallelJsonHttpService() : this(null) { }
        public ParallelJsonHttpService(StubHttp[] stubs)
        {
            Link = new LinkHttpResponseShort();
            Link.ValidateCRC = false;
            if (stubs != null)
                Agent = new AgentHttp(stubs);
        }

        protected override void Work(HttpListenerContext data)
        {
            if (Agent == null)
            {
                _LOG.Warning("No Agent");
                data.Response.StatusCode = 404;
                data.Response.StatusDescription = "No Agent";
                data.Response.Close();
            }
            else
            {
                Agent.Context = data;
                data.Response.AppendHeader("Access-Control-Allow-Origin", "*");
                data.Response.AppendHeader("Access-Control-Allow-Headers", "AccessToken");
                data.Response.ContentType = "text/plain; charset=utf-8";
                data.Response.ContentEncoding = Encoding.UTF8;
                if (data.Request.HttpMethod.ToLower() == "options")
                {
                    data.Response.Close();
                }
                else
                {
                    Agent.OnProtocol(null);
                }
            }
        }
        protected override void OnIdle()
        {
            if (hotFixAgent != null)
            {
                this.Agent = hotFixAgent;
                hotFixAgent = null;
            }
        }
        /// <summary>热更新Agent处理协议：StubHttp[] Method(Func&lt;HttpListenerContext> getData)</summary>
        public void HotFixAgent(MethodInfo method)
        {
            HotFix((StubHttp[])method.Invoke(null, new object[] { new Func<HttpListenerContext>(GetContext) }));
        }
        public void HotFixAgent(Func<Func<HttpListenerContext>, StubHttp[]> method)
        {
            HotFix((StubHttp[])method(GetContext));
        }
        void HotFix(StubHttp[] stubs)
        {
            hotFixAgent = new AgentHttp(stubs);
            if (Idle)
                OnIdle();
        }
    }
    public abstract class ParallelBinaryService<T> : ParallelQueue<T>
    {
        public Agent Agent { get; protected set; }
        public Link Link { get; protected set; }
        private Agent hotFixAgent;

        protected override void OnIdle()
        {
            if (hotFixAgent != null)
            {
                this.Agent = hotFixAgent;
                hotFixAgent = null;
            }
        }
        /// <summary>热更新Agent处理协议：Stub[] Method(Func&lt;T> getData)</summary>
        public void HotFixAgent(MethodInfo method)
        {
            HotFix((Stub[])method.Invoke(null, new object[] { new Func<T>(GetContext) }));
        }
        public void HotFixAgent(Func<Func<T>, Stub[]> method)
        {
            HotFix((Stub[])method(GetContext));
        }
        void HotFix(Stub[] stubs)
        {
            hotFixAgent = new AgentProtocolStub(Link, stubs);
            if (Idle)
                OnIdle();
        }
    }
    public class ParallelBinaryHttpService : ParallelBinaryService<HttpListenerContext>
    {
        public ParallelBinaryHttpService()
        {
            Link = new LinkHttpResponseShort();
        }
        protected override void Work(HttpListenerContext data)
        {
            if (Agent == null)
            {
                _LOG.Warning("No Agent");
                data.Response.StatusCode = 404;
                data.Response.Close();
            }
            else
            {
                // 函数路由
                ((LinkHttpResponseShort)Link).Enqueue(data);
                /* 每帧需要手动发出客户端的请求数据 */
                while (Link.CanRead)
                {
                    byte[] package = Link.Read();
                    if (package != null)
                        Agent.OnProtocol(package);
                    else
                        break;
                }
                Link.Flush();
            }
        }
    }

    public class Cluster
    {
    }
}

#endif
