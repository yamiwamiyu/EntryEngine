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

        public DislikeInteractionCallback(
            IDislikeInteractionListener listener)
            : base("com.bytedance.sdk.openadsdk.TTAdDislike$DislikeInteractionCallback")
        {
            this.listener = listener;
        }

        public void onSelected(int var1, string var2, bool enforce)
        {
            this.listener.OnSelected(var1, var2, enforce);
        }

        public void onCancel()
        {
            this.listener.OnCancel();
        }

        public void onShow()
        {
            this.listener.OnShow();
        }

    }

#pragma warning restore SA1300
#pragma warning restore IDE1006
#endif
}
