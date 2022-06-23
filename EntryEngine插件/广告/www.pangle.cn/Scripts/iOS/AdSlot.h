//
//  AdSlot.h
//  
//
//  Created by yujie on 2021/9/13.
//
#import "BUAdObjectProtocol.h"
#include <stdio.h>
struct AdSlotStruct {
    char slotId[512];
    int adCount;
    int width;
    int height;
    int adType;
    int adLoadType;
    int intervalTime;
    int viewHeight;
    int viewWidth;
    char mediaExtra[1024];
    char userId[512];
};

