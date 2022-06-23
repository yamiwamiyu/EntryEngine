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
        public void LoadFeedAd(AdSlot adSlot, IFeedAdListener listener, bool callbackOnMainThread = true)
        {
            var androidListener = new FeedAdListener(listener, callbackOnMainThread);
            this.adNative.Call("loadFeedAd", adSlot.Handle, androidListener);
        }

        /// <summary>
        /// Load the draw feed Ad asynchronously and notice on listener.
        /// </summary>
        public void LoadDrawFeedAd(AdSlot adSlot, IDrawFeedAdListener listener, bool callbackOnMainThread = true)
        {
            var androidListener = new DrawFeedAdListener(listener, callbackOnMainThread);
            this.adNative.Call(
                "loadDrawFeedAd", adSlot.Handle, androidListener);
        }

        /// <summary>
        /// Load the native Ad asynchronously and notice on listener.
        /// </summary>
        public void LoadNativeAd(AdSlot adSlot, INativeAdListener listener, bool callbackOnMainThread = true)
        {
            var androidListener = new NativeAdListener(listener, callbackOnMainThread);
            this.adNative.Call("loadNativeAd", adSlot.Handle, androidListener);
        }

        /// <summary>
        /// Load the banner Ad asynchronously and notice on listener.
        /// </summary>
        public void LoadBannerAd(AdSlot adSlot, IBannerAdListener listener, bool callbackOnMainThread = true)
        {
            var androidListener = new BannerAdListener(listener, callbackOnMainThread);
            this.adNative.Call("loadBannerAd", adSlot.Handle, androidListener);
        }

        /// <summary>
        /// Load the interaction Ad asynchronously and notice on listener.
        /// </summary>
        public void LoadInteractionAd(
            AdSlot adSlot, IInteractionAdListener listener, bool callbackOnMainThread = true)
        {
            var androidListener = new InteractionAdListener(listener, callbackOnMainThread);
            this.adNative.Call(
                "loadInteractionAd", adSlot.Handle, androidListener);
        }

        /// <summary>
        /// Load the splash Ad asynchronously and notice on listener with
        /// specify timeout.
        /// </summary>
        public void LoadSplashAd(
            AdSlot adSlot, ISplashAdListener listener, int timeOut, bool callbackOnMainThread = true)
        {
            var androidListener = new SplashAdListener(listener, callbackOnMainThread);
            this.adNative.Call(
                "loadSplashAd", adSlot.Handle, androidListener, timeOut);
        }

        /// <summary>
        /// Load the splash Ad asynchronously and notice on listener.
        /// </summary>
        public void LoadSplashAd(AdSlot adSlot, ISplashAdListener listener, bool callbackOnMainThread = true)
        {
            var androidListener = new SplashAdListener(listener, callbackOnMainThread);
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
            AdSlot adSlot, IRewardVideoAdListener listener, bool callbackOnMainThread = true)
        {
            var androidListener = new RewardVideoAdListener(listener, callbackOnMainThread);
            this.adNative.Call(
                "loadRewardVideoAd", adSlot.Handle, androidListener);
        }

        /// <summary>
        /// Load the full screen video Ad asynchronously and notice on listener.
        /// </summary>
        public void LoadFullScreenVideoAd(
            AdSlot adSlot, IFullScreenVideoAdListener listener, bool callbackOnMainThread = true)
        {
            var androidListener = new FullScreenVideoAdListener(listener, callbackOnMainThread);
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
            AdSlot adSlot, IExpressAdListener listener, bool callbackOnMainThread)
        {
            var androidListener = new ExpressAdListener(listener, callbackOnMainThread);
            this.adNative.Call(
                "loadNativeExpressAd",adSlot.Handle, androidListener);
        }

        public void LoadExpressInterstitialAd(
            AdSlot adSlot, IExpressAdListener listener, bool callbackOnMainThread = true)
        {
            var androidListener = new ExpressAdListener(listener, callbackOnMainThread);
            this.adNative.Call(
                "loadInteractionExpressAd",adSlot.Handle, androidListener);
        }

        public void LoadExpressBannerAd(
            AdSlot adSlot, IExpressAdListener listener, bool callbackOnMainThread = true)
        {
            var androidListener = new ExpressAdListener(listener, callbackOnMainThread);
            this.adNative.Call(
                "loadBannerExpressAd",adSlot.Handle, androidListener);
        }
#pragma warning disable SA1300
#pragma warning disable IDE1006
        private sealed class FeedAdListener : AndroidJavaProxy
        {
            private readonly IFeedAdListener listener;
            private bool callbackOnMainThread;
            public FeedAdListener(IFeedAdListener listener, bool callbackOnMainThread)
                : base("com.bytedance.sdk.openadsdk.TTAdNative$FeedAdListener")
            {
                this.listener = listener;
                this.callbackOnMainThread = callbackOnMainThread;
            }

            public void onError(int code, string message)
            {
                UnityDispatcher.PostTask(
                    () => this.listener.OnError(code, message), callbackOnMainThread);
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

                UnityDispatcher.PostTask(
                    () => this.listener.OnFeedAdLoad(ads), callbackOnMainThread);
            }
        }

        private sealed class DrawFeedAdListener : AndroidJavaProxy
        {
            private readonly IDrawFeedAdListener listener;
            private bool callbackOnMainThread;

            public DrawFeedAdListener(IDrawFeedAdListener listener, bool callbackOnMainThread)
                : base("com.bytedance.sdk.openadsdk.TTAdNative$DrawFeedAdListener")
            {
                this.listener = listener;
                this.callbackOnMainThread = callbackOnMainThread;
            }

            public void onError(int code, string message)
            {
                UnityDispatcher.PostTask(
                    () => this.listener.OnError(code, message), callbackOnMainThread);
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

                UnityDispatcher.PostTask(
                    () => this.listener.OnDrawFeedAdLoad(ads), callbackOnMainThread);
            }
        }

        private sealed class NativeAdListener : AndroidJavaProxy
        {
            private readonly INativeAdListener listener;
            private bool callbackOnMainThread;
            public NativeAdListener(INativeAdListener listener,bool callbackOnMainThread)
                : base("com.bytedance.sdk.openadsdk.TTAdNative$NativeAdListener")
            {
                this.listener = listener;
                this.callbackOnMainThread = callbackOnMainThread;
            }

            public void onError(int code, string message)
            {
                UnityDispatcher.PostTask(
                    () => this.listener.OnError(code, message), callbackOnMainThread);
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

                UnityDispatcher.PostTask(
                    () => this.listener.OnNativeAdLoad(list,null), callbackOnMainThread);
            }
        }

        private sealed class BannerAdListener : AndroidJavaProxy
        {
            private readonly IBannerAdListener listener;
            private bool callbackOnMainThread;

            public BannerAdListener(IBannerAdListener listener, bool callbackOnMainThread)
                : base("com.bytedance.sdk.openadsdk.TTAdNative$BannerAdListener")
            {
                this.listener = listener;
                this.callbackOnMainThread = callbackOnMainThread;
            }

            public void onError(int code, string message)
            {
                UnityDispatcher.PostTask(
                    () => this.listener.OnError(code, message), callbackOnMainThread);
            }

            public void onBannerAdLoad(AndroidJavaObject handle)
            {
                var ad = new BannerAd(handle);
                UnityDispatcher.PostTask(
                    () => this.listener.OnBannerAdLoad(ad), callbackOnMainThread);
            }
        }

        private sealed class InteractionAdListener : AndroidJavaProxy
        {
            private readonly IInteractionAdListener listener;
            private bool callbackOnMainThread;

            public InteractionAdListener(IInteractionAdListener listener, bool callbackOnMainThread)
                : base("com.bytedance.sdk.openadsdk.TTAdNative$InteractionAdListener")
            {
                this.listener = listener;
                this.callbackOnMainThread = callbackOnMainThread;
            }

            public void onError(int code, string message)
            {
                UnityDispatcher.PostTask(
                    () => this.listener.OnError(code, message), callbackOnMainThread);
            }

            public void onInteractionAdLoad(AndroidJavaObject handle)
            {
                var ad = new InteractionAd(handle);
                UnityDispatcher.PostTask(
                    () => this.listener.OnInteractionAdLoad(ad), callbackOnMainThread);
            }
        }

        private sealed class SplashAdListener : AndroidJavaProxy
        {
            private readonly ISplashAdListener listener;
            private bool callbackOnMainThread;
            public SplashAdListener(ISplashAdListener listener, bool callbackOnMainThread)
                : base("com.bytedance.sdk.openadsdk.TTAdNative$SplashAdListener")
            {
                this.listener = listener;
                this.callbackOnMainThread = callbackOnMainThread;
            }

            public void onError(int code, string message)
            {
                UnityDispatcher.PostTask(
                    () => this.listener.OnError(code, message), callbackOnMainThread);
            }

            public void onSplashAdLoad(AndroidJavaObject handle)
            {
                var ad = new BUSplashAd(handle);
                UnityDispatcher.PostTask(
                    () => this.listener.OnSplashAdLoad(ad), callbackOnMainThread);
            }
            
            public void onTimeout()
            {
                UnityDispatcher.PostTask(
                    () => this.listener.OnTimeout(), callbackOnMainThread);
            }
        }

        private sealed class RewardVideoAdListener : AndroidJavaProxy
        {
            private readonly IRewardVideoAdListener listener;
            private bool callbackOnMainThread;
            public RewardVideoAdListener(IRewardVideoAdListener listener, bool callbackOnMainThread)
                : base("com.bytedance.sdk.openadsdk.TTAdNative$RewardVideoAdListener")
            {
                this.listener = listener;
                this.callbackOnMainThread = callbackOnMainThread;
            }

            public void onError(int code, string message)
            {
                UnityDispatcher.PostTask(
                    () => this.listener.OnError(code, message),callbackOnMainThread);
            }

            public void onRewardVideoAdLoad(AndroidJavaObject handle)
            {
                var ad = new RewardVideoAd(handle);
                UnityDispatcher.PostTask(
                    () => this.listener.OnRewardVideoAdLoad(ad),callbackOnMainThread);
            }

            public void onRewardVideoCached()
            {
                UnityDispatcher.PostTask(
                    () => this.listener.OnRewardVideoCached(),callbackOnMainThread);
            }
            
            public void onRewardVideoCached(AndroidJavaObject handle)
            {
                var ad = new RewardVideoAd(handle);
                UnityDispatcher.PostTask(
                    () => this.listener.OnRewardVideoCached(ad),callbackOnMainThread);
            } 
        }

        private sealed class FullScreenVideoAdListener : AndroidJavaProxy
        {
            private readonly IFullScreenVideoAdListener listener;
            private bool callbackOnMainThread;

            public FullScreenVideoAdListener(
                IFullScreenVideoAdListener listener, bool callbackOnMainThread)
                : base("com.bytedance.sdk.openadsdk.TTAdNative$FullScreenVideoAdListener")
            {
                this.listener = listener;
                this.callbackOnMainThread = callbackOnMainThread;
            }

            public void onError(int code, string message)
            {
                UnityDispatcher.PostTask(
                    () => this.listener.OnError(code, message), callbackOnMainThread);
            }

            public void onFullScreenVideoAdLoad(AndroidJavaObject handle)
            {
                var ad = new FullScreenVideoAd(handle);
                UnityDispatcher.PostTask(
                    () => this.listener.OnFullScreenVideoAdLoad(ad), callbackOnMainThread);
            }

            public void onFullScreenVideoCached()
            {
                UnityDispatcher.PostTask(
                    () => this.listener.OnFullScreenVideoCached(), callbackOnMainThread);
            }
            
            public void onFullScreenVideoCached(AndroidJavaObject handle)
            {
                var ad = new FullScreenVideoAd(handle);
                UnityDispatcher.PostTask(
                    () => this.listener.OnFullScreenVideoCached(ad), callbackOnMainThread);
            }
            
        }

        private sealed class ExpressAdListener : AndroidJavaProxy {
            private readonly IExpressAdListener callback;
            private bool callbackOnMainThread;

            public ExpressAdListener (IExpressAdListener listener, bool callbackOnMainThread) : base ("com.bytedance.sdk.openadsdk.TTAdNative$NativeExpressAdListener") {
                this.callback = listener;
                this.callbackOnMainThread = callbackOnMainThread;
            }

            void onError (int code, String message) {
                UnityDispatcher.PostTask (
                    () => this.callback.OnError (code, message), callbackOnMainThread);
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
                UnityDispatcher.PostTask (
                    () => this.callback.OnExpressAdLoad (expressAds), callbackOnMainThread);
            }
        }

#pragma warning restore SA1300
#pragma warning restore IDE1006
    }
#endif
}
