using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine;
using EntryEngine.UI;

public class MAIN : UIScene
{
    protected override IEnumerable<ICoroutine> Loading()
    {
        // 你可以自定义你的第一个初始菜单来替换MAIN

        AsyncReadFile async;

        // 你还可以加载其它一些常量表

        //async = _IO.ReadAsync("Tables\\CC.xml");
        //if (!async.IsEnd) yield return async;
        //_CC.Load(_IO.ReadPreambleText(async.Data));
        //_LOG.Debug("加载客户端常量表完毕");

        //async = _IO.ReadAsync("Tables\\C.xml");
        //if (!async.IsEnd) yield return async;
        //_C.Load(_IO.ReadPreambleText(async.Data));
        //_LOG.Debug("加载常量表完毕");

        Encoding encoding = _IO.IOEncoding;
        _IO.IOEncoding = Encoding.Default;

        async = _IO.ReadAsync("Tables\\LANGUAGE.csv");
        if (!async.IsEnd) yield return async;
        _LANGUAGE.Load(_IO.ReadPreambleText(async.Data), "");
        _LOG.Debug("加载语言表完毕");

        //foreach (var item in _TABLE.LoadAsync("Tables\\"))
        //    if (!item.IsEnd)
        //        yield return item;
        //_LOG.Debug("加载数据表完毕");

        _IO.IOEncoding = encoding;

        // 确保加载完了所有的基本资源后就可以启动第一个正式的菜单了
        //Entry.ShowMainScene<T>();
        this.State = EState.Release;
    }
}
