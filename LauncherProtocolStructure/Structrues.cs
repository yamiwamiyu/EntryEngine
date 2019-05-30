using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LauncherProtocolStructure
{
    public class ServiceType
    {
        public string Name;
        public string SVNPath;
        public string SVNUser;
        public string SVNPassword;
        public string Exe;
        public string LaunchCommand;

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
    public class Server
    {
        public ushort ID;
        public string EndPoint;
        public string NickName;
        public Service[] Services = new Service[0];

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
    public class Service
    {
        /// <summary>一个平台不允许出现重名</summary>
        public string Name;
        /// <summary>服务类型</summary>
        public string Type;
        /// <summary>服务实例版本号，负数则为热更</summary>
        public int Revision;
        public string LaunchCommand;
        public string Exe;

        public int RevisionOnServer;
        public string LastStatusTime;
        public EServiceStatus Status;

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
}
