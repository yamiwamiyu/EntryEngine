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
    /// The Ad dislike object.
    /// </summary>
    public sealed class AdDislike
    {
        private readonly AndroidJavaObject ad;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdDislike"/> class.
        /// </summary>
        internal AdDislike(AndroidJavaObject ad)
        {
            this.ad = ad;
        }

        /// <summary>
        /// Show the dislike dialog.
        /// </summary>
        public void ShowDislikeDialog()
        {
            this.ad.Call("showDislikeDialog");
        }

        /// <summary>
        /// Set the dislike interaction listener.
        /// </summary>
        public void SetDislikeInteractionCallback(
            IDislikeInteractionListener listener)
        {
            var androidListener = new DislikeInteractionCallback(listener);
            this.ad.Call("setDislikeInteractionCallback", androidListener);
        }
    }
#endif
}
