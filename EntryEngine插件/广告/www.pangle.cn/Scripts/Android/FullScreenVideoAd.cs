//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

namespace ByteDance.Union
{
#if (DEV || !UNITY_EDITOR) && UNITY_ANDROID
    using UnityEngine;
    using System.Collections.Generic;
    /// <summary>
    /// The full screen video Ad.
    /// </summary>
    public sealed class FullScreenVideoAd : IClintBidding
    {
        private readonly AndroidJavaObject ad;
        /// <summary>
        /// Initializes a new instance of the <see cref="FullScreenVideoAd"/>
        /// class.
        /// </summary>
        internal FullScreenVideoAd(AndroidJavaObject ad)
        {
            this.ad = ad;
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Sets the interaction listener for this Ad.
        /// </summary>
        public void SetFullScreenVideoAdInteractionListener(
            IFullScreenVideoAdInteractionListener listener, bool callbackOnMainThread)
        {
            var androidListener =
                new FullScreenVideoAdInteractionListener(listener, callbackOnMainThread);
            this.ad.Call(
                "setFullScreenVideoAdInteractionListener", androidListener);
        }

        /// <summary>
        /// Sets the listener for the Ad download.
        /// </summary>
        public void SetDownloadListener(IAppDownloadListener listener, bool callbackOnMainThread = true)
        {
            var androidListener = new AppDownloadListener(listener, callbackOnMainThread);
            this.ad.Call("setDownloadListener", androidListener);
        }

        /// <summary>
        /// Gets the interaction type.
        /// </summary>
        public int GetInteractionType()
        {
            return this.ad.Call<int>("getInteractionType");
        }

        /// <summary>
        /// Show the full screen video.
        /// </summary>
        public void ShowFullScreenVideoAd()
        {
            var activity = SDK.GetActivity();
            var runnable = new AndroidJavaRunnable(
                () => this.ad.Call("showFullScreenVideoAd", activity));
            activity.Call("runOnUiThread", runnable);
        }

        /// <summary>
        /// Set to show the download bar.
        /// </summary>
        public void SetShowDownLoadBar(bool show)
        {
            this.ad.Call("setShowDownLoadBar", show);
        }
        /// <summary>
        /// get media extra info dictionary,all value is string type,some need developer cast to real type manually
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetMediaExtraInfo()
        {
            var map= this.ad.Call<AndroidJavaObject>("getMediaExtraInfo");
            var result = new Dictionary<string, string>();
            var entries = map.Call<AndroidJavaObject>("entrySet").Call<AndroidJavaObject>("iterator");

            while (entries.Call<bool>("hasNext"))
            {
                var entry = entries.Call<AndroidJavaObject>("next");
                var key = entry.Call<AndroidJavaObject>("getKey").Call<string>("toString");
                var value = entry.Call<AndroidJavaObject>("getValue").Call<string>("toString");
                result.Add(key,value);
            }
            return result;
        }

#pragma warning disable SA1300
#pragma warning disable IDE1006

        private sealed class FullScreenVideoAdInteractionListener : AndroidJavaProxy
        {
            private readonly IFullScreenVideoAdInteractionListener listener;
            private bool callbackOnMainThread;

            public FullScreenVideoAdInteractionListener(
                IFullScreenVideoAdInteractionListener listener, bool callbackOnMainThread)
                : base("com.bytedance.sdk.openadsdk.TTFullScreenVideoAd$FullScreenVideoAdInteractionListener")
            {
                this.listener = listener;
                this.callbackOnMainThread = callbackOnMainThread;
            }

            public void onAdShow()
            {
               UnityDispatcher.PostTask(()=>this.listener.OnAdShow(), callbackOnMainThread);
            }

            public void onAdVideoBarClick()
            {
                UnityDispatcher.PostTask(()=>this.listener.OnAdVideoBarClick(), callbackOnMainThread);
            }

            public void onAdClose()
            {
                UnityDispatcher.PostTask(()=>this.listener.OnAdClose(), callbackOnMainThread);
            }

            public void onVideoComplete()
            {
                UnityDispatcher.PostTask(()=>this.listener.OnVideoComplete(), callbackOnMainThread);
            }

            public void onSkippedVideo()
            {
                UnityDispatcher.PostTask(()=>this.listener.OnSkippedVideo(), callbackOnMainThread);
            }
        }

#pragma warning restore SA1300
#pragma warning restore IDE1006
        public void Win(double auctionBidToWin)
        {
            ClientBiddingUtils.Win(ad, auctionBidToWin);
        }

        public void Loss(double auctionPrice = double.NaN, string lossReason = null, string winBidder = null)
        {
            ClientBiddingUtils.Loss(ad, auctionPrice, lossReason, winBidder);
        }

        public void SetPrice(double auctionPrice = double.NaN)
        {
            ClientBiddingUtils.SetPrice(ad, auctionPrice);
        }
    }
#endif
}
