using System;
using StarkSDKSpace;

public class AD : ADBase
{
    public static StarkAdManager SDK;
    public override string Name
    {
        get { return "头条广告联盟"; }
    }
    protected override void _Initialize(Action<bool, string> callback)
    {
        SDK = StarkSDK.API.GetStarkAdManager();
    }
    public override void LoadAD(string adid, EADType type)
    {
        if (type == EADType.Interaction)
            interstitialAd = SDK.CreateInterstitialAd(adid);
    }
    static StarkAdManager.InterstitialAd interstitialAd;

    public override void ShowAD(string adid, EADType type, float x, float y, int width, int height, Action onReward)
    {
        switch (type)
        {
            case EADType.Unknown:
                break;
            case EADType.Banner:
                var style = new StarkAdManager.BannerStyle();
                style.left = (int)x;
                style.top = (int)y;
                style.width = width;
                SDK.CreateBannerAd(adid, style, 30);
                break;
            case EADType.Interaction:
                if (interstitialAd == null)
                    LoadAD(adid, type);
                if (interstitialAd.IsLoaded())
                {
                    interstitialAd.Show();
                    LoadAD(adid, type);
                }
                break;
            case EADType.Splash:
                break;
            case EADType.Video: 
                SDK.ShowVideoAdWithId(adid, (b) => {
                    if (b) onReward();
                });
                break;
        }
    }
}