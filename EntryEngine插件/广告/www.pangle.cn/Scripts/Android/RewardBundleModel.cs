using System;
using UnityEngine;

#if UNITY_ANDROID
namespace ByteDance.Union
{
    public class RewardBundleModel : IRewardBundleModel
    {
        ///服务器回调错误码，int型
        private static string REWARD_EXTRA_KEY_ERROR_CODE = "reward_extra_key_error_code";

        ///服务器回调错误信息，string型
        private static string REWARD_EXTRA_KEY_ERROR_MSG = "reward_extra_key_error_msg";

        ///开发者平台配置激励名称，string型
        private static string REWARD_EXTRA_KEY_REWARD_NAME = "reward_extra_key_reward_name";

        ///开发者平台配置激励数量，int型
        private static string REWARD_EXTRA_KEY_REWARD_AMOUNT = "reward_extra_key_reward_amount";

        /// 建议奖励百分比，float型
        /// 基础奖励为1，进阶奖励为0.1,0.2，开发者自行换算
        private static string REWARD_EXTRA_KEY_REWARD_PROPOSE = "reward_extra_key_reward_propose";

        private RewardBundleModel(int serverErrorCode, string serverErrorMsg, string rewardName, int rewardAmount,
            float rewardPropose)
        {
            ServerErrorCode = serverErrorCode;
            ServerErrorMsg = serverErrorMsg;
            RewardName = rewardName;
            RewardAmount = rewardAmount;
            RewardPropose = rewardPropose;
        }

        public static RewardBundleModel Create(AndroidJavaObject extraInfo)
        {
            if (extraInfo == null)
            {
                return null;
            }
               
            try
            {
                var serverErrorCode = extraInfo.Call<int>("getInt", REWARD_EXTRA_KEY_ERROR_CODE);
                var serverErrorMsg = extraInfo.Call<string>("getString", REWARD_EXTRA_KEY_ERROR_MSG);
                var rewardName = extraInfo.Call<string>("getString", REWARD_EXTRA_KEY_REWARD_NAME);
                var rewardAmount = extraInfo.Call<int>("getInt", REWARD_EXTRA_KEY_REWARD_AMOUNT);
                var rewardPropose = extraInfo.Call<float>("getFloat", REWARD_EXTRA_KEY_REWARD_PROPOSE);
                return new RewardBundleModel(serverErrorCode, serverErrorMsg, rewardName, rewardAmount, rewardPropose);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                return null;
            }
        }

        public int ServerErrorCode { get; }
        public string ServerErrorMsg { get; }
        public string RewardName { get; }
        public int RewardAmount { get; }
        public float RewardPropose { get; }

        public override string ToString()
        {
            return $"ServerErrorCode : {ServerErrorCode}, ServerErrorMsg:{ServerErrorMsg}, RewardName:{RewardName}, RewardAmount:{RewardAmount}, RewardPropose:{RewardPropose}";
        }
    }
}
#endif