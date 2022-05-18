using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LauncherProtocolStructure
{
    /// <summary>服务类型，一个服务类型需要一个SVN目录</summary>
    public class ServiceType
    {
        /// <summary>服务名字</summary>
        public string Name;
        /// <summary>SVN的URL</summary>
        public string SVNPath;
        /// <summary>SVN的用户名</summary>
        public string SVNUser;
        /// <summary>SVN的密码</summary>
        public string SVNPassword;
        /// <summary>程序的可执行文件，不需要运行的例如网页前端可以为空</summary>
        public string Exe;
        /// <summary>程序的初始启动命令，可以写入可执行文件支持的所有命令</summary>
        public string LaunchCommand;
        /// <summary>线上最新的版本号</summary>
        public int Revision;

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            ServiceType other = obj as ServiceType;
            if (other == null)
                return false;

            return other.Name == this.Name;
        }
        public override int GetHashCode()
        {
            if (Name == null)
                return base.GetHashCode();
            else
                return Name.GetHashCode();
        }
    }
    /// <summary>服务器，一台服务器可以有多个服务</summary>
    public class Server
    {
        public ushort ID;
        public string EndPoint;
        public string NickName;
        public List<Service> Services = new List<Service>();

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(NickName))
                    return EndPoint;
                else
                    return NickName;
            }
        }
    }
    /// <summary>服务，可以是服务端，也可以是客户端，服务是一个服务类型的实例</summary>
    public class Service
    {
        /// <summary>一个平台不允许出现重名</summary>
        public string Name;
        /// <summary>服务类型</summary>
        public string Type;
        /// <summary>服务实例版本号，负数则为热更</summary>
        public int Revision;
        /// <summary>程序的初始启动命令</summary>
        public string LaunchCommand;
        /// <summary>程序的可执行文件</summary>
        public string Exe;

        /// <summary>服务类型的SVN最新版本</summary>
        public int RevisionOnServer;
        /// <summary>最后状态变更时间</summary>
        public string LastStatusTime;
        /// <summary>服务运行状态</summary>
        public EServiceStatus Status;

        /// <summary>打了[CMD]标记的接口命令(方法名 参数名)，由LoggerToShell.StatusRunning自动获取调用该方法的程序集</summary>
        public List<string> Commands { get; set; }

        public string Directory
        {
            get { return string.Format("__{0}/{1}/", Type, Name); }
        }
        public bool NeedUpdate
        {
            get
            {
                return Revision != RevisionOnServer && (Status == EServiceStatus.Stop || Math.Abs(Revision) != RevisionOnServer);
            }
        }
    }
    public class Command
    {
        public string CommandName;
        public string[] Arguments;
    }
    public enum EServiceStatus : byte
    {
        Stop,
        Starting,
        Running,
    }
    public class ServiceTypeRevision
    {
        public string Type;
        public int Revision;
    }
    public struct ServerStatusData
    {
        public ushort Connections;
        /// <summary>mbyte</summary>
        public ushort Memory;
        /// <summary>Cpu%</summary>
        public byte Cpu;
        /// <summary>byte</summary>
        public uint Network;
        /// <summary>byte</summary>
        public uint Disk;
    }
    public static class _EX
    {
        public static void Check(this string message, bool isThrow)
        {
            if (isThrow)
                throw new Exception(message);
        }
    }
}
