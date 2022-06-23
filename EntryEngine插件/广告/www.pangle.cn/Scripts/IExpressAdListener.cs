//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

namespace ByteDance.Union
{
    using System.Collections.Generic;
	/// <summary>
	/// The listener for express Ad.
	/// </summary>
	public interface IExpressAdListener
	{
		/// <summary>
		/// Invoke when load Ad error.
		/// </summary>
		void OnError(int code, string message);

		/// <summary>
		/// Invoke when the Ad load success.
		/// </summary>
        void OnExpressAdLoad(List<ExpressAd> ads);
#if UNITY_IOS
        /// <summary>
        /// Invoke when the ExpressBannerAd load success.
        /// </summary>
        void OnExpressBannerAdLoad(ExpressBannerAd ad);
        
        void OnAdViewRenderError(int code, string message);
        
        void OnAdViewRenderSucc(ExpressBannerAd ad);
        
        void OnAdRemoved(ExpressBannerAd ad);

        void OnAdRemoved(ExpressAd ad);

        void OnRenderFailed(int code, string message);

        /// <summary>
        /// Invoke when the ExpressBannerAd load success.
        /// </summary>
        void OnExpressInterstitialAdLoad(ExpressInterstitialAd ad);
        
        void OnAdViewRenderSucc(ExpressInterstitialAd ad);
#else
#endif
    }
}
