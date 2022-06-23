using System;
using System.Runtime.InteropServices;

namespace ByteDance.Union
{
#if !UNITY_EDITOR && UNITY_IOS
    public class ClientBidManager
    {
        public static void SetAuctionPrice(IntPtr ad, double price)
        {
            UnionPlatform_ClientBidSetPrice(ad,price);
        }

        public static void Win(IntPtr ad, double price)
        {
            UnionPlatform_ClientBidWin(ad,price);
        }

        public static void Loss(IntPtr ad, double price, string reason, string bidder)
        {
            UnionPlatform_ClientBidSetLoss(ad,price,reason,bidder);
        }
        
        [DllImport("__Internal")]
        private static extern void UnionPlatform_ClientBidSetPrice(IntPtr ad,double price);
        
        [DllImport("__Internal")]
        private static extern void UnionPlatform_ClientBidWin(IntPtr ad,double price);
        
        [DllImport("__Internal")]
        private static extern void UnionPlatform_ClientBidSetLoss(IntPtr ad,double price,string reason,string bidder);

    }
#endif
}