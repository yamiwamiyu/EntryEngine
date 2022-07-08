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
/// <summary>预加载完成的广告</summary>
public class LoadedAD
{
    /// <summary>广告在平台的ID</summary>
    public string ADID { get; internal set; }
    /// <summary>广告类型</summary>
    public EADType Type { get; internal set; }
    /// <summary>广告展示X坐标，可以不设置</summary>
    public int? X { get; internal set; }
    /// <summary>广告展示Y坐标，可以不设置</summary>
    public int? Y { get; internal set; }
    /// <summary>广告展示宽度，可以不设置</summary>
    public int? Width { get; internal set; }
    /// <summary>广告展示高度，可以不设置</summary>
    public int? Height { get; internal set; }
    /// <summary>加载成功时的回调</summary>
    internal Action<LoadedAD> OnLoad;
    private object ad;
    /// <summary>加载好的AD对象</summary>
    public object AD
    {
        get { return ad; }
        set
        {
            ad = value;
            if (value != null && OnLoad != null)
                OnLoad(this);
        }
    }
    public bool IsLoaded { get { return AD != null; } }
}
public abstract class ADBase
{
    /// <summary>AutoCreateFromAvailableFactory和CreateEmpty会自动赋值的广告实例</summary>
    public static ADBase AD;

    private static Dictionary<EADType, Dictionary<string, LoadedAD>> loads
        = new Dictionary<EADType, Dictionary<string, LoadedAD>>();

    /// <summary>广告平台名字</summary>
    public abstract string Name { get; }
    /// <summary>获取已经加载的AD对象，返回null则没加载好</summary>
    public LoadedAD this[string adid, EADType type]
    {
        get
        {
            Dictionary<string, LoadedAD> temp;
            LoadedAD ad;
            if (loads.TryGetValue(type, out temp))
                if (temp.TryGetValue(adid, out ad))
                    return ad;
            return null;
        }
    }

    public string AppID { get; private set; }
    public string AppName { get; private set; }
    public bool IsInitialized { get { return !string.IsNullOrEmpty(AppID); } }

    /// <summary>初始化SDK，若初始化失败ADBase.AD将置为空广告对象</summary>
    /// <param name="appID">广告所属AppID，广告平台创建应用一般都有的</param>
    /// <param name="appName">广告所属App名字，广告平台创建应用一般都有的</param>
    /// <param name="callback">bool: 初始化成功/失败；string: 初始化失败原因</param>
    public void Initialize(string appID, string appName, Action<bool, string> callback)
    {
        if (string.IsNullOrEmpty(appID))
            throw new ArgumentNullException("SDK AppID 不能为空！");
        this.AppID = appID;
        this.AppName = appName;
        try
        {
            _Initialize((ret, msg) =>
            {
                if (!ret && AD == this)
                    AD = CreateEmpty();
                if (callback != null)
                    callback(ret, msg);
            });
        }
        catch (Exception ex)
        {
            if (AD == this)
                AD = CreateEmpty();
            throw ex;
        }
    }
    protected abstract void _Initialize(Action<bool, string> callback);

    /// <summary>预加载一条广告，若已经加载过则也会重复加载</summary>
    /// <param name="adid">广告在平台的ID</param>
    /// <param name="type">广告类型</param>
    /// <param name="x">广告显示位置，单位屏幕像素</param>
    /// <param name="y">广告显示位置，单位屏幕像素</param>
    /// <param name="width">广告宽度，单位屏幕像素</param>
    /// <param name="height">广告高度，单位屏幕像素</param>
    /// <param name="onLoad">加载成功时回调，仅最后一次回调生效</param>
    public void LoadAD(string adid, EADType type, int? x, int? y, int? width, int? height, Action<LoadedAD> onLoad)
    {
        Dictionary<string, LoadedAD> temp;
        LoadedAD ad;
        if (!loads.TryGetValue(type, out temp))
        {
            temp = new Dictionary<string, LoadedAD>();
            loads.Add(type, temp);
        }
        if (!temp.TryGetValue(adid, out ad))
        {
            ad = new LoadedAD();
            ad.Type = type;
            ad.ADID = adid;
            temp.Add(adid, ad);
        }
        ad.X = x;
        ad.Y = y;
        ad.Width = width;
        ad.Height = height;
        ad.OnLoad = onLoad;
        _LoadAD(ad);
    }
    /// <summary>预加载一条广告（不一定需要预加载，可以不实现；实现时对load.AD赋值即可）</summary>
    protected virtual void _LoadAD(LoadedAD load) { load.AD = "loaded"; }
    /// <summary>加载并展示一条广告</summary>
    /// <param name="adid">广告在平台的ID</param>
    /// <param name="type">广告类型</param>
    /// <param name="x">广告显示位置，单位屏幕像素</param>
    /// <param name="y">广告显示位置，单位屏幕像素</param>
    /// <param name="width">广告宽度，单位屏幕像素</param>
    /// <param name="height">广告高度，单位屏幕像素</param>
    /// <param name="onReward">广告看完时回调，一般用于看完有奖励的广告</param>
    public void ShowAD(string adid, EADType type,
        int? x, int? y, int? width, int? height,
        Action onReward)
    {
        var ad = this[adid, type];
        if (ad == null)
            LoadAD(adid, type, x, y, width, height, i => ShowAD(i, onReward));
        else
            ShowAD(ad, onReward);
    }
    public void ShowAD(string adid, EADType type)
    {
        ShowAD(adid, type, null, null, null, null, null);
    }
    public void ShowAD(string adid, EADType type, Action onReward)
    {
        ShowAD(adid, type, null, null, null, null, onReward);
    }
    public void ShowAD(string adid)
    {
        ShowAD(adid, EADType.Unknown, null, null, null, null, null);
    }
    public abstract void ShowAD(LoadedAD ad, Action onReward);

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
                                AD = (ADBase)Activator.CreateInstance(_AutoType);
                                return AD;
                            }
                        }
                    }
                }
            }
        }
        if (_AutoType == null)
            AD = new ADEmpty();
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
    public override void ShowAD(LoadedAD ad, Action onReward)
    {
        if (onReward != null)
            onReward();
    }
    protected override void _Initialize(Action<bool, string> callback)
    {
        callback(true, "");
    }
}