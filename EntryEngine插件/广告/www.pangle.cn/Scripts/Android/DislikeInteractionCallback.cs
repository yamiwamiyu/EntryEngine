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
    /// The android proxy listener for <see cref=
    /// "IDislikeInteractionListener"/>.
    /// </summary>
    internal sealed class DislikeInteractionCallback : AndroidJavaProxy
    {
        private readonly IDislikeInteractionListener listener;
        private bool callbackOnMainThread;
        public DislikeInteractionCallback(
            IDislikeInteractionListener listener, bool callbackOnMainThread)
            : base("com.bytedance.sdk.openadsdk.TTAdDislike$DislikeInteractionCallback")
        {
            this.listener = listener;
            this.callbackOnMainThread = callbackOnMainThread;
        }

        public void onSelected(int var1, string var2, bool enforce)
        {
            UnityDispatcher.PostTask(
                () => this.listener.OnSelected(var1, var2, enforce), callbackOnMainThread);
        }

        public void onCancel()
        {
            UnityDispatcher.PostTask(
                () => this.listener.OnCancel(), callbackOnMainThread);
        }

        public void onShow()
        {
            UnityDispatcher.PostTask(
                () => this.listener.OnShow(), callbackOnMainThread);
        }

    }

#pragma warning restore SA1300
#pragma warning restore IDE1006
#endif
}
