//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

namespace ByteDance.Union
{
#if !UNITY_EDITOR && UNITY_ANDROID
#pragma warning disable SA1300
#pragma warning disable IDE1006
    using UnityEngine;

    /// <summary>
    /// The android proxy listener for <see cref="IAppDownloadListener"/>.
    /// </summary>
    internal sealed class AppDownloadListener : AndroidJavaProxy
    {
        private readonly IAppDownloadListener listener;
        private bool callbackOnMainThread;
        public AppDownloadListener(
            IAppDownloadListener listener, bool callbackOnMainThread)
            : base("com.bytedance.sdk.openadsdk.TTAppDownloadListener")
        {
            this.listener = listener;
            this.callbackOnMainThread = callbackOnMainThread;
        }

        public void onIdle()
        {
            UnityDispatcher.PostTask(() => this.listener.OnIdle(), callbackOnMainThread);
        }

        public void onDownloadActive(
            long totalBytes, long currBytes, string fileName, string appName)
        {
            UnityDispatcher.PostTask(() => this.listener.OnDownloadActive(
                totalBytes, currBytes, fileName, appName), callbackOnMainThread);
        }

        public void onDownloadPaused(
            long totalBytes, long currBytes, string fileName, string appName)
        {
            UnityDispatcher.PostTask(() => this.listener.OnDownloadPaused(
                totalBytes, currBytes, fileName, appName), callbackOnMainThread);
        }

        public void onDownloadFailed(
            long totalBytes, long currBytes, string fileName, string appName)
        {
            UnityDispatcher.PostTask(() => this.listener.OnDownloadFailed(
                totalBytes, currBytes, fileName, appName), callbackOnMainThread);
        }

        public void onDownloadFinished(
            long totalBytes, string fileName, string appName)
        {
            UnityDispatcher.PostTask(() => this.listener.OnDownloadFinished(
                totalBytes, fileName, appName), callbackOnMainThread);
        }

        public void onInstalled(string fileName, string appName)
        {
            UnityDispatcher.PostTask(
                () => this.listener.OnInstalled(fileName, appName), callbackOnMainThread);
        }
    }

#pragma warning restore SA1300
#pragma warning restore IDE1006
#endif
}
