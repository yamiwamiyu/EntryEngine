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
    /// The full screen video Ad.
    /// </summary>
    public sealed class FullScreenVideoAd
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
            IFullScreenVideoAdInteractionListener listener)
        {
            var androidListener =
                new FullScreenVideoAdInteractionListener(listener);
            this.ad.Call(
                "setFullScreenVideoAdInteractionListener", androidListener);
        }

        /// <summary>
        /// Sets the listener for the Ad download.
        /// </summary>
        public void SetDownloadListener(IAppDownloadListener listener)
        {
            var androidListener = new AppDownloadListener(listener);
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

#pragma warning disable SA1300
#pragma warning disable IDE1006

        private sealed class FullScreenVideoAdInteractionListener : AndroidJavaProxy
        {
            private readonly IFullScreenVideoAdInteractionListener listener;

            public FullScreenVideoAdInteractionListener(
                IFullScreenVideoAdInteractionListener listener)
                : base("com.bytedance.sdk.openadsdk.TTFullScreenVideoAd$FullScreenVideoAdInteractionListener")
            {
                this.listener = listener;
            }

            public void onAdShow()
            {
                this.listener.OnAdShow();
            }

            public void onAdVideoBarClick()
            {
                this.listener.OnAdVideoBarClick();
            }

            public void onAdClose()
            {
                this.listener.OnAdClose();
            }

            public void onVideoComplete()
            {
                this.listener.OnVideoComplete();
            }

            public void onSkippedVideo()
            {
                this.listener.OnSkippedVideo();
            }
        }

#pragma warning restore SA1300
#pragma warning restore IDE1006
    }
#endif
}
