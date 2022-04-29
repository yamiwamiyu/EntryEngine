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
    /// Set the interaction Ad.
    /// </summary>
    public sealed class InteractionAd
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
            IInteractionAdInteractionListener listener)
        {
            var androidListener = new AdInteractionListener(listener);
            this.ad.Call("setAdInteractionListener", androidListener);
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

            public AdInteractionListener(
                IInteractionAdInteractionListener listener)
                : base("com.bytedance.sdk.openadsdk.TTInteractionAd$AdInteractionListener")
            {
                this.listener = listener;
            }

            public void onAdClicked()
            {
                this.listener.OnAdClicked();
            }

            public void onAdShow()
            {
                this.listener.OnAdShow();
            }

            public void onAdDismiss()
            {
                this.listener.OnAdDismiss();
            }
        }

#pragma warning restore SA1300
#pragma warning restore IDE1006
    }
#endif
}
