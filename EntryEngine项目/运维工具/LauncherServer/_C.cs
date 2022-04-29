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
        public ushort PortManager;
        public ushort PortManagerBS;
        public ushort PortLauncher;
        /// <summary>连接服务端的公钥</summary>
        public string PublicKey;
    }
    public static __C ___C;
    public static string Name { get { return ___C.Name; } }
    public static ushort PortManager { get { return ___C.PortManager; } }
    public static ushort PortManagerBS { get { return ___C.PortManagerBS; } }
    public static ushort PortLauncher { get { return ___C.PortLauncher; } }
    /// <summary>连接服务端的公钥</summary>
    public static string PublicKey { get { return ___C.PublicKey; } }
    
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
