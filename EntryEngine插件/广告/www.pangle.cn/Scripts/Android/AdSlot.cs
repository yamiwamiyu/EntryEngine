//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

namespace ByteDance.Union
{
    using UnityEngine;

#if !UNITY_EDITOR && UNITY_ANDROID
    /// <summary>
    /// The Ad slot for android.
    /// </summary>
    public sealed class AdSlot
    {
        private AndroidJavaObject slot;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdSlot"/> class.
        /// </summary>
        internal AdSlot(AndroidJavaObject slot)
        {
            this.slot = slot;
        }

        internal AndroidJavaObject Handle
        {
            get { return this.slot; }
        }

        /// <summary>
        /// The builder used to build an <see cref="AdSlot"/>.
        /// </summary>
        public sealed class Builder
        {
            private AndroidJavaObject builder;

            /// <summary>
            /// Initializes a new instance of the <see cref="Builder"/> class.
            /// </summary>
            public Builder()
            {
                this.builder = new AndroidJavaObject(
                    "com.bytedance.sdk.openadsdk.AdSlot$Builder");
            }

            /// <summary>
            /// Sets the code ID.
            /// </summary>
            public Builder SetCodeId(string codeId)
            {
                this.builder.Call<AndroidJavaObject>("setCodeId", codeId);
                return this;
            }

            /// <summary>
            /// Sets the image accepted size.
            /// </summary>
            public Builder SetImageAcceptedSize(int width, int height)
            {
                this.builder.Call<AndroidJavaObject>(
                    "setImageAcceptedSize", width, height);
                return this;
            }

            /// <summary>
            /// Sets the size of the express view accepted in dp.
            /// </summary>
            /// <returns>The Builder.</returns>
            /// <param name="width">Width.</param>
            /// <param name="height">Height.</param>
            public Builder SetExpressViewAcceptedSize(float width, float height)
            {
                this.builder.Call<AndroidJavaObject>(
                    "setExpressViewAcceptedSize", width, height);
                return this;
            }

            /// <summary>
            /// Sets a value indicating wheteher the Ad support deep link.
            /// </summary>
            public Builder SetSupportDeepLink(bool support)
            {
                this.builder.Call<AndroidJavaObject>(
                    "setSupportDeepLink", support);
                return this;
            }

            /// <summary>
            /// Sets the Ad count.
            /// </summary>
            public Builder SetAdCount(int count)
            {
                this.builder.Call<AndroidJavaObject>("setAdCount", count);
                return this;
            }

            /// <summary>
            /// Sets the Native Ad type.
            /// </summary>
            public Builder SetNativeAdType(AdSlotType type)
            {
                this.builder.Call<AndroidJavaObject>(
                    "setNativeAdType", type.ToAndroid());
                return this;
            }

            /// <summary>
            /// Sets the reward name.
            /// </summary>
            public Builder SetRewardName(string name)
            {
                this.builder.Call<AndroidJavaObject>("setRewardName", name);
                return this;
            }

            /// <summary>
            /// Sets the reward amount.
            /// </summary>
            public Builder SetRewardAmount(int amount)
            {
                this.builder.Call<AndroidJavaObject>("setRewardAmount", amount);
                return this;
            }

            /// <summary>
            /// Sets the user ID.
            /// </summary>
            public Builder SetUserID(string id)
            {
                this.builder.Call<AndroidJavaObject>("setUserID", id);
                return this;
            }

            /// <summary>
            /// Sets the Ad orientation.
            /// </summary>
            public Builder SetOrientation(AdOrientation orientation)
            {
                this.builder.Call<AndroidJavaObject>(
                    "setOrientation", orientation.ToAndroid());
                return this;
            }

            /// <summary>
            /// Sets the extra media for Ad.
            /// </summary>
            public Builder SetMediaExtra(string extra)
            {
                this.builder.Call<AndroidJavaObject>("setMediaExtra", extra);
                return this;
            }

            /// <summary>
            /// Build the Ad slot.
            /// </summary>
            public AdSlot Build()
            {
                var native = this.builder.Call<AndroidJavaObject>("build");
                return new AdSlot(native);
            }
        }
    }
#endif
}
