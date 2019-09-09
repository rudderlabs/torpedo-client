#import "RudderCarierInfo.h"
#import <CoreTelephony/CTTelephonyNetworkInfo.h>
#import <CoreTelephony/CTCarrier.h>

const char * _GetiOSCarrierName()
{
    CTTelephonyNetworkInfo *netinfo = [[CTTelephonyNetworkInfo alloc] init];
    if (@available(iOS 12.0, *)) {
        NSDictionary *info = netinfo.serviceSubscriberCellularProviders;
        return [[info valueForKey:@"serviceSubscriberCellularProvider"] UTF8String];
    } else {
        CTCarrier *carrier = [netinfo subscriberCellularProvider];
        return [[carrier carrierName] UTF8String];
    }
}
