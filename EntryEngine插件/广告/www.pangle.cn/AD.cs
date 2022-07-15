﻿using System;
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
        Debug.LogFormat("_LoadAD! ID: {0} Type: {1}", load.ADID, load.Type);
        if (load.Width != null && load.Height != null)
        {
            int width = load.Width.Value;
            int height = load.Height.Value;
            builder.SetImageAcceptedSize(1080, 1920)
                .SetExpressViewAcceptedSize(width, height)
                .SetOrientation(width < height ? AdOrientation.Vertical : AdOrientation.Horizontal);
            Debug.LogFormat("SetExpressViewAcceptedSize! Width: {0} Height: {1}", load.Width.Value, load.Height.Value);
        }
        AdSlot slot = builder.Build();
        switch (load.Type)
        {
            case EADType.Banner:
                _native.LoadExpressBannerAd(slot, new ExpressAdListener() { AD = load, Type = 1 });
                break;
            case EADType.Interaction:
                _native.LoadExpressInterstitialAd(slot, new ExpressAdListener() { AD = load, Type = 2 });
                // 新插屏广告
                //_native.LoadFullScreenVideoAd(slot, new FullScreenVideoAdListener() { AD = load });
                break;
            case EADType.Splash:
                _native.LoadExpressSplashAd(slot, new SplashAdListener() { AD = load });
                break;
            default:
                // 奖励广告
                _native.LoadRewardVideoAd(slot, new RewardVideoAdListener() { AD = load });
                break;
        }
        Debug.LogFormat("_LoadAD Completed! ID: {0} Type: {1}", load.ADID, load.Type);
    }
    public override void ShowAD(LoadedAD ad, Action onReward)
    {
        if (ad.IsLoaded)
        {
            switch (ad.Type)
            {
                case EADType.Banner:
                    //((ExpressAd)ad.AD).ShowExpressAd(ad.X.Value, ad.Y.Value);
                    ((ExpressAd)ad.AD).SetSlideIntervalTime(30 * 1000);
                    NativeAdManager.Instance().ShowExpressBannerAd(SDK.GetActivity(), ((ExpressAd)ad.AD).handle, null, null);
                    break;
                case EADType.Interaction:
                    NativeAdManager.Instance().ShowExpressInterstitialAd(SDK.GetActivity(), ((ExpressAd)ad.AD).handle, null);
                    break;
                case EADType.Splash:
                    break;
                case EADType.Video:
                    ((RewardVideoAd)ad.AD).ShowRewardVideoAd();
                    break;
            }
        }
    }

    class AdListenerBase
    {
        internal LoadedAD AD;
        internal Action OnReward;
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
            Debug.LogErrorFormat("ExpressAdListener Error! code: {0} msg: {1}", code, message);
        }
        public void OnExpressAdLoad(List<ExpressAd> ads)
        {
            Debug.LogFormat("OnExpressAdLoad: {0}", ads.Count);
            foreach (var ad in ads)
            {
                //if (AD.X != null && AD.Y != null)
                //    ad.ShowExpressAd(AD.X.Value, AD.Y.Value);
                //ad.SetSlideIntervalTime(30 * 1000);
                //ad.SetExpressInteractionListener(this);
                AD.AD = ad;
            }
        }

        public void OnAdViewRenderSucc(ExpressAd ad, float width, float height)
        {
        }
        public void OnAdViewRenderError(ExpressAd ad, int code, string message)
        {
            Debug.LogErrorFormat("OnAdViewRenderError code: {0} msg: {1}", code, message);
        }
        public void OnAdShow(ExpressAd ad)
        {
            Debug.LogFormat("OnAdShow", ad.index);
        }
        public void OnAdClicked(ExpressAd ad)
        {
        }
        public void OnAdClose(ExpressAd ad)
        {
            if (OnReward != null)
                OnReward();
        }
        public void onAdRemoved(ExpressAd ad)
        {
        }
    }
    class SplashAdListener : AdListenerBase, ISplashAdListener
    {
        public void OnError(int code, string message)
        {
            Debug.LogErrorFormat("SplashAdListener Error! code: {0} msg: {1}", code, message);
        }
        public void OnSplashAdLoad(BUSplashAd ad)
        {
            ad.SetDownloadListener(new AppDownloadListener(this));
            AD.AD = ad;
        }
        public void OnTimeout()
        {
            Debug.Log("加载 SplashAd 超时");
        }
    }
    class RewardVideoAdListener : AdListenerBase, IRewardVideoAdListener, IRewardAdInteractionListener
    {
        void IRewardVideoAdListener.OnError(int code, string message)
        {
            Debug.LogErrorFormat("RewardVideoAdListener Error! code: {0} msg: {1}", code, message);
        }
        void IRewardVideoAdListener.OnRewardVideoAdLoad(RewardVideoAd ad)
        {
            ad.SetRewardAdInteractionListener(this);
            AD.AD = ad;
        }
        void IRewardVideoAdListener.OnRewardVideoCached()
        {
        }
        void IRewardVideoAdListener.OnExpressRewardVideoAdLoad(ExpressRewardVideoAd ad)
        {
        }
        void IRewardVideoAdListener.OnRewardVideoCached(RewardVideoAd ad)
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
        void IRewardAdInteractionListener.OnRewardVerify(bool rewardVerify, int rewardAmount, string rewardName, int rewardType, float rewardPropose)
        {
        }
        void IRewardAdInteractionListener.OnRewardArrived(bool isRewardValid, int rewardType, IRewardBundleModel extraInfo)
        {
            if (isRewardValid && OnReward != null)
                OnReward();
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