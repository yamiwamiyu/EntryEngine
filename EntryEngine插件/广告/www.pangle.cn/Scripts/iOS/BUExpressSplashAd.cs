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
    public sealed class BUExpressSplashAd : BUSplashAd
    {
        private static int loadContextID = 0;

        private static Dictionary<int, ISplashAdListener> loadListeners =
            new Dictionary<int, ISplashAdListener>();

        private static int interactionContextID = 0;

        private static Dictionary<int, ISplashAdInteractionListener> interactionListeners =
            new Dictionary<int, ISplashAdInteractionListener>();

        private delegate void ExpressSplashAd_OnError(int code, string message, int context);

        private delegate void ExpressSplashAd_OnLoad(IntPtr splashAd, int context);

        private delegate void ExpressSplashAd_OnAdShow(int context, int type);

        private delegate void ExpressSplashAd_OnAdClick(int context, int type);

        private delegate void ExpressSplashAd_OnAdClose(int context);

        private delegate void ExpressSplashAd_OnAdSkip(int context);

        private delegate void ExpressSplashAd_OnAdTimeOver(int context);

        private IntPtr splashAd;
        private bool disposed;

        private BUExpressSplashAd(IntPtr splashAd)
        {
            this.splashAd = splashAd;
        }

        ~BUExpressSplashAd()
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

            UnionPlatform_ExpressSplashAd_Dispose(this.splashAd);
            this.disposed = true;
        }


        /// <summary>
        /// Load the  splash Ad.
        /// </summary>
        internal static BUExpressSplashAd LoadSplashAd(AdSlot adSlot, ISplashAdListener listener, int timeOut,
            bool callbackOnMainThead)
        {
            CallbackOnMainThead = callbackOnMainThead;
            var context = loadContextID++;
            loadListeners.Add(context, listener);
            AdSlotStruct slot = AdSlotBuilder.getAdSlot(adSlot);
            IntPtr ad = UnionPlatform_ExpressSplashAd_Load(
                ref slot,
                timeOut,
                ExpressSplashAd_OnErrorMethod,
                ExpressSplashAd_OnLoadMethod,
                context);

            return new BUExpressSplashAd(ad);
        }

        /// <summary>
        /// Sets the interaction listener for this Ad.
        /// </summary>
        public void SetSplashInteractionListener(
            ISplashAdInteractionListener listener, bool callbackOnMainThead = true)
        {
            CallbackOnMainThead = callbackOnMainThead;
            var context = interactionContextID++;
            interactionListeners.Add(context, listener);

            UnionPlatform_ExpressSplashAd_SetInteractionListener(
                this.splashAd,
                ExpressSplashAd_OnAdShowMethod,
                ExpressSplashAd_OnAdClickMethod,
                ExpressSplashAd_OnAdCloseMethod,
                ExpressSplashAd_OnAdSkipMethod,
                ExpressSplashAd_OnAdTimeOverMethod,
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
        private static extern IntPtr UnionPlatform_ExpressSplashAd_Load(
            ref AdSlotStruct adslot,
            int timeOut,
            ExpressSplashAd_OnError onError,
            ExpressSplashAd_OnLoad onAdLoad,
            int context);

        [DllImport("__Internal")]
        private static extern void UnionPlatform_ExpressSplashAd_SetInteractionListener(
            IntPtr splashAd,
            ExpressSplashAd_OnAdShow onShow,
            ExpressSplashAd_OnAdClick onAdClick,
            ExpressSplashAd_OnAdClose onAdClose,
            ExpressSplashAd_OnAdSkip onAdSkip,
            ExpressSplashAd_OnAdTimeOver onAdTimeOver,
            int context);

        [DllImport("__Internal")]
        private static extern void UnionPlatform_SplashAd_Show(
            IntPtr splashAd);

        [DllImport("__Internal")]
        private static extern void UnionPlatform_ExpressSplashAd_Dispose(
            IntPtr splashAd);


        [AOT.MonoPInvokeCallback(typeof(ExpressSplashAd_OnError))]
        private static void ExpressSplashAd_OnErrorMethod(int code, string message, int context)
        {
            Debug.Log("expressSplash load OnError");
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

        [AOT.MonoPInvokeCallback(typeof(ExpressSplashAd_OnLoad))]
        private static void ExpressSplashAd_OnLoadMethod(IntPtr splashAd, int context)
        {
            Debug.Log("expressSplash load Onsucc");
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

        [AOT.MonoPInvokeCallback(typeof(ExpressSplashAd_OnAdShow))]
        private static void ExpressSplashAd_OnAdShowMethod(int context, int type)
        {
            Debug.Log("expressSplash Ad OnAdShow");
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

        [AOT.MonoPInvokeCallback(typeof(ExpressSplashAd_OnAdClick))]
        private static void ExpressSplashAd_OnAdClickMethod(int context, int type)
        {
            Debug.Log("expressSplash Ad OnAdClicked type");
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


        [AOT.MonoPInvokeCallback(typeof(ExpressSplashAd_OnAdClose))]
        private static void ExpressSplashAd_OnAdCloseMethod(int context)
        {
            Debug.Log("expressSplash Ad OnAdClose");
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
                        "The ExpressSplashAd_OnAdClose can not find the context.");
                }
            }, CallbackOnMainThead);
        }

        [AOT.MonoPInvokeCallback(typeof(ExpressSplashAd_OnAdSkip))]
        private static void ExpressSplashAd_OnAdSkipMethod(int context)
        {
            Debug.Log("expressSplash Ad OnAdSkip ");
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

        [AOT.MonoPInvokeCallback(typeof(ExpressSplashAd_OnAdTimeOver))]
        private static void ExpressSplashAd_OnAdTimeOverMethod(int context)
        {
            Debug.Log("expressSplash Ad OnAdTimeOver ");
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
    }
#endif
}