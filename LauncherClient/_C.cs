using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine;
using EntryEngine.Serialize;

[AReflexible]public static partial class _C
{
    public static string IP;
    public static ushort Port;
    /// <summary>启动器客户端连接服务端的公钥</summary>
    public static string LauncherPublicKey;
    
    public static Action OnSave;
    public static Action OnLoad;
    public static void Save(string file)
    {
        if (OnSave != null) OnSave();
        _IO.WriteText(file, new XmlWriter().WriteStatic(typeof(_C)));
    }
    public static void Load(string content)
    {
        new XmlReader(content).ReadStatic(typeof(_C));
        if (OnLoad != null) OnLoad();
    }
}
