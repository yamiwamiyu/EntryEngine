//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

namespace ByteDance.Union
{
#if !UNITY_EDITOR && UNITY_IOS
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The advertisement native object for iOS.
    /// </summary>
    public sealed class AdNative
    {
        /// <summary>
        /// Load the feed Ad asynchronously and notice on listener.
        /// </summary>
        public void LoadFeedAd(AdSlot adSlot, IFeedAdListener listener)
        {
            listener.OnError(0, "Not Support on this platform");
        }

        /// <summary>
        /// Load the draw feed Ad asynchronously and notice on listener.
        /// </summary>
        public void LoadDrawFeedAd(AdSlot adSlot, IDrawFeedAdListener listener)
        {
            listener.OnError(0, "Not Support on this platform");
        }

        /// <summary>
        /// Load the native Ad asynchronously and notice on listener.
        /// </summary>
        public void LoadNativeAd(AdSlot adSlot, INativeAdListener listener, bool callbackOnMainThead = true)
        {
            NativeAd.LoadNativeAd(adSlot,listener, callbackOnMainThead);
        }

        /// <summary>
        /// Load the banner Ad asynchronously and notice on listener.
        /// </summary>
        public void LoadBannerAd(AdSlot adSlot, IBannerAdListener listener)
        {
            listener.OnError(0, "Not Support on this platform");
        }

        /// <summary>
        /// Load the interaction Ad asynchronously and notice on listener.
        /// </summary>
        public void LoadInteractionAd(
            AdSlot adSlot, IInteractionAdListener listener)
        {
            listener.OnError(0, "Not Support on this platform");
        }

        /// <summary>
        /// Load the splash Ad asynchronously and notice on listener with
        /// specify timeout.
        /// </summary>
        public BUSplashAd LoadSplashAd_iOS(
            AdSlot adSlot, ISplashAdListener listener, int timeOut, bool callbackOnMainThead = true)
        {
            return BUSplashAd.LoadSplashAd(adSlot, listener, timeOut, callbackOnMainThead);
        }

        /// <summary>
        /// Load the splash Ad asynchronously and notice on listener.
        /// </summary>
        public BUSplashAd LoadSplashAd_iOS(AdSlot adSlot, ISplashAdListener listener, bool callbackOnMainThead = true)
        {
            return BUSplashAd.LoadSplashAd(adSlot, listener, -1, callbackOnMainThead);
        }

        /// <summary>
        /// Load the splash Ad asynchronously and notice on listener with
        /// specify timeout.
        /// </summary>
        public BUExpressSplashAd LoadExpressSplashAd_iOS(
            AdSlot adSlot, ISplashAdListener listener, int timeOut, bool callbackOnMainThread = true)
        {
            return BUExpressSplashAd.LoadSplashAd(adSlot, listener, timeOut, callbackOnMainThread);
        }

        /// <summary>
        /// Load the splash Ad asynchronously and notice on listener.
        /// </summary>
        public BUExpressSplashAd LoadExpressSplashAd_iOS(AdSlot adSlot, ISplashAdListener listener, bool callbackOnMainThread = true)
        {
            return BUExpressSplashAd.LoadSplashAd(adSlot, listener, -1, callbackOnMainThread);
        }

        /// <summary>
        /// Load the reward video Ad asynchronously and notice on listener.
        /// </summary>
        public void LoadRewardVideoAd(
            AdSlot adSlot, IRewardVideoAdListener listener, bool callbackOnMainThead =true)
        {
            RewardVideoAd.LoadRewardVideoAd(adSlot, listener, callbackOnMainThead);
        }

        /// <summary>
        /// Load the full screen video Ad asynchronously and notice on listener.
        /// </summary>
        public void LoadFullScreenVideoAd(
            AdSlot adSlot, IFullScreenVideoAdListener listener, bool callbackOnMainThead = true)
        {
            FullScreenVideoAd.LoadFullScreenVideoAd(adSlot, listener, callbackOnMainThead);
        }
    
        public void LoadExpressRewardAd(
            AdSlot adSlot, IRewardVideoAdListener listener, bool callbackOnMainThead = true)
        {
            ExpressRewardVideoAd.LoadRewardVideoAd(adSlot, listener, callbackOnMainThead);
        }

        public void LoadExpressFullScreenVideoAd(
           AdSlot adSlot, IFullScreenVideoAdListener listener, bool callbackOnMainThead = true)
        {
            ExpressFullScreenVideoAd.LoadFullScreenVideoAd(adSlot, listener, callbackOnMainThead);
        }

        public void LoadNativeExpressAd(
            AdSlot adSlot, IExpressAdListener listener, bool callbackOnMainThead = true)
        {
            ExpressAd.LoadExpressAdAd(adSlot, listener, callbackOnMainThead);
        }

        public void LoadExpressInterstitialAd(
            AdSlot adSlot, IExpressAdListener listener, bool callbackOnMainThead = true)
        {
            ExpressInterstitialAd.LoadExpressAd(adSlot, listener, callbackOnMainThead);
        }

        public void LoadExpressBannerAd(
            AdSlot adSlot, IExpressAdListener listener, bool callbackOnMainThead = true)
        {
            ExpressBannerAd.LoadExpressAd(adSlot, listener, callbackOnMainThead);
        }
    }
#endif
}
