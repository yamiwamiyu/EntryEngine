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
        public string IP;
        public ushort Port;
        /// <summary>启动器客户端连接服务端的公钥</summary>
        public string LauncherPublicKey;
        /// <summary>服务名</summary>
        public string Name;
    }
    public static __C ___C;
    public static string IP { get { return ___C.IP; } }
    public static ushort Port { get { return ___C.Port; } }
    /// <summary>启动器客户端连接服务端的公钥</summary>
    public static string LauncherPublicKey { get { return ___C.LauncherPublicKey; } }
    /// <summary>服务名</summary>
    public static string Name { get { return ___C.Name; } }
    
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
