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
        // 渲染设置：分辨率，屏幕外剔除渲染
        Entry.GRAPHICS.GraphicsSize = new VECTOR2(900, 1600);
        //Entry.GRAPHICS.Culling = true;

        AsyncReadFile async;

#if DEBUG
        // 资源解密，发布时使用了EntryBuilder BuildEncrypt的资源就需要用这两行来对资源解密
	    //Entry.OnNewiO += (io) => _IO.SetDecrypt(io);
        //_IO.SetDecrypt(_IO._iO);

        // 可以加载打包大图的配置
        async = _IO.ReadAsync("piece.pcsv");
        if (!async.IsEnd) yield return async;
        PipelinePiece.GetDefaultPipeline().LoadMetadata(_IO.ReadPreambleText(async.Data));
        _LOG.Debug("加载Piece完成");
#endif
        // 你还可以加载其它一些常量表

        //async = _IO.ReadAsync("Tables\\CC.xml");
        //if (!async.IsEnd) yield return async;
        //_CC.Load(_IO.ReadPreambleText(async.Data));
        //_LOG.Debug("加载客户端常量表完毕");

        //async = _IO.ReadAsync("Tables\\C.xml");
        //if (!async.IsEnd) yield return async;
        //_C.Load(_IO.ReadPreambleText(async.Data));
        //_LOG.Debug("加载常量表完毕");

        //async = _IO.ReadAsync("Tables\\LANGUAGE.csv");
        //if (!async.IsEnd) yield return async;
        //_LANGUAGE.Load(_IO.ReadPreambleText(async.Data), "");
        //_LOG.Debug("加载语言表完毕");

        //foreach (var item in _TABLE.LoadAsync("Tables\\"))
        //    if (!item.IsEnd)
        //        yield return item;
        //_LOG.Debug("加载数据表完毕");

        #region 系统级加载

        // todo: 添加龙骨骼，Spine等第三方插件加载管道
        //Entry.OnNewContentManager += (cm) => cm.AddPipeline(new PipelineDragonBones());

        // 替换系统内容管理器，确保使用了上面的第三方插件
        Entry._ContentManager = Entry.NewContentManager();
        var content = Entry._ContentManager;

        // todo: 使用content加载一些系统资源
        //yield return content.WaitAsyncLoadHandle;

        #endregion

        // 确保加载完了所有的基本资源后就可以启动第一个正式的菜单了
        //Entry.ShowMainScene<T>();
        this.State = EState.Release;

        yield break;
    }
}
