using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ByteDance.Union;

public class AD : ADBase
{
    public override string Name { get { return "穿山甲(Unity)"; } }

    private AdNative _native;
    public override void LoadAD(string adid, EADType type)
    {
    }
    protected override void _Initialize(Action<bool, string> callback)
    {
        SDK.RequestPermissionIfNecessary();
        Pangle.InitializeSDK((success, msg) =>
        {
            if (success)
            {
                _native = SDK.CreateAdNative();
            }
            if (callback != null)
                callback(success, msg);
        });
    }
    public override void ShowAD(string adid, EADType type, float x, float y, int width, int height, Action onOver)
    {
        AdSlot slot = new AdSlot.Builder()
            .SetCodeId(adid)
            //.SetNativeAdType(ChangeADType(type))
            .SetAdCount(1)
            .SetImageAcceptedSize(width, height)
            .SetExpressViewAcceptedSize(width, height)
            .SetOrientation(width < height ? AdOrientation.Vertical : AdOrientation.Horizontal)
            .SetSupportDeepLink(true)
            .Build();
        switch (type)
        {
            case EADType.Banner:
                _native.LoadExpressBannerAd(slot, new ExpressAdListener()
                {
                    onOver = onOver,
                    x = x,
                    y = y,
                    Type = 1,
                });
                break;
            case EADType.Interaction:
                //_native.LoadExpressInterstitialAd(slot, new ExpressAdListener()
                //{
                //    onError = onError,
                //    onLoad = onLoad,
                //    onClick = onClick,
                //    onOver = onOver,
                //    x = x,
                //    y = y,
                //    Type = 2,
                //});
                // 新插屏广告
                _native.LoadFullScreenVideoAd(slot, new FullScreenVideoAdListener()
                {
                    onOver = onOver,
                    x = x,
                    y = y,
                });
                break;
            case EADType.Splash:
                _native.LoadExpressSplashAd(slot, new SplashAdListener()
                {
                    onOver = onOver,
                    x = x,
                    y = y,
                });
                break;
            default:
                //_native.LoadFullScreenVideoAd(slot, new FullScreenVideoAdListener()
                //{
                //    onError = onError,
                //    onLoad = onLoad,
                //    onClick = onClick,
                //    onOver = onOver,
                //    x = x,
                //    y = y,
                //});
                // 奖励广告
                _native.LoadRewardVideoAd(slot, new RewardVideoAdListener()
                {
                    onOver = onOver,
                    x = x,
                    y = y,
                });
                break;
        }
    }

    class AdListenerBase
    {
        internal Action<int, string> onError;
        internal Action<IDisposable> onLoad;
        internal Action onClick;
        internal Action onOver;
        internal float x;
        internal float y;
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
        class DislikeInteractionListener : IDislikeInteractionListener
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
            if (onError != null)
                onError(code, message);
        }
        public void OnExpressAdLoad(List<ExpressAd> ads)
        {
            foreach (var ad in ads)
            {
                //if (ad.GetInteractionType() == 4)
                //    ad.SetDownloadListener(new AppDownloadListener(this));
                //else
                ad.SetSlideIntervalTime(30 * 1000);
                ad.SetExpressInteractionListener(this);
                //ad.SetDownloadListener(new AppDownloadListener(this));
                if (Type == 1)
                    NativeAdManager.Instance().ShowExpressBannerAd(SDK.GetActivity(), ad.handle, this, DislikeInteractionListener.Instance);
                else if (Type == 2)
                    NativeAdManager.Instance().ShowExpressInterstitialAd(SDK.GetActivity(), ad.handle, this);
                else
                    NativeAdManager.Instance().ShowExpressFeedAd(SDK.GetActivity(), ad.handle, this, DislikeInteractionListener.Instance);
            }
        }

        public void OnAdViewRenderSucc(ExpressAd ad, float width, float height)
        {
            if (onLoad != null)
                onLoad(ad);
        }
        public void OnAdViewRenderError(ExpressAd ad, int code, string message)
        {
            if (onError != null)
                onError(code, message);
        }
        public void OnAdShow(ExpressAd ad)
        {
        }
        public void OnAdClicked(ExpressAd ad)
        {
            if (onClick != null)
                onClick();
        }
        public void OnAdClose(ExpressAd ad)
        {
            if (onOver != null)
                onOver();
        }
        public void onAdRemoved(ExpressAd ad)
        {
        }
    }
    class BannerAdListener : AdListenerBase, IBannerAdListener
    {
        public void OnError(int code, string message)
        {
            if (onError != null)
                onError(code, message);
        }
        public void OnBannerAdLoad(BannerAd ad)
        {
            ad.SetDownloadListener(new AppDownloadListener(this));
            if (onLoad != null)
                onLoad(new AdDisposible(ad));
        }
    }
    class InteractionAdListener : AdListenerBase, IInteractionAdListener, IInteractionAdInteractionListener
    {
        public void OnError(int code, string message)
        {
            if (onError != null)
                onError(code, message);
        }
        public void OnInteractionAdLoad(InteractionAd ad)
        {
            ad.SetAdInteractionListener(this);
            if (ad.GetInteractionType() == 4)
            {
                ad.SetDownloadListener(new AppDownloadListener(this));
            }
            if (onLoad != null)
                onLoad(new AdDisposible(ad));
            ad.ShowInteractionAd();
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
            if (onError != null)
                onError(code, message);
        }
        public void OnSplashAdLoad(BUSplashAd ad)
        {
            ad.SetDownloadListener(new AppDownloadListener(this));
            if (onLoad != null)
                onLoad(new AdDisposible(ad));
        }
        public void OnTimeout()
        {
            if (onError != null)
                onError(-1, "加载超时");
        }
    }
    class FullScreenVideoAdListener : AdListenerBase, IFullScreenVideoAdListener, IFullScreenVideoAdInteractionListener
    {
        public void OnError(int code, string message)
        {
            if (onError != null)
                onError(code, message);
        }
        public void OnFullScreenVideoAdLoad(FullScreenVideoAd ad)
        {
            //ad.SetDownloadListener(new AppDownloadListener(this));
            ad.SetFullScreenVideoAdInteractionListener(this);
            if (onLoad != null)
                onLoad(new AdDisposible(ad));
            ad.ShowFullScreenVideoAd();
        }
        public void OnFullScreenVideoCached()
        {
        }
        public void OnExpressFullScreenVideoAdLoad(ExpressFullScreenVideoAd ad)
        {
            //_LOG.Debug("OnExpressFullScreenVideoAdLoad");
            //ad.SetDownloadListener(new AppDownloadListener(this));
            //if (onLoad != null)
            //    onLoad(new AdDisposible(ad));
            //ad.ShowFullScreenVideoAd();
        }

        void IFullScreenVideoAdInteractionListener.OnAdShow()
        {
        }
        void IFullScreenVideoAdInteractionListener.OnAdVideoBarClick()
        {
            if (onClick != null)
                onClick();
        }
        void IFullScreenVideoAdInteractionListener.OnAdClose()
        {
        }
        void IFullScreenVideoAdInteractionListener.OnVideoComplete()
        {
            if (onOver != null)
                onOver();
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
            if (onError != null)
                onError(code, message);
        }
        public void OnRewardVideoAdLoad(RewardVideoAd ad)
        {
            if (onLoad != null)
                onLoad(new AdDisposible(ad));
            ad.SetRewardAdInteractionListener(this);
            ad.ShowRewardVideoAd();
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
            if (onClick != null)
                onClick();
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
            if (onOver != null)
                onOver();
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
            if (_base.onClick != null)
                _base.onClick();
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