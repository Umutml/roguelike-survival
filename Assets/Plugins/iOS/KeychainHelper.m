#import <Foundation/Foundation.h>
#import <Security/Security.h>

void SaveToKeychain(const char* key, const char* value)
{
    NSString *keyString = [NSString stringWithUTF8String:key];
    NSString *valueString = [NSString stringWithUTF8String:value];

    NSData *valueData = [valueString dataUsingEncoding:NSUTF8StringEncoding];

    NSDictionary *query = @{
        (__bridge id)kSecClass: (__bridge id)kSecClassGenericPassword,
        (__bridge id)kSecAttrAccount: keyString,
        (__bridge id)kSecValueData: valueData
    };

    SecItemDelete((__bridge CFDictionaryRef)query);
    SecItemAdd((__bridge CFDictionaryRef)query, NULL);
}

const char* LoadFromKeychain(const char* key)
{
    NSString *keyString = [NSString stringWithUTF8String:key];

    NSDictionary *query = @{
        (__bridge id)kSecClass: (__bridge id)kSecClassGenericPassword,
        (__bridge id)kSecAttrAccount: keyString,
        (__bridge id)kSecReturnData: @YES,
        (__bridge id)kSecMatchLimit: (__bridge id)kSecMatchLimitOne
    };

    CFTypeRef result = NULL;
    SecItemCopyMatching((__bridge CFDictionaryRef)query, &result);

    if (result == NULL) return NULL;

    NSData *valueData = (__bridge_transfer NSData *)result;
    NSString *valueString = [[NSString alloc] initWithData:valueData encoding:NSUTF8StringEncoding];

    return strdup([valueString UTF8String]);
}
