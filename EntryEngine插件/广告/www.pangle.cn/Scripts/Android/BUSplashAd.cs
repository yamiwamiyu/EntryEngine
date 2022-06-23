//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

using System;

namespace ByteDance.Union
{
#if (DEV || !UNITY_EDITOR) && UNITY_ANDROID
    using UnityEngine;

    /// <summary>
    /// The splash Ad.
    /// </summary>
    public class BUSplashAd : IClintBidding
    {
        private readonly AndroidJavaObject ad;

        /// <summary>
        /// Initializes a new instance of the <see cref="SplashAd"/> class.
        /// </summary>
        internal BUSplashAd(AndroidJavaObject ad)
        {
            this.ad = ad;
        }

        /// <summary>
        /// Gets the interaction type.
        /// </summary>
        public int GetInteractionType()
        {
            return this.ad.Call<int>("getInteractionType");
        }

        /// <summary>
        /// Get current SpalshAd
        /// </summary>
        public AndroidJavaObject getCurrentSplshAd()
        {
            return this.ad;
        }

        /// <summary>
        /// Sets the interaction listener for this Ad.
        /// </summary>
        public void SetSplashInteractionListener(
            ISplashAdInteractionListener listener, bool callbackOnMainThread = true)
        {
            var androidListener = new AdInteractionListener(listener, callbackOnMainThread);
            this.ad.Call("setSplashInteractionListener", androidListener);
        }

        /// <summary>
        /// Sets the download listener.
        /// </summary>
        public void SetDownloadListener(IAppDownloadListener listener, bool callbackOnMainThread = true)
        {
            var androidListener = new AppDownloadListener(listener, callbackOnMainThread);
            this.ad.Call("setDownloadListener", androidListener);
        }

        /// <summary>
        /// Set this Ad not allow sdk count down.
        /// </summary>
        public void SetNotAllowSdkCountdown()
        {
            this.ad.Call("setNotAllowSdkCountdown");
        }

        public void Dispose()
        {
        }

#pragma warning disable SA1300
#pragma warning disable IDE1006

        private sealed class AdInteractionListener : AndroidJavaProxy
        {
            private readonly ISplashAdInteractionListener listener;
            private bool callbackOnMainThread;
            public AdInteractionListener(
                ISplashAdInteractionListener listener, bool callbackOnMainThread)
                : base("com.bytedance.sdk.openadsdk.TTSplashAd$AdInteractionListener")
            {
                this.listener = listener;
                this.callbackOnMainThread = callbackOnMainThread;
            }

            public void onAdClicked(AndroidJavaObject view, int var2)
            {
                UnityDispatcher.PostTask(
                    () => this.listener.OnAdClicked(var2), callbackOnMainThread);
            }

            public void onAdShow(AndroidJavaObject view, int var2)
            {
                UnityDispatcher.PostTask(
                    () => this.listener.OnAdShow(var2), callbackOnMainThread);
            }

            public void onAdSkip()
            {
                UnityDispatcher.PostTask(() => this.listener.OnAdSkip(), callbackOnMainThread);
            }

            public void onAdTimeOver()
            {
                UnityDispatcher.PostTask(() => this.listener.OnAdTimeOver(), callbackOnMainThread);
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