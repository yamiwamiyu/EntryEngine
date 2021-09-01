#import <BUAdSDK/BUAdSDK.h>
#if defined (__cplusplus)
extern "C" {
#endif
typedef void(*PangleInitializeCallBack)(bool success, const char* message); 

void UnionPlatform_PangleInitializeSDK(PangleInitializeCallBack callback){
    [BUAdSDKManager startWithSyncCompletionHandler:^(BOOL success, NSError *error) {
        if (callback) {
            callback(success,[error.userInfo[@"message"]?:@"" cStringUsingEncoding:NSUTF8StringEncoding]);
        }
    }];
}
#if defined (__cplusplus)
}
#endif
