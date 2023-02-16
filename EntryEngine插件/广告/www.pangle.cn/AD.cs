using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ByteDance.Union;
using UnityEngine;

public class AD : ADBase
{
    public override string Name { get { return "穿山甲(Unity)"; } }

    private AdNative _native;
    
    protected override void _Initialize(Action<bool, string> callback)
    {
        Pangle.InitializeSDK((success, msg) =>
        {
            if (success)
            {
                _native = SDK.CreateAdNative();
                SDK.RequestPermissionIfNecessary();
            }
            if (callback != null)
                callback(success, msg);
        });
    }
    protected override void _LoadAD(LoadedAD load)
    {
        var builder = new AdSlot.Builder()
               .SetCodeId(load.ADID)
               .SetNativeAdType(ChangeADType(load.Type))
               .SetAdCount(1)
               .SetSupportDeepLink(true);
        if (load.Width != null && load.Height != null)
        {
            var width = (int)load.Width.Value;
            var height = (int)load.Height.Value;
            float px2dp = 160 / Screen.dpi;
            builder.SetImageAcceptedSize(width, height)
                .SetExpressViewAcceptedSize(width * px2dp, height * px2dp)
                .SetOrientation(width < height ? AdOrientation.Vertical : AdOrientation.Horizontal);
        }
        AdSlot slot = builder.Build();
        switch (load.Type)
        {
            case EADType.Banner:
                _native.LoadExpressBannerAd(slot, new ExpressAdListener() { AD = load, Type = 1 });
                break;
            case EADType.Interaction:
                //_native.LoadExpressInterstitialAd(slot, new ExpressAdListener() { AD = load, Type = 2 });
                // 新插屏广告
                _native.LoadFullScreenVideoAd(slot, new FullScreenVideoAdListener() { AD = load });
                break;
            case EADType.Splash:
                _native.LoadSplashAd(slot, new SplashAdListener() { AD = load });
                break;
            default:
                // 奖励广告
                _native.LoadRewardVideoAd(slot, new RewardVideoAdListener() { AD = load });
                break;
        }
    }
    public override void ShowAD(LoadedAD ad)
    {
        if (!ad.IsLoaded)
            return;
        switch (ad.Type)
        {
            case EADType.Banner:
                NativeAdManager.Instance().ShowExpressBannerAd(SDK.GetActivity(), ((ExpressAd)ad.AD).handle, new ExpressAdListener() { AD = ad, Type = 1 }, ExpressAdListener.DislikeInteractionListener.Instance);
                break;
            case EADType.Interaction:
                // 新插屏广告
                ((FullScreenVideoAd)ad.AD).ShowFullScreenVideoAd();
                break;
            case EADType.Splash:
                break;
            default:
                ((RewardVideoAd)ad.AD).ShowRewardVideoAd();
                break;
        }
    }

    class AdListenerBase
    {
        internal LoadedAD AD;
        protected void Reward()
        {
            if (AD.OnReward != null)
                AD.OnReward(AD);
        }
    }
    class AdDisposible : IDisposable
    {
        public object AD;
        public AdDisposible(object ad)
        {
            this.AD = ad;
        }
        public void Dispose()
        {
            // todo: 关闭广告
            AD = null;
        }
    }
    class ExpressAdListener : AdListenerBase, IExpressAdListener, IExpressAdInteractionListener
    {
        /// <summary>0: Feed / 1: Banner / 2: Interaction</summary>
        public int Type;
        public class DislikeInteractionListener : IDislikeInteractionListener
        {
            public static DislikeInteractionListener Instance = new DislikeInteractionListener();
            public void OnSelected(int var1, string var2, bool enforce)
            {
            }
            public void OnCancel()
            {
            }
            public void OnShow()
            {
            }
        }
        public void OnError(int code, string message)
        {
        }
        public void OnExpressAdLoad(List<ExpressAd> ads)
        {
            foreach (var ad in ads)
            {
                ad.SetSlideIntervalTime(30 * 1000);
                ad.SetExpressInteractionListener(this);
                AD.AD = ad;
            }
        }
        public void OnAdViewRenderSucc(ExpressAd ad, float width, float height)
        {
        }
        public void OnAdViewRenderError(ExpressAd ad, int code, string message)
        {
        }
        public void OnAdShow(ExpressAd ad)
        {
        }
        public void OnAdClicked(ExpressAd ad)
        {
        }
        public void OnAdClose(ExpressAd ad)
        {
            // bug: 关掉了Banner广告也不会回调
            Reward();
        }
        public void onAdRemoved(ExpressAd ad)
        {
        }
    }
    class InteractionAdListener : AdListenerBase, IInteractionAdListener, IInteractionAdInteractionListener
    {
        public void OnError(int code, string message)
        {
        }
        public void OnInteractionAdLoad(InteractionAd ad)
        {
            ad.SetAdInteractionListener(this);
            if (ad.GetInteractionType() == 4)
            {
                ad.SetDownloadListener(new AppDownloadListener(this));
            }
            AD.AD = ad;
            //ad.ShowInteractionAd();
        }

        public void OnAdClicked()
        {
        }
        public void OnAdShow()
        {
        }
        public void OnAdDismiss()
        {
        }
        public void onAdRemoved()
        {
        }
    }
    class SplashAdListener : AdListenerBase, ISplashAdListener
    {
        public void OnError(int code, string message)
        {
        }
        public void OnSplashAdLoad(BUSplashAd ad)
        {
            ad.SetDownloadListener(new AppDownloadListener(this));
            AD.AD = ad;
        }
        public void OnTimeout()
        {
        }
    }
    class FullScreenVideoAdListener : AdListenerBase, IFullScreenVideoAdListener, IFullScreenVideoAdInteractionListener
    {
        public void OnError(int code, string message)
        {
        }
        public void OnFullScreenVideoAdLoad(FullScreenVideoAd ad)
        {
            //ad.SetDownloadListener(new AppDownloadListener(this));
            ad.SetFullScreenVideoAdInteractionListener(this);
            AD.AD = ad;
        }
        public void OnFullScreenVideoCached()
        {
        }
        public void OnExpressFullScreenVideoAdLoad(ExpressFullScreenVideoAd ad)
        {
        }

        void IFullScreenVideoAdInteractionListener.OnAdShow()
        {
            Reward();
        }
        void IFullScreenVideoAdInteractionListener.OnAdVideoBarClick()
        {
        }
        void IFullScreenVideoAdInteractionListener.OnAdClose()
        {
        }
        void IFullScreenVideoAdInteractionListener.OnVideoComplete()
        {
        }
        void IFullScreenVideoAdInteractionListener.OnSkippedVideo()
        {
        }
        void IFullScreenVideoAdInteractionListener.OnVideoError()
        {
        }
    }
    class RewardVideoAdListener : AdListenerBase, IRewardVideoAdListener, IRewardAdInteractionListener
    {
        public void OnError(int code, string message)
        {
        }
        public void OnRewardVideoAdLoad(RewardVideoAd ad)
        {
            ad.SetRewardAdInteractionListener(this);
            AD.AD = ad;
        }
        public void OnRewardVideoCached()
        {
        }
        public void OnExpressRewardVideoAdLoad(ExpressRewardVideoAd ad)
        {
        }

        void IRewardAdInteractionListener.OnAdShow()
        {
        }
        void IRewardAdInteractionListener.OnAdVideoBarClick()
        {
        }
        void IRewardAdInteractionListener.OnAdClose()
        {
        }
        void IRewardAdInteractionListener.OnVideoComplete()
        {
        }
        void IRewardAdInteractionListener.OnVideoSkip()
        {
        }
        void IRewardAdInteractionListener.OnVideoError()
        {
        }
        void IRewardAdInteractionListener.OnRewardVerify(bool rewardVerify, int rewardAmount, string rewardName)
        {
            if (rewardVerify)
                Reward();
        }
    }
    class AppDownloadListener : IAppDownloadListener
    {
        AdListenerBase _base;
        public AppDownloadListener(AdListenerBase _base)
        {
            this._base = _base;
        }
        public void OnIdle()
        {
        }
        public void OnDownloadActive(long totalBytes, long currBytes, string fileName, string appName)
        {
        }
        public void OnDownloadPaused(long totalBytes, long currBytes, string fileName, string appName)
        {
        }
        public void OnDownloadFailed(long totalBytes, long currBytes, string fileName, string appName)
        {
        }
        public void OnDownloadFinished(long totalBytes, string fileName, string appName)
        {
        }
        public void OnInstalled(string fileName, string appName)
        {
        }
    }
    static AdSlotType ChangeADType(EADType type)
    {
        switch (type)
        {
            case EADType.Banner: return AdSlotType.Banner;
            case EADType.Interaction: return AdSlotType.InteractionAd;
            case EADType.Splash: return AdSlotType.Splash;
            case EADType.Video: return AdSlotType.FullScreenVideo;
            default: return (AdSlotType)0;
        }
    }
}