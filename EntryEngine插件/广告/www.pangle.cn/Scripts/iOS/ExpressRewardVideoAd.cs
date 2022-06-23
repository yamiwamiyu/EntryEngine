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
    public sealed class ExpressRewardVideoAd : IDisposable,IClientBidding
    {
        private static int loadContextID = 0;
        private static Dictionary<int, IRewardVideoAdListener> loadListeners =
            new Dictionary<int, IRewardVideoAdListener>();

        private static int interactionContextID = 0;
        private static Dictionary<int, IRewardAdInteractionListener> interactionListeners =
            new Dictionary<int, IRewardAdInteractionListener>();
            private static Dictionary<int, IRewardAdInteractionListener> againInteractionListeners =
            new Dictionary<int, IRewardAdInteractionListener>();

        private delegate void RewardVideoAd_OnError(int code, string message, int context);
        private delegate void RewardVideoAd_OnRewardVideoAdLoad(IntPtr rewardVideoAd, int context);
        private delegate void RewardVideoAd_OnRewardVideoCached(int context);

        private delegate void RewardVideoAd_OnAdShow(int context);
        private delegate void RewardVideoAd_OnAdVideoBarClick(int context);
        private delegate void RewardVideoAd_OnAdClose(int context);
        private delegate void RewardVideoAd_OnVideoComplete(int context);
        private delegate void RewardVideoAd_OnVideoError(int context);
        private delegate void RewardVideoAd_OnVideoSkip(int context);
        private delegate void RewardVideoAd_OnRewardVerify(
            bool rewardVerify, int rewardAmount, string rewardName, int rewardType, float rewardPropose, int context);

        private IntPtr rewardVideoAd;
        private bool disposed;

        private static bool _callbackOnMainThead;
        internal ExpressRewardVideoAd(IntPtr rewardVideoAd)
        {
            this.rewardVideoAd = rewardVideoAd;
        }

        ~ExpressRewardVideoAd()
        {
            this.Dispose(false);
        }

        internal static void LoadRewardVideoAd(
            AdSlot adSlot, IRewardVideoAdListener listener, bool callbackOnMainThead)
        {
            _callbackOnMainThead = callbackOnMainThead;
            var context = loadContextID++;
            loadListeners.Add(context, listener);
            AdSlotStruct slot = AdSlotBuilder.getAdSlot(adSlot);
            UnionPlatform_ExpressRewardVideoAd_Load(
                ref slot,
                RewardVideoAd_OnErrorMethod,
                RewardVideoAd_OnRewardVideoAdLoadMethod,
                RewardVideoAd_OnRewardVideoCachedMethod,
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

            UnionPlatform_ExpressRewardVideoAd_Dispose(this.rewardVideoAd);
            this.disposed = true;
        }

        /// <summary>
        /// Sets the interaction listener for this Ad.
        /// </summary>
        public void SetRewardAdInteractionListener(
            IRewardAdInteractionListener listener, bool callbackOnMainThead = true)
        {
            _callbackOnMainThead = callbackOnMainThead;
            var context = interactionContextID++;
            interactionListeners.Add(context, listener);

            UnionPlatform_ExpressRewardVideoAd_SetInteractionListener(
                this.rewardVideoAd,
                RewardVideoAd_OnAdShowMethod,
                RewardVideoAd_OnAdVideoBarClickMethod,
                RewardVideoAd_OnAdCloseMethod,
                RewardVideoAd_OnVideoCompleteMethod,
                RewardVideoAd_OnVideoErrorMethod,
                RewardVideoAd_OnVideoSkipMethod,
                RewardVideoAd_OnRewardVerifyMethod,
                context);
        }

        /// <summary>
        /// Sets the interaction listener for this Ad.
        /// </summary>
        public void SetAgainRewardAdInteractionListener(
            IRewardAdInteractionListener againListener, bool callbackOnMainThead = true)
        {
            _callbackOnMainThead = callbackOnMainThead;
            againInteractionListeners.Add(interactionContextID, againListener);

            UnionPlatform_ExpressRewardVideoAd_Again_SetInteractionListener(
                this.rewardVideoAd,
                RewardVideoAd_Again_OnAdShowMethod,
                RewardVideoAd_Again_OnAdVideoBarClickMethod,
                RewardVideoAd_Again_OnVideoCompleteMethod,
                RewardVideoAd_Again_OnVideoErrorMethod,
                RewardVideoAd_Again_OnVideoSkipMethod,
                RewardVideoAd_Again_OnRewardVerifyMethod,
                interactionContextID);
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
        /// Show the reward video Ad.
        /// </summary>
        public void ShowRewardVideoAd()
        {
            UnionPlatform_ExpressRewardVideoAd_ShowRewardVideoAd(this.rewardVideoAd);
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
            return UnionPlatform_expressRewardVideoMaterialMetaIsFromPreload(this.rewardVideoAd);
        }

        /// <summary>
        /// return the expire time of the video
        /// </summary>
        /// <returns>time stamp</returns>
        public long expireTime()
        {
            return UnionPlatform_expressRewardVideoExpireTime(this.rewardVideoAd);
        }

        [DllImport("__Internal")]
        private static extern void UnionPlatform_ExpressRewardVideoAd_Load(
            ref AdSlotStruct adSlot,
            RewardVideoAd_OnError onError,
            RewardVideoAd_OnRewardVideoAdLoad onRewardVideoAdLoad,
            RewardVideoAd_OnRewardVideoCached onRewardVideoCached,
            int context);

        [DllImport("__Internal")]
        private static extern void UnionPlatform_ExpressRewardVideoAd_SetInteractionListener(
            IntPtr rewardVideoAd,
            RewardVideoAd_OnAdShow onAdShow,
            RewardVideoAd_OnAdVideoBarClick onAdVideoBarClick,
            RewardVideoAd_OnAdClose onAdClose,
            RewardVideoAd_OnVideoComplete onVideoComplete,
            RewardVideoAd_OnVideoError onVideoError,
            RewardVideoAd_OnVideoSkip onVideoSkip,
            RewardVideoAd_OnRewardVerify onRewardVerify,
            int context);
        [DllImport("__Internal")]
        private static extern void UnionPlatform_ExpressRewardVideoAd_Again_SetInteractionListener(
            IntPtr rewardVideoAd,
            RewardVideoAd_OnAdShow onAdShow,
            RewardVideoAd_OnAdVideoBarClick onAdVideoBarClick,
            RewardVideoAd_OnVideoComplete onVideoComplete,
            RewardVideoAd_OnVideoError onVideoError,
            RewardVideoAd_OnVideoSkip onVideoSkip,
            RewardVideoAd_OnRewardVerify onRewardVerify,
            int context);
            

        [DllImport("__Internal")]
        private static extern void UnionPlatform_ExpressRewardVideoAd_ShowRewardVideoAd(
            IntPtr rewardVideoAd);

        [DllImport("__Internal")]
        private static extern void UnionPlatform_ExpressRewardVideoAd_Dispose(
            IntPtr rewardVideoAd);
        
        [DllImport("__Internal")]
        private static extern bool UnionPlatform_expressRewardVideoMaterialMetaIsFromPreload(IntPtr rewardVideoAd);
        [DllImport("__Internal")]
        private static extern long UnionPlatform_expressRewardVideoExpireTime(IntPtr rewardVideoAd);

        [AOT.MonoPInvokeCallback(typeof(RewardVideoAd_OnError))]
        private static void RewardVideoAd_OnErrorMethod(int code, string message, int context)
        {
            Debug.Log("OnRewardError:" + message);
            UnityDispatcher.PostTask(() =>
            {
                IRewardVideoAdListener listener;
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

        [AOT.MonoPInvokeCallback(typeof(RewardVideoAd_OnRewardVideoAdLoad))]
        private static void RewardVideoAd_OnRewardVideoAdLoadMethod(IntPtr rewardVideoAd, int context)
        {
            Debug.Log("OnRewardVideoAdLoad");
            UnityDispatcher.PostTask(() =>
            {
                IRewardVideoAdListener listener;
                if (loadListeners.TryGetValue(context, out listener))
                {
                    listener.OnExpressRewardVideoAdLoad(new ExpressRewardVideoAd(rewardVideoAd));
                }
                else
                {
                    Debug.LogError(
                        "The OnExpressRewardVideoAdLoad can not find the context.");
                }
            }, _callbackOnMainThead);
        }

        [AOT.MonoPInvokeCallback(typeof(RewardVideoAd_OnRewardVideoCached))]
        private static void RewardVideoAd_OnRewardVideoCachedMethod(int context)
        {
            Debug.Log("OnRewardVideoCached");
            UnityDispatcher.PostTask(() =>
            {
                IRewardVideoAdListener listener;
                if (loadListeners.TryGetValue(context, out listener))
                {
                    listener.OnRewardVideoCached();
                    loadListeners.Remove(context);
                }
                else
                {
                    Debug.LogError(
                        "The OnRewardVideoCached can not find the context.");
                }
            }, _callbackOnMainThead);
        }

        [AOT.MonoPInvokeCallback(typeof(RewardVideoAd_OnAdShow))]
        private static void RewardVideoAd_OnAdShowMethod(int context)
        {
            UnityDispatcher.PostTask(() =>
            {
                IRewardAdInteractionListener listener;
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

        [AOT.MonoPInvokeCallback(typeof(RewardVideoAd_OnAdVideoBarClick))]
        private static void RewardVideoAd_OnAdVideoBarClickMethod(int context)
        {
            Debug.Log("rewardVideoAd OnAdVideoBarClick");
            UnityDispatcher.PostTask(() =>
            {
                IRewardAdInteractionListener listener;
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

        [AOT.MonoPInvokeCallback(typeof(RewardVideoAd_OnAdClose))]
        private static void RewardVideoAd_OnAdCloseMethod(int context)
        {
            Debug.Log("rewardVideoAd close");
            UnityDispatcher.PostTask(() =>
            {
                IRewardAdInteractionListener listener;
                if (interactionListeners.TryGetValue(context, out listener))
                {
                    listener.OnAdClose();
                    interactionListeners.Remove(context);
                    againInteractionListeners.Remove(context);
                }
                else
                {
                    Debug.LogError(
                        "The OnAdClose can not find the context.");
                }
            }, _callbackOnMainThead);
        }

        [AOT.MonoPInvokeCallback(typeof(RewardVideoAd_OnVideoComplete))]
        private static void RewardVideoAd_OnVideoCompleteMethod(int context)
        {
            Debug.Log("rewardVideoAd complete");
            UnityDispatcher.PostTask(() =>
            {
                IRewardAdInteractionListener listener;
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

        [AOT.MonoPInvokeCallback(typeof(RewardVideoAd_OnVideoError))]
        private static void RewardVideoAd_OnVideoErrorMethod(int context)
        {
            Debug.Log("rewardVideoAd error");
            UnityDispatcher.PostTask(() =>
            {
                IRewardAdInteractionListener listener;
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

        [AOT.MonoPInvokeCallback(typeof(RewardVideoAd_OnVideoSkip))]
        private static void RewardVideoAd_OnVideoSkipMethod(int context)
        {
            Debug.Log("rewardVideoAd skip");
            UnityDispatcher.PostTask(() =>
            {
                IRewardAdInteractionListener listener;
                if (interactionListeners.TryGetValue(context, out listener))
                {
                    listener.OnVideoSkip();
                }
                else
                {
                    Debug.LogError(
                        "The OnVideoSkip can not find the context.");
                }
            }, _callbackOnMainThead);
        }

        [AOT.MonoPInvokeCallback(typeof(RewardVideoAd_OnRewardVerify))]
        private static void RewardVideoAd_OnRewardVerifyMethod(
            bool rewardVerify, int rewardAmount, string rewardName, int rewardType, float rewardPropose,  int context)
        {
            Debug.Log("rewardVideoAd verify:"+ rewardVerify.ToString());
            UnityDispatcher.PostTask(() =>
            {
                IRewardAdInteractionListener listener;
                if (interactionListeners.TryGetValue(context, out listener))
                {
                    listener.OnRewardVerify(rewardVerify, rewardAmount, rewardName, rewardType, rewardPropose);
                }
                else
                {
                    Debug.LogError(
                        "The OnRewardVerify can not find the context.");
                }
            }, _callbackOnMainThead);
        }

        ///-------------again

        [AOT.MonoPInvokeCallback(typeof(RewardVideoAd_OnAdShow))]
        private static void RewardVideoAd_Again_OnAdShowMethod(int context)
        {
            Debug.Log("rewardVideoAd show");
            UnityDispatcher.PostTask(() =>
            {
                IRewardAdInteractionListener listener;
                if (againInteractionListeners.TryGetValue(context, out listener))
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

        [AOT.MonoPInvokeCallback(typeof(RewardVideoAd_OnAdVideoBarClick))]
        private static void RewardVideoAd_Again_OnAdVideoBarClickMethod(int context)
        {
            UnityDispatcher.PostTask(() =>
            {
                IRewardAdInteractionListener listener;
                if (againInteractionListeners.TryGetValue(context, out listener))
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

        [AOT.MonoPInvokeCallback(typeof(RewardVideoAd_OnVideoComplete))]
        private static void RewardVideoAd_Again_OnVideoCompleteMethod(int context)
        {
            UnityDispatcher.PostTask(() =>
            {
                IRewardAdInteractionListener listener;
                if (againInteractionListeners.TryGetValue(context, out listener))
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

        [AOT.MonoPInvokeCallback(typeof(RewardVideoAd_OnVideoError))]
        private static void RewardVideoAd_Again_OnVideoErrorMethod(int context)
        {
            Debug.Log("rewardVideoAd error");
            UnityDispatcher.PostTask(() =>
            {
                IRewardAdInteractionListener listener;
                if (againInteractionListeners.TryGetValue(context, out listener))
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

        [AOT.MonoPInvokeCallback(typeof(RewardVideoAd_OnVideoSkip))]
        private static void RewardVideoAd_Again_OnVideoSkipMethod(int context)
        {
            UnityDispatcher.PostTask(() =>
            {
                IRewardAdInteractionListener listener;
                if (againInteractionListeners.TryGetValue(context, out listener))
                {
                    listener.OnVideoSkip();
                }
                else
                {
                    Debug.LogError(
                        "The OnVideoSkip can not find the context.");
                }
            }, _callbackOnMainThead);
        }

        [AOT.MonoPInvokeCallback(typeof(RewardVideoAd_OnRewardVerify))]
        private static void RewardVideoAd_Again_OnRewardVerifyMethod(
            bool rewardVerify, int rewardAmount, string rewardName, int rewardType, float rewardPropose, int context)
        {
            UnityDispatcher.PostTask(() =>
            {
                IRewardAdInteractionListener listener;
                if (againInteractionListeners.TryGetValue(context, out listener))
                {
                    listener.OnRewardVerify(rewardVerify, rewardAmount, rewardName, rewardType, rewardPropose);
                }
                else
                {
                    Debug.LogError(
                        "The OnRewardVerify can not find the context.");
                }
            }, _callbackOnMainThead);
        }

        public void setAuctionPrice(double price)
        {
            ClientBidManager.SetAuctionPrice(this.rewardVideoAd,price);
        }

        public void win(double price)
        {
            ClientBidManager.Win(this.rewardVideoAd,price);
        }

        public void Loss(double price, string reason, string bidder)
        {
            ClientBidManager.Loss(this.rewardVideoAd,price,reason,bidder);
        }
    }
#endif
}
