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

    /// <summary>请求限制</summary>
	public class AcceptLimit
	{
        /// <summary>允许请求的IP，null代表不限制IP，匹配规则为{0}%</summary>
		public string LimitIP;
        /// <summary>允许请求的端口，0代表不限制端口</summary>
		public ushort LimitPort;

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

        /// <summary>是否允许终端访问，true代表可以访问</summary>
		public bool Permit(IPEndPoint endpoint)
		{
            // 限制IP
			if (!string.IsNullOrEmpty(LimitIP) &&
                !endpoint.Address.ToString().StartsWith(LimitIP))
				return false;

            // 限制端口
			if (LimitPort != 0 && LimitPort != endpoint.Port)
				return false;

			return true;
		}
	}
    /// <summary>请求封禁</summary>
	public class AcceptBlock
	{
        public AcceptLimit Block;
        /// <summary>开始封禁时间</summary>
		public DateTime BlockTime;
        /// <summary>封禁时长，0代表永久</summary>
		public TimeSpan BlockDuration;
        /// <summary>封禁原因</summary>
		public string Reason;

		public DateTime? ReleaseTime
		{
			get
			{
				if (BlockDuration.Ticks > 0)
				{
					return BlockTime + BlockDuration;
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
				if (BlockDuration.Ticks > 0)
				{
					return DateTime.Now >= BlockTime + BlockDuration;
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

        private const string BLOCK_FILE = "BlockIP.blist";
		private static TimeSpan SECOND = TimeSpan.FromSeconds(1);

		private TIMER secondTimer = new TIMER();
		private Dictionary<IPAddress, int> loginCountPerSecondByIP = new Dictionary<IPAddress, int>();
		private Dictionary<string, Agent> clients = new Dictionary<string, Agent>();
        private List<AcceptBlock> blockIP = new List<AcceptBlock>();
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
        /// <summary>限定每秒允许同一个IP的接入数，超过则禁止该IP登录</summary>
        public uint? PermitSameIPLinkPerSecord = 1;
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
        public event Action<Link, EAcceptClose> AcceptDisconnect;
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
        public List<AcceptBlock> BlockIP { get { return blockIP; } }
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
                LoadBlock();
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
                for (int i = blockIP.Count - 1; i >= 0; i--)
                {
                    if (blockIP[i].IsRelease)
                    {
                        blockIP.RemoveAt(i);
                        _LOG.Info("IP:{0}自动解封");
                    }
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
                                Block("锁定登录", TimeSpan.FromDays(1), link.Link.EndPoint.Address.ToString());
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
                AcceptDisconnect(link.Link, status);
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

                    IPEndPoint endPoint = client.EndPoint;
                    string key = endPoint.ToString();
                    IPAddress address = endPoint.Address;

                    // 封禁IP不能登录
                    AcceptBlock block;
                    foreach (var item in blockIP)
                    {
                        if (item.Block.Permit(endPoint))
                        {
                            _LOG.Warning("封禁的IP {0} 尝试访问!", address);
                            client.Close();
                            continue;
                        }
                    }

                    // 记录每秒登录次数，若过于频繁则不允许登录甚至封禁
                    {
                        int loginCount;
                        if (loginCountPerSecondByIP.TryGetValue(address, out loginCount))
                        {
                            loginCount++;
                            loginCountPerSecondByIP[address] = loginCount;
                            if (loginCount > PermitSameIPLinkPerSecord)
                            {
                                client.Close();
                                Block("相同IP每秒访问次数过多", TimeSpan.MinValue, address.ToString());
                                continue;
                            }
                        }
                        else
                        {
                            loginCountPerSecondByIP.Add(address, 1);
                        }
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
                    OnAccept(ref client, acceptCount);
                    if (client == null) continue;

                    _LOG.Debug("accept = {0}", client.EndPoint);
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
            OnResetSecond();
		}
        protected virtual void OnResetSecond() { }
        private void SaveBlock()
        {
            _IO.WriteText(BLOCK_FILE, JsonWriter.Serialize(blockIP));
        }
        private void LoadBlock()
        {
            if (File.Exists(_IO.BuildPath(BLOCK_FILE)))
            {
                try
                {
                    blockIP = JsonReader.Deserialize<List<AcceptBlock>>(_IO.ReadText(BLOCK_FILE));
                    _LOG.Info("加载了被锁定的IP");
                }
                catch
                {
                    _LOG.Warning("加载锁定IP异常");
                }
            }
        }
        /// <summary>封禁IP的请求</summary>
        /// <param name="reason">封禁原因</param>
        /// <param name="duration">封禁时长，0代表永久</param>
        /// <param name="address">可以是192.168</param>
		public void Block(string reason, TimeSpan duration, params string[] address)
		{
			foreach (var item in address)
			{
                AcceptBlock block = null;
                bool flag = true;
                foreach (var b in blockIP)
                {
                    if (b.Block.LimitIP.StartsWith(item))
                    {
                        _LOG.Warning("已经封禁的IP:{0}，跳过封禁IP:{1}", b.Block.LimitIP, item);
                        flag = false;
                        break;
                    }
                    else if (item.StartsWith(b.Block.LimitIP))
                    {
                        if (item != b.Block.LimitIP)
                            _LOG.Warning("已经封禁的IP:{0}替换成{1}", b.Block.LimitIP, item);
                        block = b;
                        break;
                    }
                }
                if (!flag) continue;
                if (block == null)
                {
                    block = new AcceptBlock();
                    block.Block = new AcceptLimit();
                    blockIP.Add(block);
                }
                block.Block.LimitIP = item;
                block.BlockTime = DateTime.Now;
                block.Reason = reason;
                block.BlockDuration = duration;
                _LOG.Warning("封禁IP: {0} 时长：{1} 原因：{2}", item, duration.Ticks <= 0 ? "永久" : block.ReleaseTime.Value.ToString("yyyy-MM-dd HH:mm:ss"), reason);
			}
            SaveBlock();
		}
		public void ReleaseBlock(params string[] address)
		{
			foreach (var item in address)
			{
                for (int i = blockIP.Count - 1; i >= 0; i--)
                {
                    if (blockIP[i].Block.LimitIP.StartsWith(item))
                    {
                        _LOG.Info("解禁IP: {0}", item);
                        blockIP.RemoveAt(i);
                    }
                }
			}
            SaveBlock();
		}
        public void ClearBlock()
        {
            _LOG.Info("Clear Block IP");
            blockIP.Clear();
            SaveBlock();
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
        class LinkHttpListenerContext : Link
        {
            public HttpListenerContext Context;

            public LinkHttpListenerContext(HttpListenerContext context)
            {
                this.Context = context;
            }

            public override IPEndPoint EndPoint
            {
                get
                {
                    string nginxRemoteIP = Context.Request.Headers["X-Real-IP"];
                    if (!string.IsNullOrEmpty(nginxRemoteIP)) return new IPEndPoint(IPAddress.Parse(nginxRemoteIP), Context.Request.RemoteEndPoint.Port);
                    return Context.Request.RemoteEndPoint;
                }
            }
            public override bool IsConnected
            {
                get { throw new NotImplementedException(); }
            }
            public override bool CanRead
            {
                get { throw new NotImplementedException(); }
            }
            public override void Write(byte[] buffer, int offset, int size)
            {
                throw new NotImplementedException();
            }
            protected internal override void WriteBytes(byte[] buffer, int offset, int size)
            {
                throw new NotImplementedException();
            }
            public override byte[] Read()
            {
                throw new NotImplementedException();
            }
            public override byte[] Flush()
            {
                throw new NotImplementedException();
            }
            public override void Close()
            {
                if (this.Context != null)
                {
                    this.Context.Response.StatusCode = 403;
                    this.Context.Response.Close();
                }
                this.Context = null;
            }
        }

        /// <summary>限定每秒允许同一个接口的接入数，超过则禁止该IP登录</summary>
        public uint? PermitSameIPHandlePerSecord = 3;
        private HttpListener handle;
        private Queue<HttpListenerContext> contexts = new Queue<HttpListenerContext>();
        private Dictionary<IPAddress, Dictionary<string, int>> sameIPHandlePerSecond = new Dictionary<IPAddress, Dictionary<string, int>>();

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
        protected sealed override Link Accept()
        {
            HttpListenerContext context;
            lock (contexts)
                context = contexts.Dequeue();
            return new LinkHttpListenerContext(context);
        }
        protected sealed override void OnAccept(ref Link link, int acceptCount)
        {
            LinkHttpListenerContext __link = (LinkHttpListenerContext)link;

            // 检查同一IP访问同一地址的情况，访问多了进行封禁
            {
                IPEndPoint ep = __link.Context.Request.RemoteEndPoint;
                string url = __link.Context.Request.Url.AbsolutePath;
                Dictionary<string, int> temp;
                if (sameIPHandlePerSecond.TryGetValue(ep.Address, out temp))
                {
                    int count;
                    if (temp.TryGetValue(url, out count))
                    {
                        temp[url] = ++count;
                        if (count > PermitSameIPHandlePerSecord)
                        {
                            Block("相同IP每秒访问相同接口次数过多", TimeSpan.MinValue, ep.Address.ToString());
                            __link.Close();
                            return;
                        }
                    }
                    else
                        temp.Add(url, 1);
                }
                else
                {
                    temp = new Dictionary<string, int>();
                    sameIPHandlePerSecond.Add(ep.Address, temp);
                    temp.Add(url, 1);
                }
            }

            link = InternalAccept(__link.Context);
            __link.Context = null;
        }
        protected abstract Link InternalAccept(HttpListenerContext context);
        protected override void OnResetSecond()
        {
            sameIPHandlePerSecond.Clear();
        }
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
#if dotnet45
    public class LinkWebSocket : LinkBinary
    {
        private Task<WebSocketReceiveResult> received;
        public WebSocketMessageType MessageType = WebSocketMessageType.Binary;
        public Action<HttpListenerWebSocketContext> OnConnected;

        public HttpListenerContext Context { get; internal set; }
        public Task<HttpListenerWebSocketContext> Connecting { get; private set; }
        public WebSocket Socket { get; internal set; }
        public override bool IsConnected
        {
            get
            {
                if (Socket == null)
                {
                    if (Connecting.IsCompleted)
                    {
                        Socket = Connecting.Result.WebSocket;
                        if (OnConnected != null)
                            OnConnected(Connecting.Result);
                    }
                    else
                        return false;
                }
                return Connecting.IsCompleted && Socket.State == WebSocketState.Open;
            }
        }
        public override bool CanRead
        {
            get { return received == null || received.IsCompleted; }
        }
        protected override int DataLength
        {
            get { return received == null ? MAX_BUFFER_SIZE : MAX_BUFFER_SIZE + received.Result.Count; }
        }
        public override IPEndPoint EndPoint
        {
            get { return Context.Request.RemoteEndPoint; }
        }

        internal LinkWebSocket(HttpListenerContext context)
        {
            this.Context = context;
            this.ValidateCRC = false;
            this.Connecting = context.AcceptWebSocketAsync(null);
        }

        public override void Write(byte[] buffer, int offset, int size)
        {
            writer.WriteBytes(buffer, offset, size);
            // 防止粘包，每次写入都释放掉所有内容
            Flush();
        }
        protected override int PeekSize(byte[] buffer, out int peek)
        {
            peek = 0;
            if (received == null)
            {
                received = Socket.ReceiveAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), CancellationToken.None);
                return 0;
            }
            if (!received.IsCompleted)
                return 0;
            // WebSocket不需要传输包长，为了兼容传统TCP，这里手动加上包长
            int count = received.Result.Count;
            Array.Copy(buffer, 0, buffer, MAX_BUFFER_SIZE, count);
            int size = count + MAX_BUFFER_SIZE;
            Binary.GetBytes(buffer, 0, size);
            return size;
        }
        protected override void InternalFlush(byte[] buffer)
        {
            Socket.SendAsync(new ArraySegment<byte>(buffer), MessageType, true, CancellationToken.None);
        }
        protected override int InternalRead(byte[] buffer, int offset, int size)
        {
            var result = received.Result;
            received = null;
            return result.Count + MAX_BUFFER_SIZE;
        }
        public override void Close()
        {
            Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "关闭连接", CancellationToken.None);
        }

        public static bool IsWebSocketHandshake(HttpListenerContext c)
        {
            string socket = c.Request.Headers["Sec-WebSocket-Key"];
            return !string.IsNullOrEmpty(socket);
        }
    }
    public abstract class ServiceWebSocket : ProxyHttpAsync
    {
        protected override Link InternalAccept(HttpListenerContext context)
        {
            if (!LinkWebSocket.IsWebSocketHandshake(context))
                throw new InvalidCastException("非WebSocket连接");
            return new LinkWebSocket(context) { MessageType = WebSocketMessageType.Text };
        }
        //protected override IEnumerator<LoginResult> Login(Link link)
        //{
        //    LoginResult result = new LoginResult();
        //    LinkWebSocket lws = (LinkWebSocket)link;

        //    var time = GameTime.Time.CurrentFrame;
        //    while (!lws.IsConnected)
        //    {
        //        // 连接超时
        //        if ((GameTime.Time.CurrentFrame - time).Seconds > 5)
        //        {
        //            result.Result = EAcceptPermit.Refuse;
        //            yield return result;
        //            yield break;
        //        }
        //        yield return null;
        //    }

        //    result.Agent = new AgentProtocolStubJson(link, new _IWSStub(ws));
        //    result.Result = EAcceptPermit.Permit;
        //    yield return result;
        //}
    }
#endif

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

    public sealed class FileUpload : IDisposable
    {
        private AgentHttp agent;
        public string Filename { get; internal set; }
        public string ContentType { get; internal set; }
        /// <summary>文件流</summary>
        public FileStream File
        {
            get;
            internal set;
        }

        internal FileUpload(AgentHttp agent)
        {
        }

        public byte[] ReadAsByte()
        {
            if (File.Length > int.MaxValue)
                throw new InvalidOperationException("文件过大，不能读取字节内容");
            File.Seek(0, SeekOrigin.Begin);
            return _IO.ReadStream(File);
        }
        public void SaveAs(string fullFilePath)
        {
            System.IO.File.Copy(File.Name, fullFilePath, true);
        }
        public void Dispose()
        {
            if (File != null)
            {
                File.Close();
                File = null;
            }
        }
    }
    public class AgentHttp : Agent, IDisposable
    {
        protected static readonly byte[] ZERO = new byte[0];
        internal Dictionary<string, StubHttp> protocols = new Dictionary<string, StubHttp>();
        private Queue<HttpListenerContext> queue = new Queue<HttpListenerContext>();
        /// <summary>一次OnProtocol处理完一个请求时触发</summary>
        public Action OnReset;
        private HttpListenerContext context;
        /// <summary>当前请求的表单参数</summary>
        private System.Collections.Specialized.NameValueCollection parameters;
        /// <summary>当前请求的上传文件</summary>
        private Dictionary<string, FileUpload> files = new Dictionary<string, FileUpload>();
        private int uploadFileBufferSize = 4 * 1024 * 1024;
        private byte[] uploadFileBuffer;
        public Action<HttpListenerContext> OnChangeContext;
        public HttpListenerContext Context
        {
            get { return context; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("context");
                context = value;
                parameters = null;
                ReleaseFiles();
                if (OnChangeContext != null)
                    OnChangeContext(value);
            }
        }
        public bool IsPost { get { return Context.Request.HttpMethod == "POST"; } }
        public int UploadFileBufferSize
        {
            get { return uploadFileBufferSize; }
            set
            {
                if (value < 256)
                    value = 256;
                if (uploadFileBufferSize != value)
                    uploadFileBuffer = null;
                uploadFileBufferSize = value;
            }
        }
        internal byte[] UploadFileBuffer
        {
            get
            {
                if (uploadFileBuffer == null)
                    uploadFileBuffer = new byte[uploadFileBufferSize];
                return uploadFileBuffer;
            }
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
                            splitIndex = i + 1;
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

                int index2 = localPath.IndexOf('/', splitIndex);
                string protocol = localPath.Substring(splitIndex, index2 - splitIndex);

                foreach (var item in protocols)
                {
                    //if (path.StartsWith(protocol.Key))
                    if (protocol == item.Key)
                    {
                        agent = item.Value;
                        break;
                    }
                }

                if (agent == null)
                {
                    Context.Response.StatusCode = 404;
                    throw new ArgumentException(string.Format("No agent! URL: {0}", protocol));
                }

                string stub = localPath.Substring(index2 + 1);
                int paramIndex = stub.IndexOf('?');
                if (paramIndex != -1)
                    stub = stub.Substring(0, paramIndex);

                agent[stub](Context);
            }
            catch (HttpException ex)
            {
                _LOG.Warning("协议错误信息！URL: {0} EX: {1}", Context.Request.Url.LocalPath, ex.Message);
                try
                {
                    int err = (int)ex.StatusCode;
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
                    _LOG.Error(exInner, "HttpException协议异常回调错误！URL: {0}", Context.Request.Url.LocalPath);
                }
            }
            catch (Exception ex)
            {
                _LOG.Error(ex, "协议处理错误！URL: {0}", Context.Request.Url.LocalPath);
                try
                {
                    int err = Context.Response.StatusCode;
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
                    _LOG.Error(exInner, "Exception协议异常回调错误！URL: {0}", Context.Request.Url.LocalPath);
                }
            }
            finally
            {
                context = null;
                parameters = null;
                if (OnReset != null)
                    OnReset();
            }
        }
        public string GetParam(string paramName)
        {
            if (IsPost)
                return GetParamPost(paramName);
            else
                return GetParamGet(paramName);
        }
        public FileUpload GetFile(string paramName)
        {
            ParsePostParam();
            FileUpload file;
            files.TryGetValue(paramName, out file);
            return file;
        }
        private string GetParamGet(string paramName)
        {
            if (parameters == null)
            {
                parameters = _NETWORK.ParseQueryString(_NETWORK.UrlDecode(Context.Request.Url.Query, Encoding.UTF8));
                //Param = Context.Request.QueryString;
            }
            return parameters[paramName];
        }
        /// <summary>解析PostBody中的所有参数，文件内容；参数在parameters中，文件在files中</summary>
        private void ParsePostParam()
        {
            if (parameters != null) return;
            string ctype = context.Request.ContentType;
            if (!string.IsNullOrEmpty(ctype) && ctype.Contains("multipart/form-data;"))
            {
                parameters = new System.Collections.Specialized.NameValueCollection();

                var stream = Context.Request.InputStream;
                MultipartReader reader = new MultipartReader(stream);
                reader.Buffer = UploadFileBuffer;

                byte[] bound = Encoding.ASCII.GetBytes(reader.ReadLine());
                while (!reader.IsEnd)
                {
                    string paramName = null;
                    FileUpload file = null;

                    // 读取参数名，文件则读取文件名和类型
                    while (true)
                    {
                        string line = reader.ReadLine();
                        if (string.IsNullOrEmpty(line))
                            break;
                        else if (line.StartsWith("Content-Disposition"))
                        {
                            // 参数名
                            int index = line.IndexOf("name=\"");
                            if (index == -1)
                                throw new ArgumentException("无效的Content-Disposition");
                            index += 6;
                            paramName = line.Substring(index, line.IndexOf('\"', index) - index);

                            // 文件名
                            index = line.IndexOf("filename=\"");
                            if (index != -1)
                            {
                                index += 10;
                                file = new FileUpload(this);
                                // multipart/form-data;的数据貌似都是默认UTF8编码
                                file.Filename = line.Substring(index, line.IndexOf('\"', index) - index);
                            }
                        }
                        else if (file != null && line.StartsWith("Content-Type"))
                            file.ContentType = line.Substring(14);
                    }

                    if (file == null)
                    {
                        StringBuilder builder = new StringBuilder();
                        // 普通参数
                        while (true)
                        {
                            bool result = reader.ReadSign(bound);
                            int position = reader.Position;
                            // 会多出一个\r\n
                            if (result)
                                position -= 2;
                            if (position > 0)
                                builder.Append(Encoding.UTF8.GetString(reader.Buffer, 0, position));
                            reader.Flush();
                            if (result)
                                break;
                        }

                        parameters.Add(paramName, builder.ToString());
                    }
                    else
                    {
                        // 文件
                        file.File = new FileStream(Guid.NewGuid().ToString("n"), FileMode.Create, FileAccess.ReadWrite, FileShare.Read, uploadFileBufferSize, FileOptions.DeleteOnClose);
                        while (true)
                        {
                            bool result = reader.ReadSign(bound);
                            int position = reader.Position;
                            // 会多出一个\r\n
                            if (result)
                                position -= 2;
                            if (position > 0)
                                file.File.Write(reader.Buffer, 0, position);
                            reader.Flush();
                            if (result)
                                break;
                        }
                        file.File.Flush();

                        files.Add(paramName, file);
                    }

                    reader.ReadLine();
                } // end of 读取一个参数
            }
            else
            {
                byte[] data = _IO.ReadStream(Context.Request.InputStream, (int)Context.Request.ContentLength64);
                // 对于传过来的Base64的字符串图片做UrlDecode会导致字符串内容还原错误
                //parameters = _NETWORK.ParseQueryString(_NETWORK.UrlDecode(data, Context.Request.ContentEncoding));
                parameters = _NETWORK.ParseQueryString(Context.Request.ContentEncoding.GetString(data), true, Context.Request.ContentEncoding);
            }
        }
        private string GetParamPost(string paramName)
        {
            ParsePostParam();
            return parameters[paramName];
        }
        private void ReleaseFiles()
        {
            if (files.Count > 0)
            {
                foreach (var item in files)
                {
                    if (item.Value.File != null)
                    {
                        item.Value.File.Close();
                    }
                }
                files.Clear();
            }
        }
        public void Dispose()
        {
            ReleaseFiles();
            while (queue.Count > 0)
                queue.Dequeue().Response.Close();
        }

        class MultipartReader
        {
            private Stream stream;
            public byte[] Buffer;
            public int Offset;
            private byte[] signBuffer;
            private int signIndex;
            private bool end;
            /// <summary>查找Sign的索引</summary>
            public int SignedIndex
            {
                get;
                private set;
            }
            public int Position { get { return SignedIndex == -1 ? Offset : SignedIndex; } }
            public bool IsEnd { get { return end && Offset == 0; } }

            public MultipartReader(Stream stream)
            {
                this.stream = stream;
            }

            private void ReadBuffer()
            {
                if (end) return;
                int read = stream.Read(Buffer, Offset, Buffer.Length - Offset);
                Offset += read;
                end = read == 0;
            }
            public string ReadLine()
            {
                ReadBuffer();
                int offset = Offset;
                for (int i = 0; i < offset; i++)
                {
                    if (Buffer[i] == 10)
                    {
                        int line = i;
                        if (i > 0 && Buffer[i - 1] == 13)
                            line--;

                        string result = Encoding.UTF8.GetString(Buffer, 0, line);

                        i++;
                        Offset -= i;
                        Array.Copy(Buffer, i, Buffer, 0, Offset);

                        return result;
                    }
                }
                throw new InvalidOperationException("不能读取到完整的行，请扩大Buffer容量");
            }
            public bool ReadSign(byte[] sign)
            {
                if (sign.Length > Buffer.Length)
                    throw new InvalidOperationException("不能读取到完整的标记，请扩大Buffer容量");

                ReadBuffer();

                if (!Utility.Equals(signBuffer, sign))
                {
                    signBuffer = sign;
                    signIndex = 0;
                    SignedIndex = -1;
                }

                for (int i = 0; i < Offset; i++)
                {
                    if (Buffer[i] == signBuffer[signIndex])
                    {
                        signIndex++;
                        if (signIndex == signBuffer.Length)
                        {
                            // 匹配
                            SignedIndex = i + 1 - signIndex;
                            return true;
                        }
                    }
                    else
                    {
                        signIndex = 0;
                    }
                }

                if (signIndex != 0)
                {
                    SignedIndex = Offset - signIndex;
                    signIndex = 0;
                }
                return false;
            }
            public bool ReadSign(string sign)
            {
                return ReadSign(Encoding.UTF8.GetBytes(sign));
            }
            /// <summary>将Buffer积累读取的数据读截取掉</summary>
            public void Flush()
            {
                if (SignedIndex != -1)
                {
                    if (signIndex == signBuffer.Length)
                    {
                        SignedIndex += signBuffer.Length;
                    }
                    Array.Copy(Buffer, SignedIndex, Buffer, 0, Offset - SignedIndex);
                    Offset -= SignedIndex;
                }
                else
                    Offset = 0;
                SignedIndex = -1;
                signIndex = 0;
            }
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
        public FileUpload GetFile(string paramName)
        {
            return ProtocolAgent.GetFile(paramName);
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
        private void EndResponse(IAsyncResult ar)
        {
            var context = (HttpListenerContext)ar.AsyncState;
            using (context.Response)
            {
                try
                {
                    context.Response.OutputStream.EndWrite(ar);
                }
                catch
                {
                    _LOG.Warning("请求已关闭");
                }
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
        int bigFileSize;

        /// <summary>收到请求后，返回下载的完整路径，返回null则使用LocalPath进行下载，返回""则不下载</summary>
        public Func<HttpListenerContext, string> OnDownload;

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
                if (string.IsNullOrEmpty(value))
                {
                    localPath = string.Empty;
                }
                else
                {
                    if (value.EndsWith("/") || value.EndsWith("\\"))
                        localPath = value;
                    else
                        localPath = value + "/";
                }
            }
        }
        public ushort Port
        {
            get;
            private set;
        }
        /// <summary>
        /// <para>文件尺寸超过这个值时属于大文件</para>
        /// <para>大文件将不使用缓存</para>
        /// <para>大文件的下载将使用文件流的形式进行文件传输</para>
        /// </summary>
        public int BigFileSize
        {
            get { return bigFileSize; }
            set
            {
                if (value < 0)
                    value = 0;
                bigFileSize = value;
            }
        }

        public bool IsBigFile(long size)
        {
            return size >= bigFileSize;
        }
        public void Start(ushort port, string path)
        {
            listener = new HttpListener();
            listener.Prefixes.Add(string.Format("http://*:{0}/{1}", port, path));
            listener.Start();
            listener.BeginGetContext(Accept, listener);
            this.Port = port;
        }
        void Accept(IAsyncResult ar)
        {
            HttpListener handle = (HttpListener)ar.AsyncState;
            HttpListenerResponse response = null;
            try
            {
                byte[] bytes = null;

                HttpListenerContext context = handle.EndGetContext(ar);
                handle.BeginGetContext(Accept, handle);
                response = context.Response;
                response.AppendHeader("Access-Control-Allow-Origin", "*");
                response.ContentType = "application/octet-stream;charset=ISO-8859-1";
                string download = null;
                string path = context.Request.Url.LocalPath.Substring(1);
                if (OnDownload != null)
                    download = OnDownload(context);
                if (download == string.Empty)
                    return;
                if (download == null)
                    download = localPath + path;
                _LOG.Debug("文件下载：{0}", download);
                if (!File.Exists(download))
                {
                    response.StatusCode = 404;
                }
                else
                {
                    if (download.Contains(".html"))
                        response.ContentType = "text/html";

                    if (UseCache)
                    {
                        bool cache;
                        lock (Cache)
                            cache = Cache.TryGetValue(path, out bytes);
                    }

                    if (bytes != null)
                    {
                        response.OutputStream.Write(bytes, 0, bytes.Length);
                    }
                    else
                    {
                        using (FileStream stream = new FileStream(download, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 2048, FileOptions.Asynchronous))
                        {
                            // 不是大文件才可以使用缓存
                            if (!IsBigFile(stream.Length) && UseCache)
                            {
                                bytes = _IO.ReadStream(stream);
                                lock (Cache)
                                    Cache[path] = bytes;
                            }

                            if (bytes != null)
                                response.OutputStream.Write(bytes, 0, bytes.Length);
                            else
                            {
                                bytes = new byte[1024 * 1024];
                                while (true)
                                {
                                    int read = stream.Read(bytes, 0, bytes.Length);
                                    if (read == 0) break;
                                    response.OutputStream.Write(bytes, 0, read);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _LOG.Error(ex, "文件服务器错误");
                if (response != null && response.StatusCode == 200)
                    response.StatusCode = 500;
            }
            finally
            {
                if (response != null)
                {
                    response.OutputStream.Close();
                    response.Close();
                }
                response = null;
            }
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
            if (getToken == null)
                throw new ArgumentNullException("getToken");
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
            if (getToken == null)
                throw new ArgumentNullException("getToken");
            if (worker == null)
                throw new ArgumentNullException("worker");
            routers.Add(new Router(t => getToken(t) ? 0 : -1, new ParallelQueue<T>[] { worker }));
            return true;
        }
        /// <summary>不用队列处理协议，但参与路由；适合快速响应的同步处理</summary>
        public bool AddRouter(Action<T> getToken)
        {
            if (getToken == null)
                throw new ArgumentNullException("getToken");
            routers.Add(new Router((t) => { getToken(t); return -1; }, null));
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
                    if (jobs.Count > 0)
                        request = jobs.Peek();
                    else
                        break;

                    try
                    {
                        Work(request.Context);
                    }
                    catch (Exception ex)
                    {
                        lock (_LOG._Logger)
                            _LOG.Error(ex, "并行[{0}]异常, ", typeof(T).Name);
                    }

                    // 工作处理完后再移除，防止工作时间较长时也被视为空闲
                    lock (jobs)
                    {
                        jobs.Dequeue();
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
        public ParallelJsonHttpService(params StubHttp[] stubs)
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
                data.Response.ContentType = "text/plain; charset=utf-8";
                data.Response.ContentEncoding = Encoding.UTF8;
                if (data.Request.HttpMethod.ToLower() == "options")
                {
                    var headers = data.Request.Headers["Access-Control-Request-Headers"];
                    if (!string.IsNullOrEmpty(headers))
                        data.Response.AddHeader("Access-Control-Allow-Headers", headers);
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
