//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

namespace ByteDance.Union
{
    using System;

    internal static class AdSlotTypeAndroid
    {
        public static int ToAndroid(this AdSlotType type)
        {
            switch (type)
            {
            case AdSlotType.Banner:
                return 1;
            case AdSlotType.InteractionAd:
                return 2;
            case AdSlotType.Splash:
                return 3;
            case AdSlotType.CachedSplash:
                return 4;
            case AdSlotType.Feed:
                return 5;
            case AdSlotType.RewardVideo:
                return 7;
            case AdSlotType.FullScreenVideo:
                return 8;
            case AdSlotType.DrawFeed:
                return 9;
            default:
                    throw new NotSupportedException(string.Format("Unknown AdSlotType: {0}",type));
            }
        }
    }
}
