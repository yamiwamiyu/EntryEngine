//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

namespace ByteDance.Union
{
    /// <summary>
    /// The type of AdSlot.
    /// </summary>
    public enum AdSlotType
    {
        /// <summary>
        /// The banner Ad type.
        /// </summary>
        Banner = 1,

        /// <summary>
        /// The interaction Ad type.
        /// </summary>
        InteractionAd = 2,

        /// <summary>
        /// The splash Ad type.
        /// </summary>
        Splash = 3,

        /// <summary>
        /// The cached splash Ad type.
        /// </summary>
        CachedSplash = 4,

        /// <summary>
        /// The feed Ad type.
        /// </summary>
        Feed = 5,

        /// <summary>
        /// The reward video Ad type.
        /// </summary>
        RewardVideo = 7,

        /// <summary>
        /// The full screen video Ad type.
        /// </summary>
        FullScreenVideo = 8,

        /// <summary>
        /// The draw feed Ad type.
        /// </summary>
        DrawFeed = 9,
    }
}
