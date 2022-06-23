using System.Runtime.InteropServices;

namespace ByteDance.Union {
#if !UNITY_EDITOR && UNITY_IOS
    using System;
    using UnityEngine;

    /// <summary>
    ///manager for native ad and express ad.
    /// </summary>
    public class BUSlotABManager {
        
        private static IBUSlotABManagerListener managerListener;
        
        private delegate void BUSlotABManager_FetchSlotComplete(String slotId, int type, int errorCode , String errorMsg);
        /// <summary>
        /// Initializes a new instance of the <see cref="NativeAd"/> class.
        /// </summary>


        public static void FetchSlotWithCodeGroupId(int codeGroupId, IBUSlotABManagerListener listener)
        {
            if (listener == null)
            {
                return;
            } 
            managerListener = listener;
            UnionPlatform_BUSlotABManagerFetchSlotWithCodeGroupId(codeGroupId,BUSlotABManager_FetchSlotCompleteMethod);
        }
        
        [DllImport("__Internal")]
        private static extern void UnionPlatform_BUSlotABManagerFetchSlotWithCodeGroupId(int codeGroupId,BUSlotABManager_FetchSlotComplete complete);
        
        [AOT.MonoPInvokeCallback(typeof(BUSlotABManager_FetchSlotComplete))]
        private static void BUSlotABManager_FetchSlotCompleteMethod(String slotId, int type, int errorCode, String errorMsg)
        {
            Debug.Log("BUSlotABManager_FetchSlotCompleteMethod");
            UnityDispatcher.PostTask(() =>
            {
                if (managerListener != null)
                { 
                    AdSlotType slotType;
                    AdSlotType.TryParse(type.ToString(),out slotType);
                    managerListener.onComplete(slotId,slotType,errorCode,errorMsg);
                }
                else
                {
                    Debug.Log("BUSlotABManager Listener is null");
                }
            });
        }
    }
#endif
}