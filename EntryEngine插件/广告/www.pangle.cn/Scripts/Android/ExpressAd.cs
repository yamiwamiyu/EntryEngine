//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

namespace ByteDance.Union
{
#if (DEV || !UNITY_EDITOR) && UNITY_ANDROID
    using System;
    using UnityEngine;
    /// <summary>
    /// The splash Ad.
    /// </summary>
    public sealed class ExpressAd : IDisposable, IClintBidding
    {

        public AndroidJavaObject javaObject;
        public int index;

        internal ExpressAd (AndroidJavaObject expressAd) {
            this.javaObject = expressAd;
        }
        public AndroidJavaObject handle{get { return this.javaObject; }}
        /// <summary>
        /// Gets the interaction type.
        /// </summary>
        public int GetInteractionType () {

          return this.javaObject.Call<int> ("getInteractionType");
            
        }

        /// <summary>
        /// Sets the interaction listener for this Ad.
        /// </summary>
        public void SetExpressInteractionListener (
            IExpressAdInteractionListener listener, bool callbackOnMainThread = true) 
        {
            this.javaObject.Call("setExpressInteractionListener", new ExpressAdInteractionCallback(listener, callbackOnMainThread));
        }

        /// <summary>
        /// Sets the download listener.
        /// </summary>
        public void SetDownloadListener (IAppDownloadListener listener, bool callbackOnMainThread = true) { }

        /// <summary>
        /// Set this Ad not allow sdk count down.
        /// </summary>
        public void SetNotAllowSdkCountdown () { }

        /// <summary>
        /// show the  express Ad
        /// <param name="x">the x of th ad</param>
        /// <param name="y">the y of th ad</param>
        /// </summary>
		public void ShowExpressAd (float x, float y) { }

        /// <inheritdoc/>
        public void Dispose () { 
            NativeAdManager.Instance().DestoryExpressAd(javaObject);
        }

        /// <summary>
        /// Sets the slide interval time.
        /// </summary>
        /// <param name="intervalTime">Interval time.</param>
        public void SetSlideIntervalTime(int intervalTime){
            this.javaObject.Call("setSlideIntervalTime", intervalTime);
        }


        private sealed class ExpressAdInteractionCallback : AndroidJavaProxy {
            private IExpressAdInteractionListener listener;
            private bool callbackOnMainThread;
            public ExpressAdInteractionCallback(IExpressAdInteractionListener callback, bool callbackOnMainThread) : base("com.bytedance.sdk.openadsdk.TTNativeExpressAd$ExpressAdInteractionListener") {
                this.listener = callback;
                this.callbackOnMainThread = callbackOnMainThread;
            }

            void onAdClicked(AndroidJavaObject view, int type) {
                UnityDispatcher.PostTask(
                   () => this.listener.OnAdClicked(null), callbackOnMainThread);
            }


            void onAdShow(AndroidJavaObject view, int type) {
                UnityDispatcher.PostTask(
                   () => this.listener.OnAdShow(null), callbackOnMainThread);
            }


            void onRenderFail(AndroidJavaObject view, string msg, int code) {
                UnityDispatcher.PostTask(
                   () => this.listener.OnAdViewRenderError(null,code,msg), callbackOnMainThread);
            }


            void onRenderSuccess(AndroidJavaObject view, float width, float height) {
                Debug.Log("onRenderSuccess ");
                UnityDispatcher.PostTask(
                () => listener.OnAdViewRenderSucc(null, width, height), callbackOnMainThread);
            }
        }
        
        public void Win(double auctionBidToWin)
        {
            ClientBiddingUtils.Win(javaObject, auctionBidToWin);
        }

        public void Loss(double auctionPrice = double.NaN, string lossReason = null, string winBidder = null)
        {
            ClientBiddingUtils.Loss(javaObject, auctionPrice, lossReason, winBidder);
        }

        public void SetPrice(double auctionPrice = double.NaN)
        {
            ClientBiddingUtils.SetPrice(javaObject, auctionPrice);
        }

    }

#endif
}