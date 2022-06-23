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
    public class BUSplashAd:IClientBidding
    {
        public AndroidJavaObject ad;

        internal BUSplashAd(AndroidJavaObject ad)
        {

        }

        public AndroidJavaObject getCurrentSplshAd()
        {
            return null;
        }

        public int GetInteractionType()
        {
            return 0;
        }

        /// <summary>
        /// Sets the interaction listener for this Ad.
        /// </summary>
        public void SetSplashInteractionListener(
            ISplashAdInteractionListener listener, bool callbackOnMainThread = true)
        {
        }

        /// <summary>
        /// Sets the listener for the Ad download.
        /// </summary>
        public void SetDownloadListener(IAppDownloadListener listener,bool callbackOnMainThread = true)
        {
        }

        public void Dispose()
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
