//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

#import <BUAdSDK/BUAdSDK.h>
#import "UnityAppController.h"
#import "AdSlot.h"
static const char* AutonomousStringCopy(const char* string);

static const char* AutonomousStringCopy(const char* string)
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
typedef void(*RewardVideoAd_OnRewardVerify)(
    bool rewardVerify, int rewardAmount, const char* rewardName, int rewardType, float rewardPropose, int context);

// The BURewardedVideoAdDelegate implement.
@interface ExpressRewardVideoAd : NSObject
@end

@interface ExpressRewardVideoAd () <BUNativeExpressRewardedVideoAdDelegate,BUAdObjectProtocol>
@property (nonatomic, strong) BUNativeExpressRewardedVideoAd *expressRewardedVideoAd;

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
@property (nonatomic, assign) RewardVideoAd_OnRewardVerify onRewardVerify;

@property (nonatomic, strong) ExpressRewardVideoAd *againObject;

@end

@implementation ExpressRewardVideoAd
#pragma mark - BUNativeExpressRewardedVideoAdDelegate
- (void)nativeExpressRewardedVideoAdDidLoad:(BUNativeExpressRewardedVideoAd *)rewardedVideoAd {
    if (self.onRewardVideoAdLoad) {
        self.onRewardVideoAdLoad((__bridge void*)self, self.loadContext);
    }
}

- (void)nativeExpressRewardedVideoAd:(BUNativeExpressRewardedVideoAd *)rewardedVideoAd didFailWithError:(NSError *_Nullable)error {
    if (self.onError) {
        self.onError((int)error.code, AutonomousStringCopy([[error localizedDescription] UTF8String]), self.loadContext);
    }
}

- (void)nativeExpressRewardedVideoAdDidDownLoadVideo:(BUNativeExpressRewardedVideoAd *)rewardedVideoAd {
    if (self.onRewardVideoCached) {
        self.onRewardVideoCached(self.loadContext);
    }
}

- (void)nativeExpressRewardedVideoAdViewRenderSuccess:(BUNativeExpressRewardedVideoAd *)rewardedVideoAd {
}

- (void)nativeExpressRewardedVideoAdViewRenderFail:(BUNativeExpressRewardedVideoAd *)rewardedVideoAd error:(NSError *_Nullable)error {
    if (self.onError) {
        self.onError((int)error.code, AutonomousStringCopy([[error localizedDescription] UTF8String]), self.loadContext);
    }
}

- (void)nativeExpressRewardedVideoAdWillVisible:(BUNativeExpressRewardedVideoAd *)rewardedVideoAd {
    if (self.onAdShow) {
        self.onAdShow(self.interactionContext);
    }
}

- (void)nativeExpressRewardedVideoAdDidVisible:(BUNativeExpressRewardedVideoAd *)rewardedVideoAd {
}

- (void)nativeExpressRewardedVideoAdWillClose:(BUNativeExpressRewardedVideoAd *)rewardedVideoAd {
}

- (void)nativeExpressRewardedVideoAdDidClose:(BUNativeExpressRewardedVideoAd *)rewardedVideoAd {
    if (self.onAdClose) {
        self.onAdClose(self.interactionContext);
    }
}

- (void)nativeExpressRewardedVideoAdDidClick:(BUNativeExpressRewardedVideoAd *)rewardedVideoAd {
    if (self.onAdVideoBarClick) {
        self.onAdVideoBarClick(self.interactionContext);
    }
}

- (void)nativeExpressRewardedVideoAdDidClickSkip:(BUNativeExpressRewardedVideoAd *)rewardedVideoAd {
    if (self.onVideoSkip) {
        self.onVideoSkip(self.interactionContext);
    }
}

- (void)nativeExpressRewardedVideoAdDidPlayFinish:(BUNativeExpressRewardedVideoAd *)rewardedVideoAd didFailWithError:(NSError *_Nullable)error {
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

- (void)nativeExpressRewardedVideoAdServerRewardDidSucceed:(BUNativeExpressRewardedVideoAd *)rewardedVideoAd verify:(BOOL)verify {
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

- (void)nativeExpressRewardedVideoAdServerRewardDidFail:(BUNativeExpressRewardedVideoAd *)rewardedVideoAd error:(NSError *)error {
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



- (id<BUAdClientBiddingProtocol>)adObject {
    return self.expressRewardedVideoAd;
}

@end

#if defined (__cplusplus)
extern "C" {
#endif

void UnionPlatform_ExpressRewardVideoAd_Load(
    AdSlotStruct *adSlot,
    RewardVideoAd_OnError onError,
    RewardVideoAd_OnRewardVideoAdLoad onRewardVideoAdLoad,
    RewardVideoAd_OnRewardVideoCached onRewardVideoCached,
    int context) {
    BURewardedVideoModel *model = [[BURewardedVideoModel alloc] init];
    model.userId = [[NSString alloc] initWithUTF8String:adSlot->userId?:""];
    model.extra = [[NSString alloc] initWithUTF8String:adSlot->mediaExtra?:""];
    
    BUNativeExpressRewardedVideoAd* expressRewardedVideoAd = [[BUNativeExpressRewardedVideoAd alloc] initWithSlotID:[[NSString alloc] initWithUTF8String:adSlot->slotId?:""] rewardedVideoModel:model];
    
    ExpressRewardVideoAd* instance = [[ExpressRewardVideoAd alloc] init];
    ExpressRewardVideoAd* again_instance = [[ExpressRewardVideoAd alloc] init];

    instance.expressRewardedVideoAd = expressRewardedVideoAd;
    instance.againObject = again_instance;

    instance.onError = onError;
    instance.onRewardVideoAdLoad = onRewardVideoAdLoad;
    instance.onRewardVideoCached = onRewardVideoCached;

    instance.loadContext = context;
    again_instance.loadContext = context;

    expressRewardedVideoAd.delegate = instance;
    expressRewardedVideoAd.rewardPlayAgainInteractionDelegate = again_instance;
    [expressRewardedVideoAd loadAdData];

    (__bridge_retained void*)instance;
}

void UnionPlatform_ExpressRewardVideoAd_SetInteractionListener(
    void* rewardedVideoAdPtr,
    RewardVideoAd_OnAdShow onAdShow,
    RewardVideoAd_OnAdVideoBarClick onAdVideoBarClick,
    RewardVideoAd_OnAdClose onAdClose,
    RewardVideoAd_OnVideoComplete onVideoComplete,
    RewardVideoAd_OnVideoError onVideoError,
    RewardVideoAd_OnVideoSkip onVideoSkip,
    RewardVideoAd_OnRewardVerify onRewardVerify,
    int context) {
    ExpressRewardVideoAd* rewardedVideoAd = (__bridge ExpressRewardVideoAd*)rewardedVideoAdPtr;
    rewardedVideoAd.onAdShow = onAdShow;
    rewardedVideoAd.onAdVideoBarClick = onAdVideoBarClick;
    rewardedVideoAd.onAdClose = onAdClose;
    rewardedVideoAd.onVideoComplete = onVideoComplete;
    rewardedVideoAd.onVideoError = onVideoError;
    rewardedVideoAd.onVideoSkip = onVideoSkip;
    rewardedVideoAd.onRewardVerify = onRewardVerify;
    rewardedVideoAd.interactionContext = context;
}

void UnionPlatform_ExpressRewardVideoAd_ShowRewardVideoAd(void* rewardedVideoAdPtr) {
    ExpressRewardVideoAd* rewardedVideoAd = (__bridge ExpressRewardVideoAd*)rewardedVideoAdPtr;
    [rewardedVideoAd.expressRewardedVideoAd showAdFromRootViewController:GetAppController().rootViewController];
}

void UnionPlatform_ExpressRewardVideoAd_Dispose(void* rewardedVideoAdPtr) {
    (__bridge_transfer ExpressRewardVideoAd*)rewardedVideoAdPtr;
}

void UnionPlatform_ExpressRewardVideoAd_Again_SetInteractionListener(
    void* rewardedVideoAdPtr,
    RewardVideoAd_OnAdShow onAdShow,
    RewardVideoAd_OnAdVideoBarClick onAdVideoBarClick,
    RewardVideoAd_OnVideoComplete onVideoComplete,
    RewardVideoAd_OnVideoError onVideoError,
    RewardVideoAd_OnVideoSkip onVideoSkip,
    RewardVideoAd_OnRewardVerify onRewardVerify,
    int context) {
    ExpressRewardVideoAd* rewardedVideoAd = (__bridge ExpressRewardVideoAd*)rewardedVideoAdPtr;
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
bool UnionPlatform_expressRewardVideoMaterialMetaIsFromPreload(void* rewardedVideoAdPtr) {
    ExpressRewardVideoAd* rewardedVideoAd = (__bridge ExpressRewardVideoAd*)rewardedVideoAdPtr;
    BOOL preload = [rewardedVideoAd.expressRewardedVideoAd materialMetaIsFromPreload];
    return preload == YES;
}

long UnionPlatform_expressRewardVideoExpireTime(void * rewardedVideoAdPtr) {
    ExpressRewardVideoAd* rewardedVideoAd = (__bridge ExpressRewardVideoAd*)rewardedVideoAdPtr;
    return [rewardedVideoAd.expressRewardedVideoAd getExpireTimestamp];
}
#if defined (__cplusplus)
}
#endif
