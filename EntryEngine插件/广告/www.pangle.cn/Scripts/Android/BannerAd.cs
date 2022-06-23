//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

namespace ByteDance.Union
{
#if !UNITY_EDITOR && UNITY_ANDROID
    using UnityEngine;

    /// <summary>
    /// The banner Ad.
    /// </summary>
    public sealed class BannerAd
    {
        private readonly AndroidJavaObject ad;

        /// <summary>
        /// Initializes a new instance of the <see cref="BannerAd"/> class.
        /// </summary>
        internal BannerAd(AndroidJavaObject ad)
        {
            this.ad = ad;
        }

        /// <summary>
        /// Sets the interaction listener for this Ad.
        /// </summary>
        public void SetBannerInteractionListener(
            IBannerAdInteractionListener listener, bool callbackOnMainThread = true)
        {
            var androidListener = new AdInteractionListener(listener, callbackOnMainThread);
            this.ad.Call("setBannerInteractionListener", androidListener);
        }

        /// <summary>
        /// Sets the listener for the Ad download.
        /// </summary>
        public void SetDownloadListener(IAppDownloadListener listener, bool  callbackOnMainThread = true)
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
        /// Sets the show dislike icon.
        /// </summary>
        public void SetShowDislikeIcon(IDislikeInteractionListener listener, bool callbackOnMainThread = true)
        {
            var androidListener = new DislikeInteractionCallback(listener, callbackOnMainThread);
            this.ad.Call("setShowDislikeIcon", androidListener);
        }

        /// <summary>
        /// Gets the dislike dislog.
        /// </summary>
        public AdDislike GetDislikeDialog(IDislikeInteractionListener listener, bool callbackOnMainThread = true)
        {
            var androidListener = new DislikeInteractionCallback(listener,callbackOnMainThread);
            var dislike = this.ad.Call<AndroidJavaObject>(
                "getDislikeDialog", androidListener);
            return new AdDislike(dislike);
        }

        /// <summary>
        /// Sets the slide interval time.
        /// </summary>
        public void SetSlideIntervalTime(int interval)
        {
            this.ad.Call("setSlideIntervalTime", interval);
        }

#pragma warning disable SA1300
#pragma warning disable IDE1006

        private sealed class AdInteractionListener : AndroidJavaProxy
        {
            private readonly IBannerAdInteractionListener listener;
            private bool callbackOnMainThread;

            public AdInteractionListener(
                IBannerAdInteractionListener listener, bool callbackOnMainThread)
                : base("com.bytedance.sdk.openadsdk.TTBannerAd$AdInteractionListener")
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
        }

#pragma warning restore SA1300
#pragma warning restore IDE1006
    }
#endif
}
