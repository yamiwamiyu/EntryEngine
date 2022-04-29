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
    /// The draw feed Ad.
    /// </summary>
    public sealed class DrawFeedAd : FeedAd
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawFeedAd"/> class.
        /// </summary>
        internal DrawFeedAd(AndroidJavaObject ad)
            : base(ad)
        {
        }

        /// <summary>
        /// Support whether this draw feed can interrupt video during play.
        /// </summary>
        public void SetCanInterruptVideoPlay(bool support)
        {
            this.ad.Call("setCanInterruptVideoPlay", support);
        }
    }
#endif
}
