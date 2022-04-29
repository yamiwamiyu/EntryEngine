//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

namespace ByteDance.Union
{
#if !UNITY_EDITOR && UNITY_ANDROID
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using EntryEngine;

    /// <summary>
    /// The advertisement native object for android.
    /// </summary>
    public sealed class AdNative
    {
        private readonly AndroidJavaObject adNative;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdNative"/> class.
        /// </summary>
        internal AdNative(AndroidJavaObject adNative)
        {
            this.adNative = adNative;
        }

        /// <summary>
        /// Load the feed Ad asynchronously and notice on listener.
        /// </summary>
        public void LoadFeedAd(AdSlot adSlot, IFeedAdListener listener)
        {
            var androidListener = new FeedAdListener(listener);
            this.adNative.Call("loadFeedAd", adSlot.Handle, androidListener);
        }

        /// <summary>
        /// Load the draw feed Ad asynchronously and notice on listener.
        /// </summary>
        public void LoadDrawFeedAd(AdSlot adSlot, IDrawFeedAdListener listener)
        {
            var androidListener = new DrawFeedAdListener(listener);
            this.adNative.Call(
                "loadDrawFeedAd", adSlot.Handle, androidListener);
        }

        /// <summary>
        /// Load the native Ad asynchronously and notice on listener.
        /// </summary>
        public void LoadNativeAd(AdSlot adSlot, INativeAdListener listener)
        {
            var androidListener = new NativeAdListener(listener);
            this.adNative.Call("loadNativeAd", adSlot.Handle, androidListener);
        }

        /// <summary>
        /// Load the banner Ad asynchronously and notice on listener.
        /// </summary>
        public void LoadBannerAd(AdSlot adSlot, IBannerAdListener listener)
        {
            var androidListener = new BannerAdListener(listener);
            this.adNative.Call("loadBannerAd", adSlot.Handle, androidListener);
        }

        /// <summary>
        /// Load the interaction Ad asynchronously and notice on listener.
        /// </summary>
        public void LoadInteractionAd(
            AdSlot adSlot, IInteractionAdListener listener)
        {
            var androidListener = new InteractionAdListener(listener);
            this.adNative.Call(
                "loadInteractionAd", adSlot.Handle, androidListener);
        }

        /// <summary>
        /// Load the splash Ad asynchronously and notice on listener with
        /// specify timeout.
        /// </summary>
        public void LoadSplashAd(
            AdSlot adSlot, ISplashAdListener listener, int timeOut)
        {
            var androidListener = new SplashAdListener(listener);
            this.adNative.Call(
                "loadSplashAd", adSlot.Handle, androidListener, timeOut);
        }

        /// <summary>
        /// Load the splash Ad asynchronously and notice on listener.
        /// </summary>
        public void LoadSplashAd(AdSlot adSlot, ISplashAdListener listener)
        {
            var androidListener = new SplashAdListener(listener);
            this.adNative.Call("loadSplashAd", adSlot.Handle, androidListener);
        }

        /// <summary>
        /// Load the splash Ad asynchronously and notice on listener with
        /// specify timeout.
        /// </summary>
        public void LoadExpressSplashAd(
            AdSlot adSlot, ISplashAdListener listener, int timeOut)
        {

        }


        /// <summary>
        /// Load the splash Ad asynchronously and notice on listener.
        /// </summary>
        public void LoadExpressSplashAd(AdSlot adSlot, ISplashAdListener listener)
        {

        }

        /// <summary>
        /// Load the reward video Ad asynchronously and notice on listener.
        /// </summary>
        public void LoadRewardVideoAd(
            AdSlot adSlot, IRewardVideoAdListener listener)
        {
            var androidListener = new RewardVideoAdListener(listener);
            this.adNative.Call(
                "loadRewardVideoAd", adSlot.Handle, androidListener);
        }

        /// <summary>
        /// Load the full screen video Ad asynchronously and notice on listener.
        /// </summary>
        public void LoadFullScreenVideoAd(
            AdSlot adSlot, IFullScreenVideoAdListener listener)
        {
            var androidListener = new FullScreenVideoAdListener(listener);
            this.adNative.Call(
                "loadFullScreenVideoAd", adSlot.Handle, androidListener);
        }

        public void LoadExpressRewardAd(
            AdSlot adSlot, IRewardVideoAdListener listener)
        {

        }

        public void LoadExpressFullScreenVideoAd(
           AdSlot adSlot, IFullScreenVideoAdListener listener)
        {

        }

        public void LoadNativeExpressAd(
            AdSlot adSlot, IExpressAdListener listener)
        {
            var androidListener = new ExpressAdListener(listener);
            this.adNative.Call(
                "loadNativeExpressAd",adSlot.Handle, androidListener);
        }

        public void LoadExpressInterstitialAd(
            AdSlot adSlot, IExpressAdListener listener)
        {
            var androidListener = new ExpressAdListener(listener);
            this.adNative.Call(
                "loadInteractionExpressAd",adSlot.Handle, androidListener);
        }

        public void LoadExpressBannerAd(
            AdSlot adSlot, IExpressAdListener listener)
        {
            var androidListener = new ExpressAdListener(listener);
            this.adNative.Call(
                "loadBannerExpressAd",adSlot.Handle, androidListener);
        }
#pragma warning disable SA1300
#pragma warning disable IDE1006
        private sealed class FeedAdListener : AndroidJavaProxy
        {
            private readonly IFeedAdListener listener;

            public FeedAdListener(IFeedAdListener listener)
                : base("com.bytedance.sdk.openadsdk.TTAdNative$FeedAdListener")
            {
                this.listener = listener;
            }

            public void onError(int code, string message)
            {
                this.listener.OnError(code, message);
            }

            public void onFeedAdLoad(AndroidJavaObject list)
            {
                var size = list.Call<int>("size");
                var ads = new FeedAd[size];
                for (int i = 0; i < size; ++i)
                {
                    ads[i] = new FeedAd(
                        list.Call<AndroidJavaObject>("get", i));
                }

                this.listener.OnFeedAdLoad(ads);
            }
        }

        private sealed class DrawFeedAdListener : AndroidJavaProxy
        {
            private readonly IDrawFeedAdListener listener;

            public DrawFeedAdListener(IDrawFeedAdListener listener)
                : base("com.bytedance.sdk.openadsdk.TTAdNative$DrawFeedAdListener")
            {
                this.listener = listener;
            }

            public void onError(int code, string message)
            {
                this.listener.OnError(code, message);
            }

            public void onDrawFeedAdLoad(AndroidJavaObject list)
            {
                var size = list.Call<int>("size");
                var ads = new DrawFeedAd[size];
                for (int i = 0; i < size; ++i)
                {
                    ads[i] = new DrawFeedAd(
                        list.Call<AndroidJavaObject>("get", i));
                }

                this.listener.OnDrawFeedAdLoad(ads);
            }
        }

        private sealed class NativeAdListener : AndroidJavaProxy
        {
            private readonly INativeAdListener listener;

            public NativeAdListener(INativeAdListener listener)
                : base("com.bytedance.sdk.openadsdk.TTAdNative$NativeAdListener")
            {
                this.listener = listener;
            }

            public void onError(int code, string message)
            {
                this.listener.OnError(code, message);
            }

            public void onNativeAdLoad(AndroidJavaObject list)
            {
                var size = list.Call<int>("size");
                var ads = new NativeAd[size];
                for (int i = 0; i < size; ++i)
                {
                    ads[i] = new NativeAd(
                        list.Call<AndroidJavaObject>("get", i));
                }

                this.listener.OnNativeAdLoad(list,null);
            }
        }

        private sealed class BannerAdListener : AndroidJavaProxy
        {
            private readonly IBannerAdListener listener;

            public BannerAdListener(IBannerAdListener listener)
                : base("com.bytedance.sdk.openadsdk.TTAdNative$BannerAdListener")
            {
                this.listener = listener;
            }

            public void onError(int code, string message)
            {
                this.listener.OnError(code, message);
            }

            public void onBannerAdLoad(AndroidJavaObject handle)
            {
                var ad = new BannerAd(handle);
                this.listener.OnBannerAdLoad(ad);
            }
        }

        private sealed class InteractionAdListener : AndroidJavaProxy
        {
            private readonly IInteractionAdListener listener;

            public InteractionAdListener(IInteractionAdListener listener)
                : base("com.bytedance.sdk.openadsdk.TTAdNative$InteractionAdListener")
            {
                this.listener = listener;
            }

            public void onError(int code, string message)
            {
                this.listener.OnError(code, message);
            }

            public void onInteractionAdLoad(AndroidJavaObject handle)
            {
                var ad = new InteractionAd(handle);
                this.listener.OnInteractionAdLoad(ad);
            }
        }

        private sealed class SplashAdListener : AndroidJavaProxy
        {
            private readonly ISplashAdListener listener;

            public SplashAdListener(ISplashAdListener listener)
                : base("com.bytedance.sdk.openadsdk.TTAdNative$SplashAdListener")
            {
                this.listener = listener;
            }

            public void onError(int code, string message)
            {
                this.listener.OnError(code, message);
            }

            public void onSplashAdLoad(AndroidJavaObject handle)
            {
                var ad = new BUSplashAd(handle);
                this.listener.OnSplashAdLoad(ad);
            }
            
            public void onTimeout()
            {
                this.listener.OnTimeout();
            }
        }

        private sealed class RewardVideoAdListener : AndroidJavaProxy
        {
            private readonly IRewardVideoAdListener listener;

            public RewardVideoAdListener(IRewardVideoAdListener listener)
                : base("com.bytedance.sdk.openadsdk.TTAdNative$RewardVideoAdListener")
            {
                this.listener = listener;
            }

            public void onError(int code, string message)
            {
                this.listener.OnError(code, message);
            }

            public void onRewardVideoAdLoad(AndroidJavaObject handle)
            {
                var ad = new RewardVideoAd(handle);
                this.listener.OnRewardVideoAdLoad(ad);
            }

            public void onRewardVideoCached()
            {
                this.listener.OnRewardVideoCached();
            }
        }

        private sealed class FullScreenVideoAdListener : AndroidJavaProxy
        {
            private readonly IFullScreenVideoAdListener listener;

            public FullScreenVideoAdListener(
                IFullScreenVideoAdListener listener)
                : base("com.bytedance.sdk.openadsdk.TTAdNative$FullScreenVideoAdListener")
            {
                this.listener = listener;
            }

            public void onError(int code, string message)
            {
                this.listener.OnError(code, message);
            }

            public void onFullScreenVideoAdLoad(AndroidJavaObject handle)
            {
                var ad = new FullScreenVideoAd(handle);
                this.listener.OnFullScreenVideoAdLoad(ad);
            }

            public void onFullScreenVideoCached()
            {
                this.listener.OnFullScreenVideoCached();
            }
        }

        private sealed class ExpressAdListener : AndroidJavaProxy {
            private readonly IExpressAdListener callback;

            public ExpressAdListener (IExpressAdListener listener) : base ("com.bytedance.sdk.openadsdk.TTAdNative$NativeExpressAdListener") {
                this.callback = listener;

            }

            void onError (int code, String message) {
                 this.callback.OnError (code, message);
            }

            void onNativeExpressAdLoad (AndroidJavaObject ads) {

                var size = ads.Call<int>("size");
                List<ExpressAd> expressAds = new List<ExpressAd>();
                for (int i = 0; i < size; ++i)
                {
                    ExpressAd ad = new ExpressAd(
                        ads.Call<AndroidJavaObject>("get", i));
                    expressAds.Insert(i, ad);
                }
                this.callback.OnExpressAdLoad (expressAds);
            }
        }

#pragma warning restore SA1300
#pragma warning restore IDE1006
    }
#endif
}
