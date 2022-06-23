//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

using System;

namespace ByteDance.Union
{
    /// <summary>
    /// The listener for reward Ad interaction.
    /// </summary>
    public interface IRewardAdInteractionListener
    {
        /// <summary>
        /// Invoke when the Ad is shown.
        /// </summary>
        void OnAdShow();

        /// <summary>
        /// Invoke when the Ad video bar is clicked.
        /// </summary>
        void OnAdVideoBarClick();

        /// <summary>
        /// Invoke when the Ad is closed.
        /// </summary>
        void OnAdClose();

        /// <summary>
        /// Invoke when the video complete.
        /// </summary>
        void OnVideoComplete();

        /// <summary>
        /// Invoke when the video click skip.
        /// </summary>
        void OnVideoSkip();

        /// <summary>
        /// Invoke when the video has an error.
        /// </summary>
        void OnVideoError();

        /// <summary>
        /// Invoke when the reward is verified.
        /// </summary>
#if UNITY_ANDROID
        [Obsolete("use OnRewardArrived(bool isRewardValid, int rewardType, IRewardBundleModel extraInfo)")]
#endif
        void OnRewardVerify(
            bool rewardVerify, int rewardAmount, string rewardName, int rewardType = -1, float rewardPropose = -1f);

        void OnRewardArrived(bool isRewardValid, int rewardType, IRewardBundleModel extraInfo);
    }
}
