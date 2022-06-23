namespace ByteDance.Union
{
    public interface IRewardBundleModel
    {
        ///获得服务器验证的错误码
        int ServerErrorCode { get; }

        ///获得服务器验证的错误信息
        string ServerErrorMsg { get; }

        /// 获得开发者平台配置的奖励名称
        string RewardName { get; }

        /// 获得开发者平台配置的奖励数量
        int RewardAmount { get; }

        /// 获得此次奖励建议发放的奖励比例
        float RewardPropose { get; }
    }
}