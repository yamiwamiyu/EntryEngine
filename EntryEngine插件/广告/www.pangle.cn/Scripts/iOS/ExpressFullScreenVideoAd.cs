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
    /// The fullScreen video Ad.
    /// </summary>
    public sealed class ExpressFullScreenVideoAd : IDisposable,IClientBidding
    {
        private static int loadContextID = 0;
        private static Dictionary<int, IFullScreenVideoAdListener> loadListeners =
            new Dictionary<int, IFullScreenVideoAdListener>();

        private static int interactionContextID = 0;
        private static Dictionary<int, IFullScreenVideoAdInteractionListener> interactionListeners =
            new Dictionary<int, IFullScreenVideoAdInteractionListener>();

        private delegate void FullScreenVideoAd_OnError(int code, string message, int context);
        private delegate void FullScreenVideoAd_OnFullScreenVideoAdLoad(IntPtr fullScreenVideoAd, int context);
        private delegate void FullScreenVideoAd_OnFullScreenVideoCached(int context);

        private delegate void FullScreenVideoAd_OnAdShow(int context);
        private delegate void FullScreenVideoAd_OnAdVideoBarClick(int context);
        private delegate void FullScreenVideoAd_OnAdVideoClickSkip(int context);
        private delegate void FullScreenVideoAd_OnAdClose(int context);
        private delegate void FullScreenVideoAd_OnVideoComplete(int context);
        private delegate void FullScreenVideoAd_OnVideoError(int context);
        private IntPtr fullScreenVideoAd;
        private bool disposed;

        private static bool _callbackOnMainThead;

        internal ExpressFullScreenVideoAd(IntPtr fullScreenVideoAd)
        {
            this.fullScreenVideoAd = fullScreenVideoAd;
        }

        ~ExpressFullScreenVideoAd()
        {
            this.Dispose(false);
        }

        internal static void LoadFullScreenVideoAd(
            AdSlot adSlot, IFullScreenVideoAdListener listener, bool callbackOnMainThead)
        {
            _callbackOnMainThead = callbackOnMainThead;
            var context = loadContextID++;
            loadListeners.Add(context, listener);

            AdSlotStruct adSlotStruct = AdSlotBuilder.getAdSlot(adSlot);
            UnionPlatform_ExpressFullScreenVideoAd_Load(
                ref adSlotStruct,
                FullScreenVideoAd_OnErrorMethod,
                FullScreenVideoAd_OnFullScreenVideoAdLoadMethod,
                FullScreenVideoAd_OnFullScreenVideoCachedMethod,
                context);
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

            UnionPlatform_ExpressFullScreenVideoAd_Dispose(this.fullScreenVideoAd);
            this.disposed = true;
        }

        /// <summary>
        /// Sets the interaction listener for this Ad.
        /// </summary>
        public void SetFullScreenVideoAdInteractionListener(
            IFullScreenVideoAdInteractionListener listener, bool callbackOnMainThead = true)
        {
            _callbackOnMainThead = callbackOnMainThead;
            var context = interactionContextID++;
            interactionListeners.Add(context, listener);

            UnionPlatform_ExpressFullScreenVideoAd_SetInteractionListener(
                this.fullScreenVideoAd,
                FullScreenVideoAd_OnAdShowMethod,
                FullScreenVideoAd_OnAdVideoBarClickMethod,
                FullScreenVideoAd_OnAdVideoClickSkipMethod,
                FullScreenVideoAd_OnAdCloseMethod,
                FullScreenVideoAd_OnVideoCompleteMethod,
                FullScreenVideoAd_OnVideoErrorMethod,
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

        /// <summary>
        /// Show the fullScreen video Ad.
        /// </summary>
        public void ShowFullScreenVideoAd()
        {
            UnionPlatform_ExpressFullScreenVideoAd_ShowFullScreenVideoAd(this.fullScreenVideoAd);
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
            return UnionPlatform_expressFullScreenVideoMaterialMetaIsFromPreload(this.fullScreenVideoAd);
        }

        /// <summary>
        /// return the expire time of the video
        /// </summary>
        /// <returns>time stamp</returns>
        public long expireTime()
        {
            return UnionPlatform_expressFullScreenVideoExpireTime(this.fullScreenVideoAd);
        }

        [DllImport("__Internal")]
        private static extern void UnionPlatform_ExpressFullScreenVideoAd_Load(
            ref AdSlotStruct adSlotStruct,
            FullScreenVideoAd_OnError onError,
            FullScreenVideoAd_OnFullScreenVideoAdLoad onFullScreenVideoAdLoad,
            FullScreenVideoAd_OnFullScreenVideoCached onFullScreenVideoCached,
            int context);

        [DllImport("__Internal")]
        private static extern void UnionPlatform_ExpressFullScreenVideoAd_SetInteractionListener(
            IntPtr fullScreenVideoAd,
            FullScreenVideoAd_OnAdShow onAdShow,
            FullScreenVideoAd_OnAdVideoBarClick onAdVideoBarClick,
            FullScreenVideoAd_OnAdVideoClickSkip onAdVideoClickSkip,
            FullScreenVideoAd_OnAdClose onAdClose,
            FullScreenVideoAd_OnVideoComplete onVideoComplete,
            FullScreenVideoAd_OnVideoError onVideoError,
            int context);

        [DllImport("__Internal")]
        private static extern void UnionPlatform_ExpressFullScreenVideoAd_ShowFullScreenVideoAd(
            IntPtr fullScreenVideoAd);

        [DllImport("__Internal")]
        private static extern void UnionPlatform_ExpressFullScreenVideoAd_Dispose(
            IntPtr fullScreenVideoAd);
        
        [DllImport("__Internal")]
        private static extern bool UnionPlatform_expressFullScreenVideoMaterialMetaIsFromPreload(IntPtr fullscreenVideoAd);
        [DllImport("__Internal")]
        private static extern long UnionPlatform_expressFullScreenVideoExpireTime(IntPtr fullscreenVideoAd);


        [AOT.MonoPInvokeCallback(typeof(FullScreenVideoAd_OnError))]
        private static void FullScreenVideoAd_OnErrorMethod(int code, string message, int context)
        {
            Debug.Log("OnFullScreenError:" + message);
            UnityDispatcher.PostTask(() =>
            {
                IFullScreenVideoAdListener listener;
                if (loadListeners.TryGetValue(context, out listener))
                {
                    loadListeners.Remove(context);
                    listener.OnError(code, message);
                }
                else
                {
                    Debug.LogError(
                        "The OnError can not find the context.");
                }
            }, _callbackOnMainThead);
        }

        [AOT.MonoPInvokeCallback(typeof(FullScreenVideoAd_OnFullScreenVideoAdLoad))]
        private static void FullScreenVideoAd_OnFullScreenVideoAdLoadMethod(IntPtr fullScreenVideoAd, int context)
        {
            Debug.Log("OnFullScreenAdLoad");
            UnityDispatcher.PostTask(() =>
            {
                IFullScreenVideoAdListener listener;
                if (loadListeners.TryGetValue(context, out listener))
                {
                    listener.OnExpressFullScreenVideoAdLoad(new ExpressFullScreenVideoAd(fullScreenVideoAd));
                }
                else
                {
                    Debug.LogError(
                        "The OnExpressFullScreenVideoAdLoad can not find the context.");
                }
            }, _callbackOnMainThead);
        }

        [AOT.MonoPInvokeCallback(typeof(FullScreenVideoAd_OnFullScreenVideoCached))]
        private static void FullScreenVideoAd_OnFullScreenVideoCachedMethod(int context)
        {
            Debug.Log("OnFullScreenVideoCached");
            UnityDispatcher.PostTask(() =>
            {
                IFullScreenVideoAdListener listener;
                if (loadListeners.TryGetValue(context, out listener))
                {
                    loadListeners.Remove(context);
                    listener.OnFullScreenVideoCached();
                }
                else
                {
                    Debug.LogError(
                        "The OnFullScreenVideoCached can not find the context.");
                }
            }, _callbackOnMainThead);
        }

        [AOT.MonoPInvokeCallback(typeof(FullScreenVideoAd_OnAdShow))]
        private static void FullScreenVideoAd_OnAdShowMethod(int context)
        {
            Debug.Log("fullScreenVideoAd show");
            UnityDispatcher.PostTask(() =>
            {
                IFullScreenVideoAdInteractionListener listener;
                if (interactionListeners.TryGetValue(context, out listener))
                {
                    listener.OnAdShow();
                }
                else
                {
                    Debug.LogError(
                        "The OnAdShow can not find the context.");
                }
            }, _callbackOnMainThead);
        }

        [AOT.MonoPInvokeCallback(typeof(FullScreenVideoAd_OnAdVideoBarClick))]
        private static void FullScreenVideoAd_OnAdVideoBarClickMethod(int context)
        {
            Debug.Log("fullScreenVideoAd OnAdVideoBarClick");
            UnityDispatcher.PostTask(() =>
            {
                IFullScreenVideoAdInteractionListener listener;
                if (interactionListeners.TryGetValue(context, out listener))
                {
                    listener.OnAdVideoBarClick();
                }
                else
                {
                    Debug.LogError(
                        "The OnAdVideoBarClick can not find the context.");
                }
            }, _callbackOnMainThead);
        }

        [AOT.MonoPInvokeCallback(typeof(FullScreenVideoAd_OnAdVideoClickSkip))]
        private static void FullScreenVideoAd_OnAdVideoClickSkipMethod(int context)
        {
            Debug.Log("fullScreenVideoAd OnSkippedVideo");
            UnityDispatcher.PostTask(() =>
            {
                IFullScreenVideoAdInteractionListener listener;
                if (interactionListeners.TryGetValue(context, out listener))
                {
                    listener.OnSkippedVideo();
                }
                else
                {
                    Debug.LogError(
                        "The OnSkippedVideo can not find the context.");
                }
            }, _callbackOnMainThead);
        }

        [AOT.MonoPInvokeCallback(typeof(FullScreenVideoAd_OnAdClose))]
        private static void FullScreenVideoAd_OnAdCloseMethod(int context)
        {
            Debug.Log("fullScreenVideoAd close");
            UnityDispatcher.PostTask(() =>
            {
                IFullScreenVideoAdInteractionListener listener;
                if (interactionListeners.TryGetValue(context, out listener))
                {
                    listener.OnAdClose();
                    interactionListeners.Remove(context);
                }
                else
                {
                    Debug.LogError(
                        "The OnAdClose can not find the context.");
                }
            }, _callbackOnMainThead);
        }

        [AOT.MonoPInvokeCallback(typeof(FullScreenVideoAd_OnVideoComplete))]
        private static void FullScreenVideoAd_OnVideoCompleteMethod(int context)
        {
            UnityDispatcher.PostTask(() =>
            {
                IFullScreenVideoAdInteractionListener listener;
                if (interactionListeners.TryGetValue(context, out listener))
                {
                    listener.OnVideoComplete();
                }
                else
                {
                    Debug.LogError(
                        "The OnVideoComplete can not find the context.");
                }
            }, _callbackOnMainThead);
        }

        [AOT.MonoPInvokeCallback(typeof(FullScreenVideoAd_OnVideoError))]
        private static void FullScreenVideoAd_OnVideoErrorMethod(int context)
        {
            Debug.Log("fullScreenVideoAd OnVideoError");
            UnityDispatcher.PostTask(() =>
            {
                IFullScreenVideoAdInteractionListener listener;
                if (interactionListeners.TryGetValue(context, out listener))
                {
                    listener.OnVideoError();
                }
                else
                {
                    Debug.LogError(
                        "The OnVideoError can not find the context.");
                }
            }, _callbackOnMainThead);
        }

        public void setAuctionPrice(double price)
        {
            ClientBidManager.SetAuctionPrice(this.fullScreenVideoAd,price);
        }

        public void win(double price)
        {
            ClientBidManager.Win(this.fullScreenVideoAd,price);
        }

        public void Loss(double price, string reason, string bidder)
        {
            ClientBidManager.Loss(this.fullScreenVideoAd,price,reason,bidder);
        }
    }
#endif
}
