#import <BUAdSDK/BUAdSDK.h>
#if defined (__cplusplus)
extern "C" {
#endif
typedef void(*PangleInitializeCallBack)(bool success, const char* message); 

void UnionPlatform_PangleInitializeSDK(PangleInitializeCallBack callback){
    
    NSString *userExtData = [BUAdSDKConfiguration configuration].userExtData;
    NSMutableString *string = [[NSMutableString alloc]initWithString:([userExtData isKindOfClass:[NSString class]] && userExtData.length>2)?userExtData:@"[]"];
    if (string.length > 2) {
        [string insertString:@"," atIndex:string.length-1];
    }
    [string insertString:@"{\"name\":\"unity_version\",\"value\":\"3.7.0.0\"}" atIndex:string.length-1];
    [BUAdSDKConfiguration configuration].userExtData = string.copy;

    [BUAdSDKManager startWithSyncCompletionHandler:^(BOOL success, NSError *error) {
        if (callback) {
            callback(success,[error.userInfo[@"message"]?:@"" cStringUsingEncoding:NSUTF8StringEncoding]);
        }
    }];
}

int UnionPlatform_PangleGetCoppa() {
    return (int)[BUAdSDKManager coppa];
}

void UnionPlatform_PangleSetCoppa(int coppa) {
    [BUAdSDKManager setCoppa:coppa];
}

#if defined (__cplusplus)
}
#endif
