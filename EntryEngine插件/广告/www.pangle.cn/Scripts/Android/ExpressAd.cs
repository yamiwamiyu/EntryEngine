//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

namespace ByteDance.Union
{
#if !UNITY_EDITOR && UNITY_ANDROID
    using System;
    using UnityEngine;
using System.Reflection;
    /// <summary>
    /// The splash Ad.
    /// </summary>
    public sealed class ExpressAd : IDisposable {

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
            IExpressAdInteractionListener listener) 
        {
            this.javaObject.Call("setExpressInteractionListener", new ExpressAdInteractionCallback(listener));
        }

        /// <summary>
        /// Sets the download listener.
        /// </summary>
        public void SetDownloadListener (IAppDownloadListener listener) { }

        /// <summary>
        /// Set this Ad not allow sdk count down.
        /// </summary>
        public void SetNotAllowSdkCountdown () { }

        /// <summary>
        /// show the  express Ad
        /// <param name="x">the x of th ad</param>
        /// <param name="y">the y of th ad</param>
        /// </summary>
        public void ShowExpressAd(float x, float y) { }

        /// <inheritdoc/>
        public void Dispose () { 
            this.javaObject.Call("destroy");
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
            public ExpressAdInteractionCallback(IExpressAdInteractionListener callback) : base("com.bytedance.sdk.openadsdk.TTNativeExpressAd$ExpressAdInteractionListener") {
                this.listener = callback;
            }

            void onAdClicked(AndroidJavaObject view, int type) {
                this.listener.OnAdClicked(null);
            }


            void onAdShow(AndroidJavaObject view, int type) {
                this.listener.OnAdShow(null);
            }


            void onRenderFail(AndroidJavaObject view, string msg, int code) {
                this.listener.OnAdViewRenderError(null,code,msg);
            }


            void onRenderSuccess(AndroidJavaObject view, float width, float height) {
                listener.OnAdViewRenderSucc(null, width, height);
            }
        }

    }

#endif
}