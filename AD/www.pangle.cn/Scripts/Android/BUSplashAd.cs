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
    /// The splash Ad.
    /// </summary>
    public class BUSplashAd
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
            ISplashAdInteractionListener listener)
        {
            var androidListener = new AdInteractionListener(listener);
            this.ad.Call("setSplashInteractionListener", androidListener);
        }

        /// <summary>
        /// Sets the download listener.
        /// </summary>
        public void SetDownloadListener(IAppDownloadListener listener)
        {
            var androidListener = new AppDownloadListener(listener);
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

            public AdInteractionListener(
                ISplashAdInteractionListener listener)
                : base("com.bytedance.sdk.openadsdk.TTSplashAd$AdInteractionListener")
            {
                this.listener = listener;
            }

            public void onAdClicked(AndroidJavaObject view, int var2)
            {
                this.listener.OnAdClicked(var2);
            }

            public void onAdShow(AndroidJavaObject view, int var2)
            {
                this.listener.OnAdShow(var2);
            }

            public void onAdSkip()
            {
                this.listener.OnAdSkip();
            }

            public void onAdTimeOver()
            {
                this.listener.OnAdTimeOver();
            }
        }

#pragma warning restore SA1300
#pragma warning restore IDE1006
    }
#endif
}
