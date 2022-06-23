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
    /// The feed Ad.
    /// </summary>
    public class FeedAd : NativeAd
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedAd"/> class.
        /// </summary>
        internal FeedAd(AndroidJavaObject ad)
            : base(ad)
        {
            this.Handle = ad;
        }

        /// <summary>
        /// Gets the java object.
        /// </summary>
        internal AndroidJavaObject Handle;

        /// <summary>
        /// Set the video Ad listener.
        /// </summary>
        public void SetVideoAdListener(IVideoAdListener listener, bool callbackOnMainThread = true)
        {
            var androidListener = new VideoAdListener(this, listener, callbackOnMainThread);
            this.ad.Call("setVideoAdListener", androidListener);
        }

#pragma warning disable SA1300
#pragma warning disable IDE1006

        private sealed class VideoAdListener : AndroidJavaProxy
        {
            private readonly FeedAd ad;
            private readonly IVideoAdListener listener;
            private bool callbackOnMainThread;
            public VideoAdListener(
                FeedAd ad,
                IVideoAdListener listener, bool callbackOnMainThread)
                : base("com.bytedance.sdk.openadsdk.TTNativeAd$VideoAdListener")
            {
                this.ad = ad;
                this.listener = listener;
                this.callbackOnMainThread = callbackOnMainThread;
            }

            public void onVideoLoad(AndroidJavaObject ad)
            {
                var feedAd = (ad == this.ad.Handle) ? this.ad : new FeedAd(ad);
                UnityDispatcher.PostTask(
                    () => this.listener.OnVideoLoad(feedAd), callbackOnMainThread);
            }

            public void onVideoError(int var1, int var2)
            {
                UnityDispatcher.PostTask(
                    () => this.listener.OnVideoError(var1, var2), callbackOnMainThread);
            }

            public void onVideoAdStartPlay(AndroidJavaObject ad)
            {
                var feedAd = (ad == this.ad.Handle) ? this.ad : new FeedAd(ad);
                UnityDispatcher.PostTask(
                    () => this.listener.OnVideoAdStartPlay(feedAd), callbackOnMainThread);
            }

            public void onVideoAdPaused(AndroidJavaObject ad)
            {
                var feedAd = (ad == this.ad.Handle) ? this.ad : new FeedAd(ad);
                UnityDispatcher.PostTask(
                    () => this.listener.OnVideoAdPaused(feedAd), callbackOnMainThread);
            }

            public void onVideoAdContinuePlay(AndroidJavaObject ad)
            {
                var feedAd = (ad == this.ad.Handle) ? this.ad : new FeedAd(ad);
                UnityDispatcher.PostTask(
                    () => this.listener.OnVideoAdContinuePlay(feedAd), callbackOnMainThread);
            }
        }

#pragma warning restore SA1300
#pragma warning restore IDE1006
        }
#endif
}
