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
    /// The banner Ad.
    /// </summary>
    public class BUSplashAd : IDisposable,IClientBidding
    {

        private static int loadContextID = 0;
        private static Dictionary<int, ISplashAdListener> loadListeners =
            new Dictionary<int, ISplashAdListener>();

        private static int interactionContextID = 0;
        private static Dictionary<int, ISplashAdInteractionListener> interactionListeners =
            new Dictionary<int, ISplashAdInteractionListener>();

        private delegate void SplashAd_OnError(int code, string message, int context);
        private delegate void SplashAd_OnLoad(IntPtr splashAd, int context);

        private delegate void SplashAd_OnAdShow(int context, int type);
        private delegate void SplashAd_OnAdClick(int context, int type);
        private delegate void SplashAd_OnAdClose(int context);
        private delegate void SplashAd_OnAdSkip(int context);
        private delegate void SplashAd_OnAdTimeOver(int context);

        private IntPtr splashAd;
        private bool disposed;


        protected static bool CallbackOnMainThead;
        public BUSplashAd ()
        {
        }

        private BUSplashAd(IntPtr splashAd)
        {
            this.splashAd = splashAd;
        }

        ~BUSplashAd()
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

            UnionPlatform_SplashAd_Dispose(this.splashAd);
            this.disposed = true;
        }

        /// <summary>
        /// Load the  splash Ad.
        /// </summary>
        internal static BUSplashAd LoadSplashAd(AdSlot adSlot, ISplashAdListener listener, int timeOut, bool callbackOnMainThead)
        {
            CallbackOnMainThead = callbackOnMainThead;
            var context = loadContextID++;
            loadListeners.Add(context, listener);
            AdSlotStruct slot = AdSlotBuilder.getAdSlot(adSlot);
            IntPtr ad = UnionPlatform_SplashAd_Load(
                ref slot,
                timeOut,
                SplashAd_OnErrorMethod,
                SplashAd_OnLoadMethod,
                context);
                
            return new BUSplashAd(ad);
        }

        /// <summary>
        /// Sets the interaction listener for this Ad.
        /// </summary>
        public void SetSplashInteractionListener(
            ISplashAdInteractionListener listener, bool callbackOnMainThead)
        {
            CallbackOnMainThead = callbackOnMainThead;
            var context = interactionContextID++;
            interactionListeners.Add(context, listener);

            UnionPlatform_SplashAd_SetInteractionListener(
                this.splashAd,
                SplashAd_OnAdShowMethod,
                SplashAd_OnAdClickMethod,
                SplashAd_OnAdCloseMethod,
                SplashAd_OnAdSkipMethod,
                SplashAd_OnAdTimeOverMethod,
                context);
        }

        /// <summary>
        /// Sets the listener for the Ad download.
        /// </summary>
        public void SetDownloadListener(IAppDownloadListener listener)
        {
        }

        /// <summary>
        /// Show the full screen video.
        /// </summary>
        public void ShowSplashAd()
        {
            UnionPlatform_SplashAd_Show(this.splashAd);
        }


        [DllImport("__Internal")]
        private static extern IntPtr UnionPlatform_SplashAd_Load(
            ref AdSlotStruct slot,
            int timeOut,
            SplashAd_OnError onError,
            SplashAd_OnLoad onAdLoad,
            int context);

        [DllImport("__Internal")]
        private static extern void UnionPlatform_SplashAd_SetInteractionListener(
            IntPtr splashAd,
            SplashAd_OnAdShow onShow,
            SplashAd_OnAdClick onAdClick,
            SplashAd_OnAdClose onClose,
            SplashAd_OnAdSkip onAdSkip,
            SplashAd_OnAdTimeOver onAdTimeOver,
            int context);

        [DllImport("__Internal")]
        private static extern void UnionPlatform_SplashAd_Show(
            IntPtr splashAd);

        [DllImport("__Internal")]
        private static extern void UnionPlatform_SplashAd_Dispose(
            IntPtr splashAd);



        [AOT.MonoPInvokeCallback(typeof(SplashAd_OnError))]
        private static void SplashAd_OnErrorMethod(int code, string message, int context)
        {
            Debug.Log("splash load OnError");
            UnityDispatcher.PostTask(() =>
            {
                ISplashAdListener listener;
                if (loadListeners.TryGetValue(context, out listener))
                {
                    loadListeners.Remove(context);
                    listener.OnError(code, message);
                }
                else
                {
                    Debug.LogError(
                        "The SplashAd_OnError can not find the context.");
                }
            }, CallbackOnMainThead);
        }

        [AOT.MonoPInvokeCallback(typeof(SplashAd_OnLoad))]
        private static void SplashAd_OnLoadMethod(IntPtr splashAd, int context)
        {
            Debug.Log("splash load OnSuccess");
            UnityDispatcher.PostTask(() =>
            {
                ISplashAdListener listener;
                if (loadListeners.TryGetValue(context, out listener))
                {
                    loadListeners.Remove(context);
                    listener.OnSplashAdLoad(null);
                }
                else
                {
                    Debug.LogError(
                        "The SplashAd_OnLoad can not find the context.");
                }
            }, CallbackOnMainThead);
        }

        [AOT.MonoPInvokeCallback(typeof(SplashAd_OnAdShow))]
        private static void SplashAd_OnAdShowMethod(int context, int type)
        {
            Debug.Log("splash Ad OnAdShow");
            UnityDispatcher.PostTask(() =>
            {
                ISplashAdInteractionListener listener;
                if (interactionListeners.TryGetValue(context, out listener))
                {
                    listener.OnAdShow(type);
                }
                else
                {
                    Debug.LogError(
                        "The SplashAd_OnAdShow can not find the context.");
                }
            }, CallbackOnMainThead);
        }

        [AOT.MonoPInvokeCallback(typeof(SplashAd_OnAdClick))]
        private static void SplashAd_OnAdClickMethod(int context, int type)
        {
            Debug.Log("splash Ad OnAdClicked type");
            UnityDispatcher.PostTask(() =>
            {
                ISplashAdInteractionListener listener;
                if (interactionListeners.TryGetValue(context, out listener))
                {
                    
                    listener.OnAdClicked(type);
                }
                else
                {
                    Debug.LogError(
                        "The SplashAd_OnAdClick can not find the context.");
                }
            }, CallbackOnMainThead);
        }

    [AOT.MonoPInvokeCallback(typeof(SplashAd_OnAdClose))]
        private static void SplashAd_OnAdCloseMethod(int context)
        {
            Debug.Log("splash Ad OnAdClose");
            UnityDispatcher.PostTask(() =>
            {
                ISplashAdInteractionListener listener;
                if (interactionListeners.TryGetValue(context, out listener))
                {
                    interactionListeners.Remove(context);
                    listener.OnAdClose();
                }
                else
                {
                    Debug.LogError(
                        "The SplashAd_OnAdClose can not find the context.");
                }
            }, CallbackOnMainThead);
        }
        
        [AOT.MonoPInvokeCallback(typeof(SplashAd_OnAdSkip))]
        private static void SplashAd_OnAdSkipMethod(int context)
        {
            Debug.Log("splash Ad OnAdSkip ");
            UnityDispatcher.PostTask(() =>
            {
                ISplashAdInteractionListener listener;
                if (interactionListeners.TryGetValue(context, out listener))
                {
                    listener.OnAdSkip();
                }
                else
                {
                    Debug.LogError(
                        "The SplashAd_OnAdSkip can not find the context.");
                }
            }, CallbackOnMainThead);
        }

        [AOT.MonoPInvokeCallback(typeof(SplashAd_OnAdTimeOver))]
        private static void SplashAd_OnAdTimeOverMethod(int context)
        {
            Debug.Log("splash Ad OnAdTimeOver ");
            UnityDispatcher.PostTask(() =>
            {
                ISplashAdInteractionListener listener;
                if (interactionListeners.TryGetValue(context, out listener))
                {
                    listener.OnAdTimeOver();
                }
                else
                {
                    Debug.LogError(
                        "The SplashAd_OnAdTimeOver can not find the context.");
                }
            }, CallbackOnMainThead);
        }

        public void setAuctionPrice(double price)
        {
            ClientBidManager.SetAuctionPrice(this.splashAd,price);
        }

        public void win(double price)
        {
            ClientBidManager.Win(this.splashAd,price);
        }

        public void Loss(double price, string reason, string bidder)
        {
            ClientBidManager.Loss(this.splashAd,price,reason,bidder);
        }
    }
#endif
}
