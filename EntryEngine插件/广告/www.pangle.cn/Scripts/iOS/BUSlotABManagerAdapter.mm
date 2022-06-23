//
//  ClientBidManager.m
//  UnityFramework
//
//  Created by yujie on 2021/10/21.
//

#import <BUAdSDK/BUAdSDK.h>
#if defined (__cplusplus)
extern "C" {
#endif
typedef void(*BUSlotABManager_FetchSlotComplete)(const char* slotId, int type, int errCode,const char* message);
void UnionPlatform_BUSlotABManagerFetchSlotWithCodeGroupId(int32_t codeGroupId,BUSlotABManager_FetchSlotComplete complete) {
    [[BUSlotABManager sharedInstance] fetchSlotWithCodeGroupId:codeGroupId completion:^(NSString * _Nullable slotId, BUAdSlotAdType slotType, NSError * _Nullable error) {
        if (complete) {
            complete([slotId UTF8String],
                     (int)slotType,
                     error? (int)error.code:0,
                     error?[error.localizedDescription UTF8String]:"");
        }
    }];
}


#if defined (__cplusplus)
}
#endif
