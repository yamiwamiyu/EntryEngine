#if (DEV || !UNITY_EDITOR) && UNITY_ANDROID
namespace ByteDance.Union
{
    public interface IClintBidding
    {
        /**
        * 竞价成功时的上报接口
        * @param auctionBidToWin 竞价放第二名价格
        */
        void Win(double auctionBidToWin);

        /**
        * 竞价失败时的上报接口
        * @param auctionPrice 胜出者的第一名价格（不想上报价格传时可不传入）
        * @param lossReason 竞价失败的原因（不想上报原因时可不传入）
        * @param winBidder 胜出者（不想上报胜出者时可不传入）
        */
        void Loss(double auctionPrice=double.NaN, string lossReason = null, string winBidder = null);

        /**
        * 开发者传入本次实际结算价（需要在show前调用）
        * @param auctionPrice 开发者传入本次实际结算价（不想传递价格可不传入）
        */
        void SetPrice(double auctionPrice=double.NaN);
    }
}
#endif