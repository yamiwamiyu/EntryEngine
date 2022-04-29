using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine;
using EntryEngine.UI;
using EntryEngine.Serialize;

/// <summary>生成的UI代码都继承于这个类，可以默认使用类里的静态变量</summary>
public class UIScene : EntryEngine.UI.UIScene
{
    /// <summary>账号登录信息保存文件</summary>
    const string TOKEN = "#login.sav";

    /// <summary>用于登录等的接口</summary>
    internal static IServiceProxy pservice = new IServiceProxy();
    internal static IUserProxy puser = new IUserProxy();
    /// <summary>玩家账号信息</summary>
    internal static T_USER User;

    static UIScene()
    {
#if !DEBUG
        // 正式服地址
        Pservice.Host = "http://ip/Action/";
#else
        // 本地|测试服地址
        pservice.Host = "http://127.0.0.1:888/Action/";
#endif
        pservice.OnError += (r, ex) =>
        {
            STextHint.ShowHint(ex.Message);
            _LOG.Warning("网络异常：{0}", ex.Message);
        };
        pservice.OnHttpError += (code, msg) =>
        {
            Entry.Instance.Synchronize(() =>
            {
                STextHint.ShowHint(msg);
                _LOG.Warning(msg);
            });
        };
        pservice.IsAsync = false;

        puser.Host = pservice.Host;
        // 在Header中带上登录的Token信息
        puser.OnSend = (r) => r.Request.Headers["AccessToken"] = User.Token;
        puser.OnError = pservice.OnError;
        puser.OnHttpError = pservice.OnHttpError;
        puser.IsAsync = false;
    }

    /// <summary>清除用户登录信息，用于例如退出登录</summary>
    internal static void ClearToken()
    {
        User.ID = 0;
        User.Token = null;
        SaveToken();
    }
    /// <summary>保存用户登录信息，用于例如登录成功</summary>
    private static void SaveToken()
    {
        ByteWriter writer = new ByteWriter();
        writer.Write(User.Token);
        writer.Write(User.LastLoginTime);
        _IO.WriteByte(TOKEN, writer.GetBuffer());
    }
    /// <summary>加载用户登录信息，用于重新打开游戏</summary>
    internal static IEnumerable<ICoroutine> LoadToken()
    {
        var async = _IO.ReadAsync(TOKEN);
        if (!async.IsEnd) yield return async;
        if (async.IsSuccess)
        {
            byte[] bytes = async.Data;
            User = new T_USER();
            ByteReader reader = new ByteReader(bytes);
            reader.Read(out User.Token);
            reader.Read(out User.LastLoginTime);
        }
    }
    /// <summary>登录成功的回调函数，例如pservice.LoginBySMSCode("177****9190", "1234", On登录成功)</summary>
    internal static void On登录成功(T_USER u)
    {
        User = u;
        // todo: 登录后的操作
        //Entry.Instance.ShowMainScene<>();
        SaveToken();
    }
}
public class MAIN : UIScene
{
    protected override IEnumerable<ICoroutine> Loading()
    {
        // 渲染设置：分辨率，屏幕外剔除渲染
        Entry.GRAPHICS.GraphicsSize = new VECTOR2(900, 1600);
        //Entry.GRAPHICS.Culling = true;

        foreach (var item in LoadToken()) yield return item;

        AsyncReadFile async;

#if !DEBUG
        // 资源解密，发布时使用了EntryBuilder BuildEncrypt的资源就需要用这两行来对资源解密
	    Entry.OnNewiO += (io) => _IO.SetDecrypt(io);
        _IO.SetDecrypt(_IO._iO);

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
