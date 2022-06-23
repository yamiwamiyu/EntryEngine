//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

#import <BUAdSDK/BURewardedVideoAd.h>
#import <BUAdSDK/BURewardedVideoModel.h>
#import "UnityAppController.h"
#import "AdSlot.h"
extern const char* AutonomousStringCopy(const char* string);

const char* AutonomousStringCopy(const char* string)
{
    if (string == NULL) {
        return NULL;
    }
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

// IRewardVideoAdListener callbacks.
typedef void(*RewardVideoAd_OnError)(int code, const char* message, int context);
typedef void(*RewardVideoAd_OnRewardVideoAdLoad)(void* rewardVideoAd, int context);
typedef void(*RewardVideoAd_OnRewardVideoCached)(int context);

// IRewardAdInteractionListener callbacks.
typedef void(*RewardVideoAd_OnAdShow)(int context);
typedef void(*RewardVideoAd_OnAdVideoBarClick)(int context);
typedef void(*RewardVideoAd_OnAdClose)(int context);
typedef void(*RewardVideoAd_OnVideoComplete)(int context);
typedef void(*RewardVideoAd_OnVideoError)(int context);
typedef void(*RewardVideoAd_OnVideoSkip)(int context);
typedef void(*ExpressRewardVideoAd_OnRewardVerify)(
    bool rewardVerify, int rewardAmount, const char* rewardName, int rewardType, float rewardPropose, int context);

// The BURewardedVideoAdDelegate implement.
@interface RewardVideoAd : NSObject
@end

@interface RewardVideoAd () <BURewardedVideoAdDelegate,BUAdObjectProtocol>
@property (nonatomic, strong) BURewardedVideoAd *rewardedVideoAd;

@property (nonatomic, assign) int loadContext;
@property (nonatomic, assign) RewardVideoAd_OnError onError;
@property (nonatomic, assign) RewardVideoAd_OnRewardVideoAdLoad onRewardVideoAdLoad;
@property (nonatomic, assign) RewardVideoAd_OnRewardVideoCached onRewardVideoCached;

@property (nonatomic, assign) int interactionContext;
@property (nonatomic, assign) RewardVideoAd_OnAdShow onAdShow;
@property (nonatomic, assign) RewardVideoAd_OnAdVideoBarClick onAdVideoBarClick;
@property (nonatomic, assign) RewardVideoAd_OnAdClose onAdClose;
@property (nonatomic, assign) RewardVideoAd_OnVideoComplete onVideoComplete;
@property (nonatomic, assign) RewardVideoAd_OnVideoError onVideoError;
@property (nonatomic, assign) RewardVideoAd_OnVideoSkip onVideoSkip;
@property (nonatomic, assign) ExpressRewardVideoAd_OnRewardVerify onRewardVerify;

@property (nonatomic, strong) RewardVideoAd *againObject;

@end

@implementation RewardVideoAd
- (void)rewardedVideoAdDidLoad:(BURewardedVideoAd *)rewardedVideoAd {
    if (self.onRewardVideoAdLoad) {
        self.onRewardVideoAdLoad((__bridge void*)self, self.loadContext);
    }
}

- (void)rewardedVideoAdVideoDidLoad:(BURewardedVideoAd *)rewardedVideoAd {
    if (self.onRewardVideoCached) {
        self.onRewardVideoCached(self.loadContext);
    }
}

- (void)rewardedVideoAdWillVisible:(BURewardedVideoAd *)rewardedVideoAd {
    if (self.onAdShow) {
        self.onAdShow(self.interactionContext);
    }
}

- (void)rewardedVideoAdDidClose:(BURewardedVideoAd *)rewardedVideoAd {
    if (self.onAdClose) {
        self.onAdClose(self.interactionContext);
    }
}

- (void)rewardedVideoAdDidClick:(BURewardedVideoAd *)rewardedVideoAd {
    if (self.onAdVideoBarClick) {  
        self.onAdVideoBarClick(self.interactionContext);
    }
}

- (void)rewardedVideoAd:(BURewardedVideoAd *)rewardedVideoAd didFailWithError:(NSError *)error {
    if (self.onError) {
        self.onError((int)error.code, AutonomousStringCopy([[error localizedDescription] UTF8String]), self.loadContext);
    }
}

- (void)rewardedVideoAdDidClickSkip:(BURewardedVideoAd *)rewardedVideoAd {
    if (self.onVideoSkip) {
        self.onVideoSkip(self.interactionContext);
    }
}

- (void)rewardedVideoAdDidPlayFinish:(BURewardedVideoAd *)rewardedVideoAd didFailWithError:(NSError *)error {
    if (error) {
        if (self.onVideoError) {
            self.onVideoError(self.interactionContext);
        }
    } else {
        if (self.onVideoComplete) {
            self.onVideoComplete(self.interactionContext);
        }
    }
}

- (void)rewardedVideoAdServerRewardDidFail:(BURewardedVideoAd *)rewardedVideoAd error:(NSError *)error {
    if (self.onRewardVerify) {
        NSString *rewardName = rewardedVideoAd.rewardedVideoModel.rewardName?:@"";
        self.onRewardVerify(false,
                            (int)rewardedVideoAd.rewardedVideoModel.rewardAmount,
                            (char*)[rewardName cStringUsingEncoding:NSUTF8StringEncoding],
                            (int)rewardedVideoAd.rewardedVideoModel.rewardType,
                            rewardedVideoAd.rewardedVideoModel.rewardPropose,
                            self.interactionContext);
    }
}

- (void)rewardedVideoAdServerRewardDidSucceed:(BURewardedVideoAd *)rewardedVideoAd verify:(BOOL)verify {
    if (self.onRewardVerify) {
        NSString *rewardName = rewardedVideoAd.rewardedVideoModel.rewardName?:@"";
        self.onRewardVerify(verify,
                            (int)rewardedVideoAd.rewardedVideoModel.rewardAmount,
                            (char*)[rewardName cStringUsingEncoding:NSUTF8StringEncoding],
                            (int)rewardedVideoAd.rewardedVideoModel.rewardType,
                            rewardedVideoAd.rewardedVideoModel.rewardPropose,
                            self.interactionContext);
    }
}

- (id<BUAdClientBiddingProtocol>)adObject {
    return self.rewardedVideoAd;
}
@end

#if defined (__cplusplus)
extern "C" {
#endif

void UnionPlatform_RewardVideoAd_Load(
    AdSlotStruct *adSlot,
    RewardVideoAd_OnError onError,
    RewardVideoAd_OnRewardVideoAdLoad onRewardVideoAdLoad,
    RewardVideoAd_OnRewardVideoCached onRewardVideoCached,
    int context) {
    BURewardedVideoModel *model = [[BURewardedVideoModel alloc] init];
    model.userId = [[NSString alloc] initWithUTF8String:adSlot->userId?:""];
    model.extra = [[NSString alloc] initWithUTF8String:adSlot->mediaExtra?:""];
    
    BURewardedVideoAd* rewardedVideoAd = [[BURewardedVideoAd alloc] initWithSlotID:[[NSString alloc] initWithUTF8String:adSlot->slotId?:""] rewardedVideoModel:model];

    RewardVideoAd* instance = [[RewardVideoAd alloc] init];
    RewardVideoAd* again_instance = [[RewardVideoAd alloc] init];

    instance.rewardedVideoAd = rewardedVideoAd;
    instance.againObject = again_instance;

    instance.onError = onError;
    instance.onRewardVideoAdLoad = onRewardVideoAdLoad;
    instance.onRewardVideoCached = onRewardVideoCached;

    instance.loadContext = context;
    again_instance.loadContext = context;

    rewardedVideoAd.delegate = instance;
    rewardedVideoAd.rewardPlayAgainInteractionDelegate = again_instance;
    [rewardedVideoAd loadAdData];

    (__bridge_retained void*)instance;
}

void UnionPlatform_RewardVideoAd_SetInteractionListener(
    void* rewardedVideoAdPtr,
    RewardVideoAd_OnAdShow onAdShow,
    RewardVideoAd_OnAdVideoBarClick onAdVideoBarClick,
    RewardVideoAd_OnAdClose onAdClose,
    RewardVideoAd_OnVideoComplete onVideoComplete,
    RewardVideoAd_OnVideoError onVideoError,
    RewardVideoAd_OnVideoSkip onVideoSkip,
    ExpressRewardVideoAd_OnRewardVerify onRewardVerify,
    int context) {
    RewardVideoAd* rewardedVideoAd = (__bridge RewardVideoAd*)rewardedVideoAdPtr;
    rewardedVideoAd.onAdShow = onAdShow;
    rewardedVideoAd.onAdVideoBarClick = onAdVideoBarClick;
    rewardedVideoAd.onAdClose = onAdClose;
    rewardedVideoAd.onVideoComplete = onVideoComplete;
    rewardedVideoAd.onVideoError = onVideoError;
    rewardedVideoAd.onVideoSkip = onVideoSkip;
    rewardedVideoAd.onRewardVerify = onRewardVerify;
    rewardedVideoAd.interactionContext = context;
}

void UnionPlatform_RewardVideoAd_ShowRewardVideoAd(void* rewardedVideoAdPtr) {
    RewardVideoAd* rewardedVideoAd = (__bridge RewardVideoAd*)rewardedVideoAdPtr;
    [rewardedVideoAd.rewardedVideoAd showAdFromRootViewController:GetAppController().rootViewController];
}

void UnionPlatform_RewardVideoAd_Dispose(void* rewardedVideoAdPtr) {
    (__bridge_transfer RewardVideoAd*)rewardedVideoAdPtr;
}

void UnionPlatform_RewardVideoAd_Again_SetInteractionListener(
    void* rewardedVideoAdPtr,
    RewardVideoAd_OnAdShow onAdShow,
    RewardVideoAd_OnAdVideoBarClick onAdVideoBarClick,
    RewardVideoAd_OnVideoComplete onVideoComplete,
    RewardVideoAd_OnVideoError onVideoError,
    RewardVideoAd_OnVideoSkip onVideoSkip,
    ExpressRewardVideoAd_OnRewardVerify onRewardVerify,
    int context) {
    RewardVideoAd* rewardedVideoAd = (__bridge RewardVideoAd*)rewardedVideoAdPtr;
    if (rewardedVideoAd.againObject) {
        rewardedVideoAd.againObject.onAdShow = onAdShow;
        rewardedVideoAd.againObject.onAdVideoBarClick = onAdVideoBarClick;
        rewardedVideoAd.againObject.onVideoComplete = onVideoComplete;
        rewardedVideoAd.againObject.onVideoError = onVideoError;
        rewardedVideoAd.againObject.onVideoSkip = onVideoSkip;
        rewardedVideoAd.againObject.onRewardVerify = onRewardVerify;
        rewardedVideoAd.againObject.interactionContext = context;
    }
}

bool UnionPlatform_rewardVideoMaterialMetaIsFromPreload(void* rewardedVideoAdPtr) {
    RewardVideoAd* rewardedVideoAd = (__bridge RewardVideoAd*)rewardedVideoAdPtr;
    BOOL preload = [rewardedVideoAd.rewardedVideoAd materialMetaIsFromPreload];
    return preload == YES;
}

long UnionPlatform_rewardVideoExpireTime(void * rewardedVideoAdPtr) {
    RewardVideoAd* rewardedVideoAd = (__bridge RewardVideoAd*)rewardedVideoAdPtr;
    return [rewardedVideoAd.rewardedVideoAd getExpireTimestamp];
}

#if defined (__cplusplus)
}
#endif
