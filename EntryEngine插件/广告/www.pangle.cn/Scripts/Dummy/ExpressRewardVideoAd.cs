namespace ByteDance.Union
{
#if UNITY_EDITOR || (!UNITY_ANDROID && !UNITY_IOS)
    using System;
    using UnityEngine;

    /// <summary>
    /// The reward video Ad.
    /// </summary>
    public sealed class ExpressRewardVideoAd : IDisposable,IClientBidding
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
            IRewardAdInteractionListener listener, bool callbackOnMainThread = true)
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
