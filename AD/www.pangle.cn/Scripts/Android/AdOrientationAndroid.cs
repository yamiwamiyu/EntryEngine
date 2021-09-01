//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

namespace ByteDance.Union
{
    using System;

    internal static class AdOrientationAndroid
    {
        public static int ToAndroid(this AdOrientation orientation)
        {
            switch (orientation)
            {
            case AdOrientation.Vertical:
                return 1;
            case AdOrientation.Horizontal:
                return 2;
            default:
                    throw new NotSupportedException(string.Format("Unknown AdOrientation:{0}",orientation));
            }
        }
    }
}
