//using System;
//using System.Collections.Generic;
//using System.Linq;
//using EntryEngine;
//using EntryEngine.Serialize;

//public partial class Text
//{
//    public static bool __Load = true;
//    /// <summary>文字信息ID</summary>
//    public enum ETextID
//    {
//        NameEmpty,
//        PasswordEmpty,
//        EntryPointEmpty,
//        WaitConnect,
//        ConnectError,
//        DeleteConfirm,
//        All,
//        TypeSame,
//        SVNEmpty,
//        ExeEmpty,
//        Wait,
//        SVNPath,
//        ServerEmpty,
//        STEmpty,
//        Single,
//        NewService,
//        ModService,
//        Unusable,
//        ServiceAvailable,
//        HotFix,
//        SelectionEmpty,
//        LoginAgain,
//        LoginError,
//        DateError,
//        LogEmpty,
//        Close,
//        UpdateClose,
//    }
//    public ETextID ID;
//    /// <summary>文字内容</summary>
//    public string Content
//    {
//        get { return _LANGUAGE.GetString(__Content); }
//        set { __Content = value; }
//    }
//    private string __Content;
//}
//static partial class _TABLE
//{
//    public static string _path { get; private set; }
//    public static Text[] _Text;
//    public static Dictionary<Text.ETextID, Text> _TextByID;
    
//    public static event Action<Text[]> OnLoadText;
    
//    public static void SetPath(string path)
//    {
//        if (_path != path)
//        {
//            _path = _IO.DirectoryWithEnding(path);
//            _LOG.Info("Set table path: {0}", _path);
//        }
//    }
//    public static void Load(string path)
//    {
//        SetPath(path);
//        _LOG.Info("Load all tables", path);
//        LoadText();
//    }
//    public static IEnumerable<AsyncReadFile> LoadAsync(string path)
//    {
//        SetPath(path);
//        _LOG.Info("LoadAsync all tables", path);
//        foreach (var item in LoadTextAsync()) yield return item;
//        yield break;
//    }
//    public static void Reload()
//    {
//        _LOG.Info("Reload all tables");
//        Load(_path);
//    }
//    public static IEnumerable<AsyncReadFile> ReloadAsync()
//    {
//        _LOG.Info("ReloadAsync all tables");
//        foreach (var item in LoadAsync(_path)) yield return item;
//    }
//    private static void __ParseText(Text[] array)
//    {
//        _TextByID = array.ToDictionary(__a => __a.ID);
//    }
//    public static void LoadText()
//    {
//        if (!Text.__Load) return;
//        _LOG.Debug("loading table Text");
//        CSVReader _reader = new CSVReader(_IO.ReadText(_path + "Text.csv"));
//        Text[] temp = _reader.ReadObject<Text[]>();
//        __ParseText(temp);
//        if (OnLoadText != null) OnLoadText(temp);
//        _Text = temp;
//    }
//    public static IEnumerable<AsyncReadFile> LoadTextAsync()
//    {
//        if (!Text.__Load) yield break;
//        _LOG.Debug("loading async table Text");
//        var async = _IO.ReadAsync(_path + "Text.csv");
//        if (!async.IsEnd) yield return async;
//        CSVReader _reader = new CSVReader(_IO.ReadPreambleText(async.Data));
//        Text[] temp = _reader.ReadObject<Text[]>();
//        __ParseText(temp);
//        if (OnLoadText != null) OnLoadText(temp);
//        _Text = temp;
//    }
//}
