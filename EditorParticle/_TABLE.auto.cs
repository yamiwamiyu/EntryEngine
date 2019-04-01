using System;
using System.Collections.Generic;
using System.Linq;
using EntryEngine;
using EntryEngine.Serialize;

[AReflexible]public partial class PF
{
    public static bool __Load = true;
    /// <summary>命名空间.类名, 程序集名</summary>
    public string TypeName;
    /// <summary>编辑器里显示的名字</summary>
    public string Name
    {
        get { return _LANGUAGE.GetString(__Name); }
        set { __Name = value; }
    }
    private string __Name;
    /// <summary>流类型{Update结果:true/false/-1，一次性:true/false，子对象:0/n}（0：运动器true，false，0；1：发射器true，false，0；2：跳转器true，true，n；3：检测器-1，false，n）</summary>
    public byte Type;
    /// <summary>说明</summary>
    public string Explain
    {
        get { return _LANGUAGE.GetString(__Explain); }
        set { __Explain = value; }
    }
    private string __Explain;
}
[AReflexible]public partial class TEXT
{
    public static bool __Load = true;
    /// <summary>键</summary>
    public enum ETEXTKey
    {
        New,
        Open,
        Save,
        Undo,
        Redo,
        Move,
        Delete,
        Directory,
        Preview,
        Help,
        Duration,
        Timeline,
        Play,
        Back,
        Forward,
        Offset,
        Scale,
        PCount,
        SaveAs,
    }
    public ETEXTKey Key;
    /// <summary>值</summary>
    public string Value
    {
        get { return _LANGUAGE.GetString(__Value); }
        set { __Value = value; }
    }
    private string __Value;
}
public static partial class _TABLE
{
    public static string _path { get; private set; }
    public static PF[] _PF;
    public static TEXT[] _TEXT;
    public static Dictionary<TEXT.ETEXTKey, TEXT> _TEXTByKey;
    
    public static event Action<PF[]> OnLoadPF;
    public static event Action<TEXT[]> OnLoadTEXT;
    
    public static void SetPath(string path)
    {
        if (_path != path)
        {
            _path = _IO.DirectoryWithEnding(path);
            _LOG.Info("Set table path: {0}", _path);
        }
    }
    public static void Load(string path)
    {
        SetPath(path);
        _LOG.Info("Load all tables", path);
        LoadPF();
        LoadTEXT();
    }
    public static IEnumerable<AsyncReadFile> LoadAsync(string path)
    {
        SetPath(path);
        _LOG.Info("LoadAsync all tables", path);
        foreach (var item in LoadPFAsync()) yield return item;
        foreach (var item in LoadTEXTAsync()) yield return item;
        yield break;
    }
    public static void Reload()
    {
        _LOG.Info("Reload all tables");
        Load(_path);
    }
    public static IEnumerable<AsyncReadFile> ReloadAsync()
    {
        _LOG.Info("ReloadAsync all tables");
        foreach (var item in LoadAsync(_path)) yield return item;
    }
    public static void LoadPF()
    {
        if (!PF.__Load) return;
        _LOG.Debug("loading table PF");
        CSVReader _reader = new CSVReader(_IO.ReadText(_path + "PF.csv"));
        PF[] temp = _reader.ReadObject<PF[]>();
        if (OnLoadPF != null) OnLoadPF(temp);
        _PF = temp;
    }
    public static IEnumerable<AsyncReadFile> LoadPFAsync()
    {
        if (!PF.__Load) yield break;
        _LOG.Debug("loading async table PF");
        var async = _IO.ReadAsync(_path + "PF.csv");
        if (!async.IsEnd) yield return async;
        CSVReader _reader = new CSVReader(_IO.ReadPreambleText(async.Data));
        PF[] temp = _reader.ReadObject<PF[]>();
        if (OnLoadPF != null) OnLoadPF(temp);
        _PF = temp;
    }
    private static void __ParseTEXT(TEXT[] array)
    {
        _TEXTByKey = array.ToDictionary(__a => __a.Key);
    }
    public static void LoadTEXT()
    {
        if (!TEXT.__Load) return;
        _LOG.Debug("loading table TEXT");
        CSVReader _reader = new CSVReader(_IO.ReadText(_path + "TEXT.csv"));
        TEXT[] temp = _reader.ReadObject<TEXT[]>();
        __ParseTEXT(temp);
        if (OnLoadTEXT != null) OnLoadTEXT(temp);
        _TEXT = temp;
    }
    public static IEnumerable<AsyncReadFile> LoadTEXTAsync()
    {
        if (!TEXT.__Load) yield break;
        _LOG.Debug("loading async table TEXT");
        var async = _IO.ReadAsync(_path + "TEXT.csv");
        if (!async.IsEnd) yield return async;
        CSVReader _reader = new CSVReader(_IO.ReadPreambleText(async.Data));
        TEXT[] temp = _reader.ReadObject<TEXT[]>();
        __ParseTEXT(temp);
        if (OnLoadTEXT != null) OnLoadTEXT(temp);
        _TEXT = temp;
    }
}
