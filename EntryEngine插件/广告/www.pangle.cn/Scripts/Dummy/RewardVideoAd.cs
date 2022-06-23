//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace ByteDance.Union
{
#if UNITY_EDITOR || (!UNITY_ANDROID && !UNITY_IOS)
    using System;
    using UnityEngine;

    /// <summary>
    /// The reward video Ad.
    /// </summary>
    public sealed class RewardVideoAd : IDisposable,IClientBidding
    {
        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <summary>
        /// Sets the interaction listener for this Ad.
        /// </summary>
        public void SetRewardAdInteractionListener(
            IRewardAdInteractionListener listener, bool callbackOnMainThread = true)
        {
        }

        /// <summary>
        /// Sets the interaction listener for this Ad.
        /// </summary>
        public void SetAgainRewardAdInteractionListener(
            IRewardAdInteractionListener againListener, bool callbackOnMainThread = true)
        {
        }

        /// <summary>
        /// Sets the download listener.
        /// </summary>
        public void SetDownloadListener(IAppDownloadListener listener, bool callbackOnMainThread = true)
        {
        }

        /// <summary>
        /// Gets the interaction type.
        /// </summary>
        public int GetInteractionType()
        {
            return 0;
        }

        /// <summary>
        /// Show the reward video Ad.
        /// </summary>
        public void ShowRewardVideoAd()
        {
        }

        /// <summary>
        /// Sets whether to show the download bar.
        /// </summary>
        public void SetShowDownLoadBar(bool show)
        {
        }

        /// <summary>
        /// return the video is From preload
        /// </summary>
        /// <returns>bool</returns>
        public bool materialMetaIsFromPreload()
        {
            return false;
        }
 /// <summary>
        /// get media extra info dictionary,all value is string type,some need developer cast to real type manually
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetMediaExtraInfo()
        {
            var result = new Dictionary<string, string>();
            return result;
        }
 
         public void setAuctionPrice(double price)
        {
            
        }

        public void win(double price)
        {
        }

        public void Loss(double price, string reason, string bidder)
        {
        }
    }
#endif
}
