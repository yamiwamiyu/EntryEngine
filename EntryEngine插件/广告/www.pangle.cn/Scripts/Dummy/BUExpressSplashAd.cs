//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

using UnityEngine;

namespace ByteDance.Union
{
#if UNITY_EDITOR || (!UNITY_ANDROID && !UNITY_IOS)
    /// <summary>
    /// The banner Ad.
    /// </summary>
    public class BUExpressSplashAd : BUSplashAd
    {
        /// <summary>
        /// Sets the interaction listener for this Ad.
        /// </summary>
        public void SetSplashInteractionListener(
            ISplashAdInteractionListener listener)
        {
        }

        /// <summary>
        /// Sets the listener for the Ad download.
        /// </summary>
        public void SetDownloadListener(IAppDownloadListener listener)
        {
        }

        public void Dispose()
        {
        }

        internal BUExpressSplashAd(AndroidJavaObject ad) : base(ad)
        {
        }
    }
#endif
}
