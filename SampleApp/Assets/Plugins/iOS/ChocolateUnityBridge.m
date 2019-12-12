//
//  VdopiaiOSBridge.m
//
//
//  Created by Sachin Patil on 02/08/17.
//
//

#import "ChocolateUnityBridge.h"
#import <ChocolatePlatform_SDK_Core/ChocolatePlatform_SDK_Core.h>


@interface ChocolateListener : NSObject <ChocolateAdDelegate> {
    NSString *unityListenerName; //class name in unity to send callbacks to
    ChocolateInterstitialAd *interstitial;
    ChocolateRewardedAd *rewarded;
}

-(void)makeUnityCallback:(NSString *)function withMessage:(NSString *)message;
-(NSString *)baseFuncName:(SEL)method;

-(void)loadInterstitial;
-(void)showInterstitial;
-(void)loadRewarded;
-(void)showRewarded:(ChocolateReward *)reward;

@end

@implementation ChocolateListener

- (id)initWithUnityListenerName:(NSString *)name {
    if(self = [super init]){
        unityListenerName = name;
        return self;
    }
    return nil;
}

-(void)makeUnityCallback:(NSString *)function withMessage:(NSString *)message {
    UnitySendMessage([unityListenerName cStringUsingEncoding:NSASCIIStringEncoding],
                     [function cStringUsingEncoding:NSASCIIStringEncoding],
                     [message cStringUsingEncoding:NSASCIIStringEncoding]);
}

-(NSString *)baseFuncName:(SEL)method {
    return [NSStringFromSelector(method) componentsSeparatedByString:@":"].firstObject;
}

#pragma mark - API bridge

-(void)loadInterstitial {
    interstitial = [[ChocolateInterstitialAd alloc] initWithDelegate:self];
    [interstitial load];
}

-(void)showInterstitial {
    [interstitial showFrom:[self visibleViewController:[UIApplication sharedApplication].keyWindow.rootViewController]];
}

-(void)loadRewarded {
    rewarded = [[ChocolateRewardedAd alloc] initWithDelegate:self];
    [rewarded load];
}

-(void)showRewarded:(ChocolateReward *)reward {
    rewarded.reward = reward;
    [rewarded showFrom:[self visibleViewController:[UIApplication sharedApplication].keyWindow.rootViewController]];
}

#pragma mark - ChocolateAdDelegate

-(void)onChocolateAdLoaded:(ChocolateAd *)ad {
    if(ad == interstitial) {
        [self makeUnityCallback:@"onInterstitialLoaded" withMessage:@""];
    } else if(ad == rewarded) {
        [self makeUnityCallback:@"onRewardLoaded" withMessage:@""];
    }
}

-(void)onChocolateAdLoadFailed:(ChocolateAd *)ad because:(ChocolateAdNoAdReason)reason {
    if(ad == interstitial) {
        [self makeUnityCallback:@"onInterstitialFailed" withMessage:[NSString stringWithFormat:@"%ld",reason]];
    } else if(ad == rewarded) {
        [self makeUnityCallback:@"onRewardFailed" withMessage:[NSString stringWithFormat:@"%ld",reason]];
    }
}

-(void)onChocolateAdShown:(ChocolateAd *)ad {
    if(ad == interstitial) {
        [self makeUnityCallback:@"onInterstitialShown" withMessage:@""];
    } else if(ad == rewarded) {
        [self makeUnityCallback:@"onRewardShown" withMessage:@""];
    }
}

-(void)onChocolateAdClosed:(ChocolateAd *)ad {
    if(ad == interstitial) {
        [self makeUnityCallback:@"onInterstitialDismissed" withMessage:@""];
    } else if(ad == rewarded) {
        [self makeUnityCallback:@"onRewardDismissed" withMessage:@""];
    }
}

//@optional
-(void)onChocolateAdClicked:(ChocolateAd *)ad {
    [self makeUnityCallback:@"onInterstitialClicked" withMessage:@""];
}

-(void)onChocolateAdFailedToStart:(ChocolateAd *)ad because:(ChocolateAdNoAdReason)reason {
    [self makeUnityCallback:@"onRewardFailed" withMessage:[NSString stringWithFormat:@"%ld",reason]];
}

-(void)onChocolateAdReward:(NSString *)rewardName amount:(NSInteger)rewardAmount {
    NSString *rewardInfo = [NSString stringWithFormat:@"%ld",rewardAmount];
    rewardInfo = [rewardInfo stringByAppendingString:@","];
    rewardInfo = [rewardInfo stringByAppendingString:rewardName];
    
    [self makeUnityCallback:@"onRewardFinished" withMessage:rewardInfo];
}

# pragma mark - helpers

- (UIViewController*) visibleViewController:(UIViewController *)rootViewController {
    if (rootViewController.presentedViewController == nil)
    {
        return rootViewController;
    }
    if ([rootViewController.presentedViewController isKindOfClass:[UINavigationController class]])
    {
        UINavigationController *navigationController = (UINavigationController *)rootViewController.presentedViewController;
        UIViewController *lastViewController = [[navigationController viewControllers] lastObject];
        
        return  [self visibleViewController:lastViewController];
    }
    if ([rootViewController.presentedViewController isKindOfClass:[UITabBarController class]])
    {
        UITabBarController *tabBarController = (UITabBarController *)rootViewController.presentedViewController;
        UIViewController *selectedViewController = tabBarController.selectedViewController;
        
        return [self visibleViewController:selectedViewController ];
    }
    
    UIViewController *presentedViewController = (UIViewController *)rootViewController.presentedViewController;
    
    return [self visibleViewController:presentedViewController];
}
    
@end


static ChocolateListener *listener;


void _initWithAPIKey(char *apiKey) {
    [ChocolatePlatform initWithAdUnitID:[NSString stringWithCString:apiKey encoding:NSASCIIStringEncoding]];
}

void _setupWithListener(char *listenerName) {
    listener = [[ChocolateListener alloc] initWithUnityListenerName:[NSString stringWithCString:listenerName encoding:NSASCIIStringEncoding]];
}

void _loadInterstitialAd(void) {
    [listener loadInterstitial];
}

void _showInterstitialAd(void) {
    [listener showInterstitial];
}

void _loadRewardAd(void) {
    
}

void _showRewardAd(int rewardAmount,char* rewardName, char* userId, char* secretKey) {
    ChocolateReward *rew = [ChocolateReward blankReward];
    rew.rewardName = [NSString stringWithCString:rewardName encoding:NSASCIIStringEncoding];
    rew.rewardAmount = rewardAmount;
    rew.userID = [NSString stringWithCString:userId encoding:NSASCIIStringEncoding];
    rew.secretKey = [NSString stringWithCString:secretKey encoding:NSASCIIStringEncoding];
    [listener showRewarded:rew];
}

void _setDemograpics(int age, char* birthDate, char* gender, char* maritalStatus,
                     char* ethnicity) {
    ChocolatePlatformDemographics *dem = [ChocolatePlatform demographics];
    dem.age = age;
    
    NSArray<NSString*> *comps = [[NSString stringWithCString:birthDate encoding:NSASCIIStringEncoding] componentsSeparatedByString:@"-"];
    if(comps.count >= 3){
        NSInteger year = [comps[0] integerValue];
        NSInteger month = [comps[1] integerValue];
        NSInteger day = [comps[2] integerValue];
        if(year != 0 && month != 0 && day != 0){
            [dem setBirthdayYear:year month:month day:day];
        }
    }
    
    NSString *msNS = [NSString stringWithCString:maritalStatus encoding:NSASCIIStringEncoding];
    if([msNS rangeOfString:@"single" options:NSCaseInsensitiveSearch].location != NSNotFound){
        dem.maritalStatus = ChocolatePlatformMaritalStatusSingle;
    }else if([msNS rangeOfString:@"married" options:NSCaseInsensitiveSearch].location != NSNotFound){
        dem.maritalStatus = ChocolatePlatformMaritalStatusMarried;
    }else if([msNS rangeOfString:@"divorced" options:NSCaseInsensitiveSearch].location != NSNotFound){
        dem.maritalStatus = ChocolatePlatformMaritalStatusDivorced;
    }else if([msNS rangeOfString:@"widowed" options:NSCaseInsensitiveSearch].location != NSNotFound){
        dem.maritalStatus = ChocolatePlatformMaritalStatusWidowed;
    }else if([msNS rangeOfString:@"separated" options:NSCaseInsensitiveSearch].location != NSNotFound){
        dem.maritalStatus = ChocolatePlatformMaritalStatusSeparated;
    }else if([msNS rangeOfString:@"other" options:NSCaseInsensitiveSearch].location != NSNotFound){
        dem.maritalStatus = ChocolatePlatformMaritalStatusOther;
    }
    
    NSString *genderString = [NSString stringWithCString:gender encoding:NSASCIIStringEncoding];
    
    NSRange maleRange = [genderString rangeOfString:@"Male" options:NSCaseInsensitiveSearch];
    NSRange feMaleRange = [genderString rangeOfString:@"Female" options:NSCaseInsensitiveSearch];
    
    if (maleRange.location != NSNotFound){
        dem.gender = ChocolatePlatformGenderMale;
    }else if (feMaleRange.location != NSNotFound){
        dem.gender = ChocolatePlatformGenderFemale;
    }else if (feMaleRange.location != NSNotFound){
        dem.gender = ChocolatePlatformGenderOther;
    }

    dem.ethnicity = [NSString stringWithCString:ethnicity encoding:NSASCIIStringEncoding];
}

void _setLocation(char* dmaCode, char* postal, char* curPostal, char* latitude, char* longitude) {
    ChocolatePlatformLocation *loc = [ChocolatePlatform location];
    
    if(dmaCode){
        loc.dmacode = [NSString stringWithCString:dmaCode encoding:NSASCIIStringEncoding];
    }
    if(postal){
        loc.postalcode = [NSString stringWithCString:postal encoding:NSASCIIStringEncoding];
    }
    if(curPostal){
        loc.currpostal = [NSString stringWithCString:curPostal encoding:NSASCIIStringEncoding];
    }
    
    if(latitude && longitude){
        NSString *latString = [NSString stringWithCString:latitude encoding:NSASCIIStringEncoding];
        NSString *lonString = [NSString stringWithCString:longitude encoding:NSASCIIStringEncoding];
        CLLocation *LocationAtual = [[CLLocation alloc] initWithLatitude:[latString floatValue] longitude:[lonString floatValue]];
    
    
        loc.location = LocationAtual;
    }
    
}

void _setAppInfo(char* appName, char* pubName,
                 char* appDomain, char* pubDomain,
                 char* storeUrl, char* iabCategory) {
    ChocolatePlatformAppInfo *ai = [ChocolatePlatform appInfo];
    
    if(appName){
        ai.appName = [NSString stringWithCString:appName encoding:NSASCIIStringEncoding];
    }
    if(pubName){
        ai.requester = [NSString stringWithCString:pubName encoding:NSASCIIStringEncoding];
    }
    if(appDomain){
        ai.appDomain = [NSString stringWithCString:appDomain encoding:NSASCIIStringEncoding];
    }
    if(pubDomain){
        ai.publisherdomain = [NSString stringWithCString:pubDomain encoding:NSASCIIStringEncoding];
    }
    if(storeUrl){
        ai.appStoreUrl = [NSString stringWithCString:storeUrl encoding:NSASCIIStringEncoding];
    }
    if(iabCategory){
        ai.Category = [NSString stringWithCString:iabCategory encoding:NSASCIIStringEncoding];
    }
}

void _setPrivacySettings(_Bool gdprApplies, char* gdprConsentString) {
    NSString *consentString = nil;
    if(gdprConsentString){
        consentString = [NSString stringWithCString:gdprConsentString encoding:NSASCIIStringEncoding];
    }
    
    [[ChocolatePlatform privacySettings]
     subjectToGDPR:gdprApplies
     withConsent:consentString];
}

#pragma mark - custom segment properties

void _setCustomSegmentProperty(char* key, char* value) {
    NSString *pKey = [NSString stringWithCString:key encoding:NSASCIIStringEncoding];
    NSString *pVal = [NSString stringWithCString:value encoding:NSASCIIStringEncoding];
    [ChocolatePlatformCustomSegmentProperties
     setCustomSegmentProperty:pKey with:pVal];
}

const char* _Nullable _getCustomSegmentProperty(char* key) {
    NSString *pKey = [NSString stringWithCString:key encoding:NSASCIIStringEncoding];
    NSString *pVal = [ChocolatePlatformCustomSegmentProperties getCustomSegmentProperty:pKey];
    return [pVal cStringUsingEncoding:NSASCIIStringEncoding];
}

const char* _Nullable _getAllCustomSegmentProperties(void) {
    NSDictionary *props = [ChocolatePlatformCustomSegmentProperties getAllCustomSegmentProperties];
    if(props.count <= 0){
        return NULL;
    }
    
    NSData *jsonRep = [NSJSONSerialization dataWithJSONObject:props options:0 error:nil];
    NSString *jsonString = [[NSString alloc] initWithData:jsonRep encoding:NSUTF8StringEncoding];
    
    return [jsonString cStringUsingEncoding:NSASCIIStringEncoding];
}


void _deleteCustomSegmentProperty(char* key) {
    NSString *pKey = [NSString stringWithCString:key encoding:NSASCIIStringEncoding];

    [ChocolatePlatformCustomSegmentProperties deleteCustomSegmentProperty:pKey];
}

void _deleteAllCustomSegmentProperties(void) {
    [ChocolatePlatformCustomSegmentProperties deleteAllCustomSegmentProperties];
}
