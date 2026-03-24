#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

void CustomOpenURL(const char* url) 
{
    NSString* urlString = [NSString stringWithUTF8String:url];
    NSURL* nsUrl = [NSURL URLWithString:urlString];
    [[UIApplication sharedApplication] openURL:nsUrl options:@{} completionHandler:nil];
}

float GetScaleScreen() 
{
    return (float)[[UIScreen mainScreen] scale];
}