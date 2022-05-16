using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine;
using EntryEngine.Serialize;
using LauncherProtocolStructure;
using EntryEngine.Network;
using EntryEngine.Cmdline;
using LauncherProtocol;
using System.Management;
using System.Threading;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace LauncherClient
{
    static class _SAVE
    {
        private const string SAVE_DATA = "data.sav";

        public static List<Service> Services = new List<Service>();
        //public static List<ServiceTypeRevision> ServiceTypes = new List<ServiceTypeRevision>();

        public static void Save()
        {
            ByteRefWriter writer = new ByteRefWriter(SerializeSetting.DefaultSerializeStatic);
            writer.WriteObject(null, typeof(_SAVE));
            _IO.WriteByte(SAVE_DATA, writer.GetBuffer());
        }
        public static void Load()
        {
            try
            {
                ByteRefReader reader = new ByteRefReader(_IO.ReadByte(SAVE_DATA), SerializeSetting.DefaultSerializeStatic);
                reader.ReadObject(typeof(_SAVE));

                foreach (var item in Services)
                {
                    item.Status = EServiceStatus.Stop;
                    item.LastStatusTime = null;
                    //item.RevisionOnServer = ServiceTypes.FirstOrDefault(s => s.Type == item.Type).Revision;
                    _LOG.Info("Load Service [{0}] Susscess!", item.Name);
                }
            }
            catch
            {
            }
        }
    }
    interface ICmdline
    {
        void CallCommand(string command);
    }

    partial class ServiceLauncher : EntryService, ICmdline
    {
        class LoggerToManager : LoggerConsole
        {
            ServiceLauncher service;
            public LoggerToManager(ServiceLauncher service)
            {
                //Colors.Remove(0);
                this.service = service;
            }
            public override void Log(ref Record record)
            {
                if (service.Proxy != null && record.Level > 0)
                    service.Proxy.LogServer(null, record);
                base.Log(ref record);
            }
        }

        int id;
        //StatisticCounter cNetwork;
        //StatisticCounter cCpu;
        //StatisticCounter cDisk;
        //StatisticCounter cMemory;

        public LinkTcp Link
        {
            get;
            private set;
        }
        public ILauncherServiceProxy Proxy
        {
            get;
            private set;
        }
        public Agent Agent
        {
            get;
            private set;
        }

        public ServiceLauncher()
        {
            SetCoroutine(Initialize());
        }

        IEnumerable<ICoroutine> Initialize()
        {
            _LOG._Logger = new LoggerFile(new LoggerToManager(this));
            //_LOG._Logger = new LoggerToManager(this);

            _LOG.Info("正在读取配置");
            _C.Load(_IO.ReadText("_C.xml"));
            _SAVE.Load();

            _LOG.Info("正在初始化服务器数据统计");
            //cNetwork = new StatisticCounter(ECounter.Network);
            //cCpu = new StatisticCounter(ECounter.CPU);
            //cDisk = new StatisticCounter(ECounter.Disk);
            //cMemory = new StatisticCounter(ECounter.Memory);

            this.SetCoroutine(ConnectManager());

            yield break;
        }
        IEnumerable<ICoroutine> ConnectManager()
        {
            Link = new LinkTcp();
            Link.Heartbeat = TimeSpan.FromMinutes(30);

            while (true)
            {
                if (!Link.IsConnected)
                {
                    Agent = null;
                    Proxy = null;

                    var async = Link.Connect(_C.IP, _C.Port);
                    yield return async;

                    if (async.IsFaulted)
                    {
                        _LOG.Error(async.FaultedReason, "连接管理服务器失败");
                    }
                    else
                    {
                        string endpoint = Link.EndPoint.ToString();

                        _LOG.Info("成功连接管理服务器{0}", endpoint);
                        ByteWriter writer = new ByteWriter();
                        string ip = _NETWORK.HostIP;
                        if (_C.IP == "127.0.0.1")
                            ip = "127.0.0.1";
                        writer.Write(ip);
                        writer.Write(_NETWORK.ValidMD5toBase64(ip + _C.LauncherPublicKey));
                        writer.Write(_C.Name);
                        Link.Write(writer.Buffer, 0, writer.Position);
                        Link.Flush();

                        TIME wait = new TIME();
                        while (Link.IsConnected)
                        {
                            byte[] callback = Link.Read();
                            if (callback == null)
                            {
                                wait.Interval += 100;
                                // 100, 200....1000 总计等待6000ms
                                if (wait.Interval > 1000)
                                {
                                    _LOG.Error("未能在时间内收到服务器的相应，关闭本次连接{0}", endpoint);
                                    Link.Close();
                                    break;
                                }
                                yield return wait;
                                continue;
                            }

                            ByteReader reader = new ByteReader(callback);
                            ushort id;
                            reader.Read(out id);

                            _LOG.Debug("成功连接服务器 ID={0}", id);
                            this.id = id;

                            Proxy = new ILauncherServiceProxy();
                            //Agent = new AgentHeartbeat(
                            //    new AgentProtocolStub(Link,
                            //        Proxy, new IManagerCallStub(this)));
                            Agent = new AgentProtocolStub(Link, Proxy, new IManagerCallStub(this));

                            Proxy.PushServices(_SAVE.Services);
                            //foreach (var item in _SAVE.ServiceTypes)
                            //    Proxy.RevisionUpdate(item);
                            break;
                        }
                    }
                }
                // 等待下一次检查与服务器的连接
                yield return new TIME(5000);
            }
        }

        protected override void InternalUpdate()
        {
            base.InternalUpdate();

            // network
            if (Link != null && Link.IsConnected && Agent != null)
            {
                Link.Flush();
                foreach (var item in Agent.Receive())
                {
                    try
                    {
                        Agent.OnProtocol(item);
                    }
                    catch (Exception ex)
                    {
                        _LOG.Error(ex, "OnProtocol");
                        // 重连服务器
                        Link.Close();
                        return;
                    }
                }

                // launcher status update
                foreach (var service in _SAVE.Services)
                {
                    var launcher = Launcher.Find(service.Name);
                    if (launcher != null && launcher.Running)
                    {
                        if (service.Status == EServiceStatus.Stop)
                        {
                            SetStatus(service, EServiceStatus.Starting);
                        }
                    }
                    else
                    {
                        if (service.Status != EServiceStatus.Stop)
                        {
                            SetStatus(service, EServiceStatus.Stop);
                            if (launcher != null)
                                launcher.Dispose();
                        }
                    }
                }

                // statistic
                //if (GameTime.TickSecond && GameTime.Second % 5 == 0)
                //{
                //    PerformanceCounter network = cNetwork["Bytes Total/sec"];
                //    PerformanceCounter cpu = cCpu["% Processor Time"];
                //    PerformanceCounter disk = cDisk["Disk Bytes/sec"];
                //    PerformanceCounter memory = cMemory["Available MBytes"];
                //    IPGlobalProperties ips = IPGlobalProperties.GetIPGlobalProperties();

                //    ServerStatusData data;
                //    data.Network = (uint)network.NextValue();
                //    data.Cpu = (byte)cpu.NextValue();
                //    data.Disk = (uint)disk.NextValue();
                //    data.Memory = (ushort)memory.NextValue();
                //    data.Connections = (ushort)ips.GetActiveTcpConnections().Where(t => t.State == TcpState.Established).Count();
                //    Proxy.ServerStatusStatistic(data);
                //}
            }
        }
        public override void Dispose()
        {
            if (Link != null)
                Link.Close();
            Link = null;
            Proxy = null;
            Agent = null;
        }

        void ICmdline.CallCommand(string command)
        {
            foreach (var launcher in Launcher.Launchers)
                launcher.ExecuteCommand(command);
        }
    }
}
