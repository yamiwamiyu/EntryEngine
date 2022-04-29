//
//  BUToUnityExpressSplashAd.cpp
//  Unity-iPhone
//
//  Created by wangchao on 2020/6/8.
//

#import <BUAdSDK/BUAdSDK.h>

#import "UnityAppController.h"
#import "BUToUnityAdManager.h"


const char* AutonomousStringCopy_ExpressSplash(const char* string)
{
    if (string == NULL) {
        return NULL;
    }
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}


// ISplashAdListener callbacks.
typedef void(*ExpressSplashAd_OnLoad)(void* splashAd, int context);
typedef void(*ExpressSplashAd_OnLoadError)(int errCode, const char* errMsg, int context);
typedef void(*ExpressSplashAd_WillVisible)(int context, int type);
typedef void(*ExpressSplashAd_DidClick)(int context, int type);
typedef void(*ExpressSplashAd_WillClose)(int context);
typedef void(*ExpressSplashAd_DidClose)(int context);
typedef void(*ExpressSplashAd_DidSkip)(int context);
typedef void(*ExpressSplashAd_DidCountdownToZero)(int context);
typedef void(*ExpressSplashAd_DidCloseOtherController)(int interactionType, int context);

@interface BUToUnityExpressSplashAd : NSObject<BUNativeExpressSplashViewDelegate>

@property (nonatomic, strong) BUNativeExpressSplashView *splashAdView;

@property (nonatomic, assign) int loadContext;
@property (nonatomic, assign) int listenContext;

@property (nonatomic, assign) ExpressSplashAd_OnLoad onLoad;
@property (nonatomic, assign) ExpressSplashAd_OnLoadError onLoadError;
@property (nonatomic, assign) ExpressSplashAd_WillVisible willVisible;
@property (nonatomic, assign) ExpressSplashAd_DidClick didClick;
@property (nonatomic, assign) ExpressSplashAd_WillClose willClose;
@property (nonatomic, assign) ExpressSplashAd_DidClose didClose;
@property (nonatomic, assign) ExpressSplashAd_DidSkip didSkip;
@property (nonatomic, assign) ExpressSplashAd_DidCountdownToZero didCountdownToZero;
@property (nonatomic, assign) ExpressSplashAd_DidCloseOtherController didCloseOtherController;

@end


@implementation BUToUnityExpressSplashAd


/**
 This method is called when splash ad material loaded successfully.
 */
- (void)nativeExpressSplashViewDidLoad:(BUNativeExpressSplashView *)splashAdView {
    
    if (self.onLoad) {
        self.onLoad((__bridge void*)self, self.loadContext);
    }
}

/**
 This method is called when splash ad material failed to load.
 @param error : the reason of error
 */
- (void)nativeExpressSplashView:(BUNativeExpressSplashView *)splashAdView didFailWithError:(NSError * _Nullable)error {
    
    if (self.onLoadError) {
        self.onLoadError((int)error.code, AutonomousStringCopy_ExpressSplash([[error localizedDescription] UTF8String]), self.loadContext);
    }

    [_splashAdView removeFromSuperview];
    _splashAdView = nil;
}

/**
 This method is called when rendering a nativeExpressAdView successed.
 */
- (void)nativeExpressSplashViewRenderSuccess:(BUNativeExpressSplashView *)splashAdView {
    
}

/**
 This method is called when a nativeExpressAdView failed to render.
 @param error : the reason of error
 */
- (void)nativeExpressSplashViewRenderFail:(BUNativeExpressSplashView *)splashAdView error:(NSError * __nullable)error {
    
}

/**
 This method is called when nativeExpressSplashAdView will be showing.
 */
- (void)nativeExpressSplashViewWillVisible:(BUNativeExpressSplashView *)splashAdView {
    
    if (self.willVisible) {
        self.willVisible(self.listenContext, 0);
    }
}

/**
 This method is called when nativeExpressSplashAdView is clicked.
 */
- (void)nativeExpressSplashViewDidClick:(BUNativeExpressSplashView *)splashAdView {
    
    if (self.didClick) {
        self.didClick(self.listenContext, 0);
    }
}

/**
 This method is called when nativeExpressSplashAdView's skip button is clicked.
 */
- (void)nativeExpressSplashViewDidClickSkip:(BUNativeExpressSplashView *)splashAdView {
    
    if (self.didSkip) {
        self.didSkip(self.listenContext);
    }
}
/**
 This method is called when nativeExpressSplashAdView countdown equals to zero
 */
- (void)nativeExpressSplashViewCountdownToZero:(BUNativeExpressSplashView *)splashAdView {
    
    if (self.didCountdownToZero) {
        self.didCountdownToZero(self.listenContext);
    }
}

/**
 This method is called when nativeExpressSplashAdView closed.
 */
- (void)nativeExpressSplashViewDidClose:(BUNativeExpressSplashView *)splashAdView {
    
    if (self.didClose) {
        self.didClose(self.listenContext);
    }
}

/**
 This method is called when when video ad play completed or an error occurred.
 */
- (void)nativeExpressSplashViewFinishPlayDidPlayFinish:(BUNativeExpressSplashView *)splashView didFailWithError:(NSError *)error {
    
}

/**
 This method is called when another controller has been closed.
 @param interactionType : open appstore in app or open the webpage or view video ad details page.
 */
- (void)nativeExpressSplashViewDidCloseOtherController:(BUNativeExpressSplashView *)splashView interactionType:(BUInteractionType)interactionType {
    
    if (self.didCloseOtherController) {
        self.didCloseOtherController((int)interactionType, self.listenContext);
    }
}


#if defined (__cplusplus)
extern "C" {
#endif
    void* UnionPlatform_ExpressSplashAd_Load(
                                            const char* slotID,
                                            int time,
                                            ExpressSplashAd_OnLoadError onLoadError,
                                            ExpressSplashAd_OnLoad onLoad,
                                     int context) {
        
        BUToUnityExpressSplashAd* instance = [[BUToUnityExpressSplashAd alloc] init];
        instance.loadContext = context;
        instance.onLoad = onLoad;
        instance.onLoadError = onLoadError;
        
        instance.splashAdView = [[BUNativeExpressSplashView alloc] initWithSlotID:[NSString stringWithUTF8String:slotID] adSize:[UIScreen mainScreen].bounds.size rootViewController:GetAppController().rootViewController];
        instance.splashAdView.delegate = instance;
        
        if (time > 0) {
            instance.splashAdView.tolerateTimeout = time;
        }
        [GetAppController().rootViewController.view addSubview:instance.splashAdView];

        [instance.splashAdView loadAdData];
        
        // 强持有，是引用加+1
        // [[BUToUnityAdManager sharedInstance] addAdManager:instance];
        
        return (__bridge_retained void*)instance;
    }
    
    void UnionPlatform_ExpressSplashAd_SetInteractionListener(void* splashAdPtr,
                                                            ExpressSplashAd_WillVisible willVisible,
                                                            ExpressSplashAd_DidClick didClick,
                                                            ExpressSplashAd_DidClose didClose,
                                                            ExpressSplashAd_DidSkip didSkip,
                                                            ExpressSplashAd_DidCountdownToZero didCountdownToZero,
                                                            int context) {
        BUToUnityExpressSplashAd* splashAd = (__bridge BUToUnityExpressSplashAd*)splashAdPtr;
        
        splashAd.listenContext = context;
        splashAd.willVisible = willVisible;
        splashAd.didClick = didClick;
        splashAd.didClose = didClose;
        splashAd.didSkip = didSkip;
        splashAd.didCountdownToZero = didCountdownToZero;
    }
    
    void UnionPlatform_ExpressSplashAd_Show (void* splashAdPtr) {
        
    }
    
    void UnionPlatform_ExpressSplashAd_Dispose(void* splashAdPtr) {
        dispatch_async(dispatch_get_main_queue(), ^{
            BUToUnityExpressSplashAd *splashAd = (__bridge BUToUnityExpressSplashAd*)splashAdPtr;
            [splashAd.splashAdView removeFromSuperview];
            splashAd.splashAdView = nil;
            (__bridge_transfer BUToUnityExpressSplashAd*)splashAdPtr;
        });
        // [[BUToUnityAdManager sharedInstance] deleteAdManager:splashAd];
    }
    
#if defined (__cplusplus)
}
#endif
    
@end
