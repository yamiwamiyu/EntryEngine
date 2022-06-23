//
//  BUToUnitySplashAd.cpp
//  Unity-iPhone
//
//  Created by wangchao on 2020/6/8.
//

#import <BUAdSDK/BUAdSDK.h>

#import "UnityAppController.h"
#import "BUToUnityAdManager.h"
#import "AdSlot.h"
const char* AutonomousStringCopy_Splash(const char* string)
{
    if (string == NULL) {
        return NULL;
    }
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}


// ISplashAdListener callbacks.
typedef void(*SplashAd_OnLoad)(void* splashAd, int context);
typedef void(*SplashAd_OnLoadError)(int errCode, const char* errMsg, int context);
typedef void(*SplashAd_WillVisible)(int context, int type);
typedef void(*SplashAd_DidClick)(int context, int type);
typedef void(*SplashAd_WillClose)(int context);
typedef void(*SplashAd_DidClose)(int context);
typedef void(*SplashAd_DidSkip)(int context);
typedef void(*SplashAd_DidCountdownToZero)(int context);
typedef void(*SplashAd_DidCloseOtherController)(int interactionType, int context);

@interface BUToUnitySplashAd : NSObject<BUSplashAdDelegate,BUAdObjectProtocol>

@property (nonatomic, strong) BUSplashAdView *splashAdView;

@property (nonatomic, assign) int loadContext;
@property (nonatomic, assign) int listenContext;

@property (nonatomic, assign) SplashAd_OnLoad onLoad;
@property (nonatomic, assign) SplashAd_OnLoadError onLoadError;
@property (nonatomic, assign) SplashAd_WillVisible willVisible;
@property (nonatomic, assign) SplashAd_DidClick didClick;
@property (nonatomic, assign) SplashAd_WillClose willClose;
@property (nonatomic, assign) SplashAd_DidClose didClose;
@property (nonatomic, assign) SplashAd_DidSkip didSkip;
@property (nonatomic, assign) SplashAd_DidCountdownToZero didCountdownToZero;
@property (nonatomic, assign) SplashAd_DidCloseOtherController didCloseOtherController;

@end


@implementation BUToUnitySplashAd

//+ (BUToUnitySplashAd *)sharedInstance {
//    static BUToUnitySplashAd *manager = nil;
//    static dispatch_once_t onceToken;
//    dispatch_once(&onceToken, ^{
//        if(!manager) {
//            manager = [[self alloc] init];
//        }
//    });
//    return manager;
//}

- (void)splashAdDidLoad:(BUSplashAdView *)splashAd {
//    NSLog(@"splashAd DidLoad");
    if (self.onLoad) {
        self.onLoad((__bridge void*)self, self.loadContext);
    }
}


- (void)splashAd:(BUSplashAdView *)splashAd didFailWithError:(NSError * _Nullable)error {
    if (self.onLoadError) {
        self.onLoadError((int)error.code, AutonomousStringCopy_Splash([[error localizedDescription] UTF8String]), self.loadContext);
    }
    [_splashAdView removeFromSuperview];
    _splashAdView = nil;
}


- (void)splashAdWillVisible:(BUSplashAdView *)splashAd {
//    NSLog(@"splashAd WillVisible");
    if (self.willVisible) {
        self.willVisible(self.listenContext, 0);
    }
}


- (void)splashAdDidClick:(BUSplashAdView *)splashAd {
    if (self.didClick) {
        self.didClick(self.listenContext, 0);
    }
}


- (void)splashAdDidClose:(BUSplashAdView *)splashAd {
    if (self.didClose) {
        self.didClose(self.listenContext);
    }
}


- (void)splashAdWillClose:(BUSplashAdView *)splashAd {
    if (self.willClose) {
        self.willClose(self.listenContext);
    }
}

/**
 This method is called when spalashAd skip button  is clicked.
 */
- (void)splashAdDidClickSkip:(BUSplashAdView *)splashAd {
    
    if (self.didSkip) {
        self.didSkip(self.listenContext);
    }
}

/**
 This method is called when spalashAd countdown equals to zero
 */
- (void)splashAdCountdownToZero:(BUSplashAdView *)splashAd {
    
    if (self.didCountdownToZero) {
        self.didCountdownToZero(self.listenContext);
    }
}

- (void)splashAdDidCloseOtherController:(BUSplashAdView *)splashAd interactionType:(BUInteractionType)interactionType {
    
//    NSLog(@"splashAd DidCloseOtherController");
    if (self.didCloseOtherController) {
        self.didCloseOtherController((int)interactionType, self.listenContext);
    }
}
- (id<BUAdClientBiddingProtocol>)adObject {
    return self.splashAdView;
}

#if defined (__cplusplus)
extern "C" {
#endif
    void* UnionPlatform_SplashAd_Load(
                                      AdSlotStruct *adSlot,
                                     int timeout,
                                     SplashAd_OnLoadError onLoadError,
                                     SplashAd_OnLoad onLoad,
                                     int context) {
        
        BUToUnitySplashAd* instance = [[BUToUnitySplashAd alloc] init];
        instance.loadContext = context;
        instance.onLoad = onLoad;
        instance.onLoadError = onLoadError;
        
        CGRect frame = CGRectMake(0, 0, CGRectGetWidth([UIScreen mainScreen].bounds), CGRectGetHeight([UIScreen mainScreen].bounds));
        BUAdSlot *slot = [BUAdSlot new];
        slot.ID = [NSString stringWithUTF8String:adSlot->slotId?:""];
        slot.AdType = BUAdSlotAdTypeSplash;
        slot.position = BUAdSlotPositionFullscreen;
        BUSize *size = [[BUSize alloc] init];
        size.width = frame.size.width * [[UIScreen mainScreen] scale];
        size.height = frame.size.height * [[UIScreen mainScreen] scale];
        slot.imgSize = size;
        slot.adloadSeq = 0;
        slot.primeRit = nil;
        if (adSlot->adLoadType != 0) {
           slot.adLoadType = (BUAdLoadType)[@(adSlot->adLoadType) integerValue];
        }
        instance.splashAdView = [[BUSplashAdView alloc] initWithSlot:slot frame:frame];
        instance.splashAdView.delegate = instance;
        
        if (timeout > 0) {
            instance.splashAdView.tolerateTimeout = timeout;
        }
        
        instance.splashAdView.rootViewController = GetAppController().rootViewController;
        [GetAppController().rootViewController.view addSubview:instance.splashAdView];
        
        [instance.splashAdView loadAdData];
        
        // 强持有，是引用加+1
        // [[BUToUnityAdManager sharedInstance] addAdManager:instance];
        
        return (__bridge_retained void*)instance;
    }
    
    void UnionPlatform_SplashAd_SetInteractionListener(void* splashAdPtr,
                                                            SplashAd_WillVisible willVisible,
                                                            SplashAd_DidClick didClick,
                                                            SplashAd_DidClose didClose,
                                                            SplashAd_DidSkip didSkip,
                                                            SplashAd_DidCountdownToZero didCountdownToZero,
                                                            int context) {
        BUToUnitySplashAd* splashAd = (__bridge BUToUnitySplashAd*)splashAdPtr;
        
        splashAd.listenContext = context;
        splashAd.willVisible = willVisible;
        splashAd.didClick = didClick;
        splashAd.didClose = didClose;
        splashAd.didSkip = didSkip;
        splashAd.didCountdownToZero = didCountdownToZero;
    }
    
    void UnionPlatform_SplashAd_Show (void* splashAdPtr) {
        
    }
    
    void UnionPlatform_SplashAd_Dispose(void* splashAdPtr) {
        
//        (__bridge_transfer BUToUnitySplashAd*)splashAd;
        dispatch_async(dispatch_get_main_queue(), ^{
            BUToUnitySplashAd *splashAd = (__bridge BUToUnitySplashAd*)splashAdPtr;
            [splashAd.splashAdView removeFromSuperview];
            splashAd.splashAdView = nil;
            (__bridge_transfer BUToUnitySplashAd*)splashAdPtr;
        });
        // [[BUToUnityAdManager sharedInstance] deleteAdManager:splashAd];
    
    }
    
#if defined (__cplusplus)
}
#endif
    
@end
