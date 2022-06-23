using System.Runtime.InteropServices;

namespace ByteDance.Union
{
#if UNITY_ANDROID
    using System;
    using UnityEngine;

    /// <summary>
    ///manager for native ad and express ad.
    /// </summary>
    public static class BUSlotABManager
    {
        private const string TtAdSdkClassString = "com.bytedance.sdk.openadsdk.TTAdSdk";

        public static void FetchSlotWithCodeGroupId(long codeGroupId, IBUSlotABManagerListener listener)
        {
            if (listener == null)
            {
                return;
            }

            var ttAdSdk = new AndroidJavaClass(TtAdSdkClassString);
            ttAdSdk.CallStatic("getCodeGroupRit", codeGroupId, new TtCodeGroupRitListener(listener));
        }


        private class TtCodeGroupRitListener : AndroidJavaProxy
        {
            private IBUSlotABManagerListener managerListener;

            public TtCodeGroupRitListener(IBUSlotABManagerListener listener) : base(
                "com.bytedance.sdk.openadsdk.TTCodeGroupRit$TTCodeGroupRitListener")
            {
                this.managerListener = listener;
            }

            /// <summary>
            /// 获取成功
            /// </summary>
            /// <param name="ritObject"></param>
            void onSuccess(AndroidJavaObject ritObject)
            {
#if DEBUG
                Debug.Log("TtCodeGroupRitListener onSuccess");
#endif
                var ritId = ritObject.Call<string>("getRit");
                var slotType = ritObject.Call<int>("getSlotType");
                var type = GetAdSlotType(slotType);

                managerListener?.onComplete(ritId,type,0,null);
            }


            /// <summary>
            /// 获取rit代码位失败
            /// </summary>
            /// <param name="code"></param>
            /// <param name="message"></param>
            void onFail(int code, string message)
            {
#if DEBUG
                Debug.Log("TtCodeGroupRitListener onFail");
#endif
                managerListener?.onComplete(null,AdSlotType.UNKOWN,code,message);
            }

            private static AdSlotType GetAdSlotType(int slotType)
            {
                switch (slotType)
                {
                    case 1:
                        return AdSlotType.Banner;
                    case 2:
                        return AdSlotType.InteractionAd;
                    case 3:
                        return AdSlotType.Splash;
                    case 4:
                        return AdSlotType.CachedSplash;
                    case 5:
                        return AdSlotType.Feed;
                    case 7:
                        return AdSlotType.RewardVideo;
                    case 8:
                        return AdSlotType.FullScreenVideo;
                    case 9:
                        return AdSlotType.DrawFeed;
                    default:
                        return AdSlotType.UNKOWN;
                }
            }
        }
    }
#endif
}