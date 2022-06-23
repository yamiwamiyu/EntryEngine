//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

namespace ByteDance.Union
{
#if !UNITY_EDITOR && UNITY_IOS
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    /// <summary>
    /// The reward video Ad.
    /// </summary>
    public sealed class ExpressInterstitialAd : IDisposable,IClientBidding
    {
        private static int loadContextID = 0;
        private static Dictionary<int, IExpressAdListener> loadListeners =
            new Dictionary<int, IExpressAdListener>();

        private static int interactionContextID = 0;
        private static Dictionary<int, IExpressAdInteractionListener> interactionListeners =
            new Dictionary<int, IExpressAdInteractionListener>();


        private delegate void ExpressAd_OnLoad(IntPtr expressAd, int context);
        private delegate void ExpressAd_OnLoadError(int code, string message, int context);
        private delegate void ExpressAd_WillVisible(int context);
        private delegate void ExpressAd_DidClick(int context);
        private delegate void ExpressAd_DidClose(int context);
        private delegate void ExpressAd_RenderFailed(int code, string message, int context);
        
        private delegate void ExpressAd_RenderSuccess(IntPtr expressAd, int context);
        private IntPtr expressAd;
        private bool disposed;
        public int index;

        private static bool _callbackOnMainThead;

        internal ExpressInterstitialAd(IntPtr expressAd)
        {
            this.expressAd = expressAd;
        }

        ~ExpressInterstitialAd()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            UnionPlatform_ExpressInterstitialAd_Dispose(this.expressAd);
            this.disposed = true;
        }

        public void ShowExpressAd(float originX, float originY)
        {
            UnionPlatform_ExpressInterstitialsAd_Show(this.expressAd, originX, originY);
        }

        internal static void LoadExpressAd(
            AdSlot slot, IExpressAdListener listener, bool callbackOnMainThead)
        {
            _callbackOnMainThead = callbackOnMainThead;
            var context = loadContextID++;
            loadListeners.Add(context, listener);

            AdSlotStruct adSlotStruct = AdSlotBuilder.getAdSlot(slot);
            UnionPlatform_ExpressInterstitialsAd_Load(
                ref adSlotStruct,
                ExpressAd_OnLoadMethod,
                ExpressAd_OnLoadErrorMethod,
                ExpressAd_RenderSuccessMethod,
                ExpressAd_RenderFailedMethod,
                context);
        }

        /// <summary>
        /// Sets the interaction listener for this Ad.
        /// </summary>
        public void SetDislikeCallback(
            IDislikeInteractionListener listener)
        {

        }


        /// <summary>
        /// Sets the interaction listener for this Ad.
        /// </summary>
        public void SetExpressInteractionListener(
            IExpressAdInteractionListener listener, bool callbackOnMainThead = true)
        {
            _callbackOnMainThead = callbackOnMainThead;
            var context = interactionContextID++;
            interactionListeners.Add(context, listener);

            Debug.Log("chaors unity interactionContextID:" + interactionContextID);
            UnionPlatform_ExpressInterstitialsAd_SetInteractionListener(
                this.expressAd,
                ExpressAd_WillVisibleMethod,
                ExpressAd_DidClickMethod,
                ExpressAd_OnAdDidCloseMethod,
                context);
        }

        /// <summary>
        /// Sets the download listener.
        /// </summary>
        public void SetDownloadListener(IAppDownloadListener listener, bool callbackOnMainThead = true)
        {
        }

        /// <summary>
        /// Gets the interaction type.
        /// </summary>
        public int GetInteractionType()
        {
            return 0;
        }


        [DllImport("__Internal")]
        private static extern void UnionPlatform_ExpressInterstitialsAd_Load(
            ref AdSlotStruct adSlotStruct,
            ExpressAd_OnLoad onLoad,
            ExpressAd_OnLoadError onLoadError,
            ExpressAd_RenderSuccess renderSuccess,
            ExpressAd_RenderFailed renderFailed,
            int context);

        [DllImport("__Internal")]
        private static extern void UnionPlatform_ExpressInterstitialsAd_Show(
            IntPtr expressAd,
            float originX,
            float originY);


        [DllImport("__Internal")]
        private static extern void UnionPlatform_ExpressInterstitialsAd_SetInteractionListener(
            IntPtr expressAd,
            ExpressAd_WillVisible willVisible,
            ExpressAd_DidClick didClick,
            ExpressAd_DidClose didClose,
            int context);

        [DllImport("__Internal")]
        private static extern void UnionPlatform_ExpressInterstitialAd_Dispose(
            IntPtr expressAdPtr);

        [AOT.MonoPInvokeCallback(typeof(ExpressAd_OnLoad))]
        private static void ExpressAd_OnLoadMethod(IntPtr expressAd, int context)
        {
            Debug.Log("OnExpressInterstitialAdLoad");
            UnityDispatcher.PostTask(() =>
            {
                ;
                IExpressAdListener listener;
                if (loadListeners.TryGetValue(context, out listener))
                {
                    loadListeners.Remove(context);
                    listener.OnExpressInterstitialAdLoad(new ExpressInterstitialAd(expressAd));
                }
                else
                {
                    Debug.LogError(
                        "The ExpressAd_OnLoad can not find the context.");
                }
            }, _callbackOnMainThead);
        }

        [AOT.MonoPInvokeCallback(typeof(ExpressAd_OnLoadError))]
        private static void ExpressAd_OnLoadErrorMethod(int code, string message, int context)
        {
            Debug.Log("onExpressAdError:" + message);
            UnityDispatcher.PostTask(() =>
            {
                IExpressAdListener listener;
                if (loadListeners.TryGetValue(context, out listener))
                {
                    listener.OnError(code, message);
                    loadListeners.Remove(context);
                }
                else
                {
                    Debug.LogError(
                        "The ExpressAd_OnLoadError can not find the context.");
                }
            }, _callbackOnMainThead);
        }

        [AOT.MonoPInvokeCallback(typeof(ExpressAd_WillVisible))]
        private static void ExpressAd_WillVisibleMethod(int context)
        {
            UnityDispatcher.PostTask(() =>
            {
                IExpressAdInteractionListener listener;
                if (interactionListeners.TryGetValue(context, out listener))
                {
                    listener.OnAdShow(null);
                }
                else
                {
                    Debug.LogError(
                        "The ExpressAd_WillVisible can not find the context.");
                }
            }, _callbackOnMainThead);
        }
        
               
        [AOT.MonoPInvokeCallback(typeof(ExpressAd_RenderFailed))]
        private static void ExpressAd_RenderFailedMethod(int code, string message, int context)
        {
            Debug.Log("express OnAdViewRenderError,type:ExpressBannerAd");
            UnityDispatcher.PostTask(() =>
            {
                IExpressAdListener listener;
                if (loadListeners.TryGetValue(context, out listener))
                {
                    listener.OnAdViewRenderError(code, message);
                    loadListeners.Remove(context);
                }
                else
                {
                    Debug.LogError(
                        "The ExpressAd_RenderFailed can not find the context.");
                }
            });
        }
        
        [AOT.MonoPInvokeCallback(typeof(ExpressAd_RenderSuccess))]
        private static void ExpressAd_RenderSuccessMethod(IntPtr expressAd, int context)
        {
            UnityDispatcher.PostTask(() =>
            {
                ;
                IExpressAdListener listener;
                if (loadListeners.TryGetValue(context, out listener))
                {
                    loadListeners.Remove(context);
                    listener.OnAdViewRenderSucc(new ExpressInterstitialAd(expressAd));
                }
                else
                {
                    Debug.LogError(
                        "The ExpressAd_RenderSuccessMethod can not find the context.");
                }
            });
        }


        [AOT.MonoPInvokeCallback(typeof(ExpressAd_DidClick))]
        private static void ExpressAd_DidClickMethod(int context)
        {
            UnityDispatcher.PostTask(() =>
            {
                IExpressAdInteractionListener listener;
                if (interactionListeners.TryGetValue(context, out listener))
                {
                    listener.OnAdClicked(null);
                }
                else
                {
                    Debug.LogError(
                        "The ExpressAd_DidClick can not find the context.");
                }
            }, _callbackOnMainThead);
        }

        [AOT.MonoPInvokeCallback(typeof(ExpressAd_DidClose))]
        private static void ExpressAd_OnAdDidCloseMethod(int context)
        {
            //Debug.Log("chaors ExpressAd_OnAdDislikeMethod")
            UnityDispatcher.PostTask(() =>
            {
                IExpressAdInteractionListener listener;
                if (interactionListeners.TryGetValue(context, out listener))
                {
                    listener.OnAdClose(null);
                }
                else
                {
                    Debug.LogError(
                        "The ExpressAd_OnAdDidCloseMethod can not find the context.");
                }
            }, _callbackOnMainThead);
        }

        public void setAuctionPrice(double price)
        {
            ClientBidManager.SetAuctionPrice(this.expressAd,price);
        }

        public void win(double price)
        {
            ClientBidManager.Win(this.expressAd,price);
        }

        public void Loss(double price, string reason, string bidder)
        {
            ClientBidManager.Loss(this.expressAd,price,reason,bidder);
        }
    }
#endif
}
