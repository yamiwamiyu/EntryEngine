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

    /// <summary>
    /// Set the interaction Ad.
    /// </summary>
    public sealed class InteractionAd : IClintBidding
    {
        private readonly AndroidJavaObject ad;

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionAd"/> class.
        /// </summary>
        internal InteractionAd(AndroidJavaObject ad)
        {
            this.ad = ad;
        }

        /// <summary>
        /// Sets the interaction listener for this Ad.
        /// </summary>
        public void SetAdInteractionListener(
            IInteractionAdInteractionListener listener, bool callbackOnMainThread = true)
        {
            var androidListener = new AdInteractionListener(listener, callbackOnMainThread);
            this.ad.Call("setAdInteractionListener", androidListener);
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
        /// Show this interaction Ad.
        /// </summary>
        public void ShowInteractionAd()
        {
            var activity = SDK.GetActivity();
            var runnable = new AndroidJavaRunnable(
                () => this.ad.Call("showInteractionAd", activity));
            activity.Call("runOnUiThread", runnable);
        }

#pragma warning disable SA1300
#pragma warning disable IDE1006

        private sealed class AdInteractionListener : AndroidJavaProxy
        {
            private readonly IInteractionAdInteractionListener listener;
            private bool callbackOnMainThread;
            public AdInteractionListener(
                IInteractionAdInteractionListener listener, bool callbackOnMainThread)
                : base("com.bytedance.sdk.openadsdk.TTInteractionAd$AdInteractionListener")
            {
                this.listener = listener;
                this.callbackOnMainThread = callbackOnMainThread;
            }

            public void onAdClicked()
            {
                UnityDispatcher.PostTask(() => this.listener.OnAdClicked(), callbackOnMainThread);
            }

            public void onAdShow()
            {
                UnityDispatcher.PostTask(() => this.listener.OnAdShow(), callbackOnMainThread);
            }

            public void onAdDismiss()
            {
                UnityDispatcher.PostTask(() => this.listener.OnAdDismiss(), callbackOnMainThread);
            }
        }
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
#pragma warning restore SA1300
#pragma warning restore IDE1006
    }
#endif
}
