using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>广告类型</summary>
public enum EADType
{
    /// <summary>不一定每个平台都需要指定广告类型</summary>
    Unknown,
    /// <summary>横幅：长时间显示</summary>
    Banner,
    /// <summary>插屏：过关等弹出</summary>
    Interaction,
    /// <summary>开屏：APP刚打开</summary>
    Splash,
    /// <summary>视频：看完获取某种奖励</summary>
    Video,
}
public abstract class ADBase
{
    /// <summary>AutoCreateFromAvailableFactory和CreateEmpty会自动赋值的广告实例</summary>
    public static ADBase AD;

    /// <summary>广告平台名字</summary>
    public abstract string Name { get; }

    public string AppID { get; private set; }
    public string AppName { get; private set; }
    public bool IsInitialized { get { return !string.IsNullOrEmpty(AppID); } }

    /// <summary>初始化SDK</summary>
    /// <param name="appID">广告所属AppID，广告平台创建应用一般都有的</param>
    /// <param name="appName">广告所属App名字，广告平台创建应用一般都有的</param>
    /// <param name="callback">bool: 初始化成功/失败；string: 初始化失败原因</param>
    public void Initialize(string appID, string appName, Action<bool, string> callback)
    {
        if (string.IsNullOrEmpty(appID))
            throw new ArgumentNullException("SDK AppID 不能为空！");
        this.AppID = appID;
        this.AppName = appName;
        _Initialize(callback);
    }
    protected abstract void _Initialize(Action<bool, string> callback);

    /// <summary>展示一条广告</summary>
    /// <param name="adid">广告在平台的ID</param>
    /// <param name="type">广告类型</param>
    /// <param name="x">广告显示位置，单位屏幕像素</param>
    /// <param name="y">广告显示位置，单位屏幕像素</param>
    /// <param name="width">广告宽度，单位屏幕像素</param>
    /// <param name="height">广告高度，单位屏幕像素</param>
    /// <param name="onError">广告显示错误时回调错误码和错误信息</param>
    /// <param name="onLoad">广告显示成功时回调广告对象</param>
    /// <param name="onClick">广告被点击时回调</param>
    /// <param name="onOver">广告看完时回调，一般用于看完有奖励的广告</param>
    /// <returns>广告对象</returns>
    public void ShowAD(string adid, EADType type,
        float x, float y, int width, int height,
        Action<int, string> onError, Action<IDisposable> onLoad, Action onClick, Action onOver)
    {
        _ShowAD(adid, type, x, y, width, height, onError, onLoad, onClick, onOver);
    }
    public void ShowAD(string adid)
    {
        _ShowAD(adid, EADType.Unknown, 0, 0, 0, 0, null, null, null, null);
    }
    protected abstract void _ShowAD(string adid, EADType type,
        float x, float y, int width, int height,
        Action<int, string> onError, Action<IDisposable> onLoad, Action onClick, Action onOver);

    /// <summary>从可用的程序集中自动创建ADBase的实例，若有多个类型则返回首个检测到的类型实例</summary>
    public static ADBase AutoCreateFromAvailableFactory()
    {
        Type _AutoType = null;
        {
            var current = System.Reflection.Assembly.GetExecutingAssembly();
            var currentSDK = typeof(ADBase);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var item in assemblies)
            {
                var referenced = item.GetReferencedAssemblies();
                foreach (var r in referenced)
                {
                    if (r.FullName == current.FullName)
                    {
                        foreach (var type in item.GetTypes())
                        {
                            if (!type.IsAbstract && type.IsAnsiClass && type.BaseType != null && type.BaseType == currentSDK)
                            {
                                _AutoType = type;
                                return (ADBase)Activator.CreateInstance(_AutoType);
                            }
                        }
                    }
                }
            }
        }
        if (_AutoType == null)
            AD = new ADEmpty();
        else
            AD = (ADBase)Activator.CreateInstance(_AutoType);
        return AD;
    }
    public static ADBase CreateEmpty()
    {
        AD = new ADEmpty();
        return AD;
    }
}
internal class ADEmpty : ADBase
{
    public override string Name
    {
        get { return "广告测试"; }
    }
    protected override void _Initialize(Action<bool, string> callback)
    {
        callback(true, "");
    }
    protected override void _ShowAD(string adid, EADType type, float x, float y, int width, int height, Action<int, string> onError, Action<IDisposable> onLoad, Action onClick, Action onOver)
    {
        if (onLoad != null)
            onLoad(null);
        if (onOver != null)
            onOver();
    }
}