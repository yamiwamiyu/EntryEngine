using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine;
using EntryEngine.Serialize;

[AReflexible]public static partial class _C
{
    [AReflexible]public partial class __C
    {
        public string Name;
        /// <summary>对管理器开放的端口</summary>
        public ushort PortManager;
        /// <summary>对管理器网页版开放的端口</summary>
        public ushort PortManagerBS;
        /// <summary>对启动器开放的端口</summary>
        public ushort PortLauncher;
        /// <summary>启动器连接服务端的公钥，要与启动器配置的一致</summary>
        public string PublicKey;
        /// <summary>默认的管理员账号</summary>
        public string DefaultAccount;
        /// <summary>默认的管理员密码</summary>
        public string DefaultPassword;
    }
    public static __C ___C;
    public static string Name { get { return ___C.Name; } }
    /// <summary>对管理器开放的端口</summary>
    public static ushort PortManager { get { return ___C.PortManager; } }
    /// <summary>对管理器网页版开放的端口</summary>
    public static ushort PortManagerBS { get { return ___C.PortManagerBS; } }
    /// <summary>对启动器开放的端口</summary>
    public static ushort PortLauncher { get { return ___C.PortLauncher; } }
    /// <summary>启动器连接服务端的公钥，要与启动器配置的一致</summary>
    public static string PublicKey { get { return ___C.PublicKey; } }
    /// <summary>默认的管理员账号</summary>
    public static string DefaultAccount { get { return ___C.DefaultAccount; } }
    /// <summary>默认的管理员密码</summary>
    public static string DefaultPassword { get { return ___C.DefaultPassword; } }
    
    public static Action OnSave;
    public static Action OnLoad;
    public static void Save(string file)
    {
        if (OnSave != null) OnSave();
        _IO.WriteText(file, XmlWriter.Serialize(___C));
    }
    public static void Load(string content)
    {
        ___C = XmlReader.Deserialize<__C>(content);
        if (OnLoad != null) OnLoad();
    }
}
