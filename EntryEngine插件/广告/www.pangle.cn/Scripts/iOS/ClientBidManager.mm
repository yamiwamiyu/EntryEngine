//
//  ClientBidManager.m
//  UnityFramework
//
//  Created by yujie on 2021/10/21.
//

#import <BUAdSDK/BUAdSDK.h>
#import "BUAdObjectProtocol.h"
#if defined (__cplusplus)
extern "C" {
#endif

id<BUAdClientBiddingProtocol> getAdObject(void* adPtr) {
    id tempAdObj = (__bridge id)adPtr;
    if ([tempAdObj conformsToProtocol:@protocol(BUAdClientBiddingProtocol)]) {
        return tempAdObj;
    }
    
    id<BUAdObjectProtocol> ad = (__bridge id)adPtr;
    if ([ad respondsToSelector:@selector(adObject)]) {
        id<BUAdClientBiddingProtocol> adObject = [ad adObject];
        if ([adObject conformsToProtocol:@protocol(BUAdClientBiddingProtocol)]) {
            return adObject;
        }
    }
    return nil;
}

void UnionPlatform_ClientBidSetPrice(void* adPtr,double price) {
    id<BUAdClientBiddingProtocol> ad = getAdObject(adPtr);
    if ([ad respondsToSelector:@selector(setPrice:)]) {
        [ad setPrice:@(price)];
    }
}

void UnionPlatform_ClientBidWin(void* adPtr,double price) {
    id<BUAdClientBiddingProtocol> ad = getAdObject(adPtr);
    if ([ad respondsToSelector:@selector(win:)]) {
        [ad win:@(price)];
    }
}

void UnionPlatform_ClientBidSetLoss(void* adPtr,double price,const char* reason, const char* bidder) {
    id<BUAdClientBiddingProtocol> ad = getAdObject(adPtr);
    if ([ad respondsToSelector:@selector(loss:lossReason:winBidder:)]) {
        [ad loss:@(price) lossReason:[NSString stringWithUTF8String:reason] winBidder:[NSString stringWithUTF8String:bidder]];
    }
}

#if defined (__cplusplus)
}
#endif
