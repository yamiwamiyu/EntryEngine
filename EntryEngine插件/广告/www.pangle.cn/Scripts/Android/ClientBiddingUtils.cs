#if (DEV || !UNITY_EDITOR) && UNITY_ANDROID

using UnityEngine;

namespace ByteDance.Union
{
    public static class ClientBiddingUtils
    {
        /**
        * 竞价成功时的上报接口
        * @param auctionBidToWin 竞价放第二名价格
        */
        public static void Win(AndroidJavaObject adObject, double auctionBidToWin)
        {
            if (double.IsNaN(auctionBidToWin))
            {
                return;
            }

            adObject?.Call("win", ToDoubleObject(auctionBidToWin));
        }

        /**
        * 竞价失败时的上报接口
        * @param auctionPrice 胜出者的第一名价格（不想上报价格传时传入NaN）
        * @param lossReason 竞价失败的原因（不想上报原因时传入null）
        * @param winBidder 胜出者（不想上报胜出者时传入null）
        */
        public static void Loss(AndroidJavaObject adObject, double auctionPrice, string lossReason, string winBidder)
        {
            adObject?.Call("loss", ToDoubleObject(auctionPrice), lossReason, winBidder);
        }

        /**
        * 开发者传入本次实际结算价（需要在show前调用）
        * @param auctionPrice 开发者传入本次实际结算价（不想传递价格传时传入NaN）
        */
        public static void SetPrice(AndroidJavaObject adObject, double auctionPrice)
        {
            adObject?.Call("setPrice", ToDoubleObject(auctionPrice));
        }

        private static AndroidJavaObject ToDoubleObject(double value)
        {
            if (double.IsNaN(value))
            {
                return null;
            }

            var bridge = new AndroidJavaClass("com.bytedance.android.ClientBiddingBridge");
            var result = bridge.CallStatic<AndroidJavaObject>("toDouble", value);
            return result;
        }
    }
}

#endif