# Changelog
All notable changes to SayKit for Unity will be documented in this file.

## 2025-11-19
### Added
- [iOS][17.5] placement value to extra2 param in interstitial_close event.
### Refactored
- [iOS][17.5] IDFA manager.
### Removed
- [iOS][17.5] unused params in SayKitConfig.

## 2025-11-18
### Added
- [Android][17.2] placement value to extra2 param in interstitial_close event.
- [Android][17.2] Facebook auto app event logging.
- [Android][17.2] details to missingPaymentProvider error for billing service.
### Fixed
- [Android][17.2] logic for updating remote config with pretty flag.
- [Android][17.2] attachments for the support page.

## 2025-11-17
### Added
- [Unity] signature field to SKPurchasedProduct.

## 2025-11-13
### Added 
- [Unity] check remote settings on pre build.

## 2025-11-06
### Added
- [iOS][17.4] say id improvements.

## 2025-10-31
### Fixed
- [Unity] runtimeInfo.

## 2025-10-30
### Added
- [Unity][iOS][17.3][Android][17.1] optional extra param to PurchaseProduct method.
- [Unity][iOS][17.3] say id.
- [iOS][17.3] new params to AdInfo.
### Refactored
- [iOS][17.3] logic for sending events with sdk version.
### Fixed
- [iOS][17.3] purchase params in saypay.

## 2025-10-28
### Updated
- [iOS][17.0][Android][17.0] networks and dependencies.
### Added
- [Android][17.0] feature toggle strict_verification_enabled.

## 2025-10-23
### Added
- [Unity][iOS][16.17][Android][16.12] local notification.
- [Unity][iOS][16.17][Android][16.12] InPlay.
### Fixed
- [Android][16.12] free trial subscription.

## 2025-10-23
### Added 
- [Unity] cross promo improvements.

## 2025-10-22
### Refactored
- [Unity] models serialization.
### Removed
- [Unity] warnings.

## 2025-10-20
### Added
- [Unity] GetServerTimestamp method.
- [Android][16.11] option to init mediation lazily.
### Updated
- [Android][16.11] cleanup logic for old configs.
### Fixed
- [iOS][16.14] crash set mutation.

## 2025-10-16
### Added
- [Android][16.10][iOS][16.13] multi ad unit dynamic change.
- [Android][16.10][iOS][16.13] config replacement unification.
- [iOS][16.13] toggle strict_verification_enabled.
### Fixed
- [Android][16.10] reloading lite/post_bid ad_unit.
- [Android][16.10] purchase error after minimizing app.

## 2025-10-13
### Updated
- [Android][16.9] Adjust to 5.4.4.

## 2025-10-10
### Updated
- [Unity][Android][16.8][iOS][16.12] timer interstitial popup.
### Fixed
- [Android][16.8] applying insets to interstitial counter view.
### Refactored
- [iOS][16.12] SayKitUtils.

## 2025-10-07
### Added
- [Android][16.7] new params to AdInfo.

## 2025-10-07
### Changed
- [iOS][16.10] access to iOS repositories from git to https.
### Fixed
- [iOS][16.10] sending iap_ios event before the verification.
- [iOS][16.9] invalid products values on second fetch.

## 2025-09-29
### Fixed
- [iOS][16.8] GetPurchasedProducts method.

## 2025-09-25
### Fixed
- [iOS][16.7] extra parameters in iap events
- [Android][16.6] extra parameters in iap events

## 2025-09-18
### Added
- [iOS][16.6] custom system alert in case of unsuccessful web purchase.

## 2025-09-18
### Updated
- [iOS][16.5] error messages for some fetch products cases.

## 2025-09-16
### Added
- [Android][16.5] feature toggle to remove Double Interstitital UI.
- [Android][16.5] display time in param2 for close events.
### Fixed
- [Android][16.5] crash in analytics storage.
- [Android][16.5] anr in asset manager.

## 2025-09-16
### Fixed
- [Unity] SayKitBridgeEditor logs.

## 2025-09-12
### Changed
- [Android][16.4] transactionId value in SKPurchasedProduct.

## 2025-09-12
### Added
- [iOS][16.4] feature toggle to remove Double Interstitital UI.

## 2025-09-10
### Fixed
- [Unity] copying notification icon.

## 2025-09-09
### Added
- [Unity] support Unity Purchasing library 5.0+ ( trackOrderPurchase, trackOrderPurchaseOffer methods)

## 2025-09-08
### Updated
- [Android][16.3] AppLovin version to 13.2.0 (downgrade)

## 2025-09-03
### Fixed
- [iOS][16.3] double fetch products on launch.
### Added
- [iOS][16.3] disabling multi ad unit v2.1 by default.

## 2025-09-01
### Fixed
- [Unity] InAppManager for Unity IAP 5+ version.

## 2025-08-28
### Modified
- [Android][16.2] activity themes for chartboost and moloco.

## 2025-08-27
### Added
- [Android][16.1] custom themes to iron source's activities.

## 2025-08-26
### Changed
- [Unity][Android][16.0] resizeable activity attribute.

## 2025-08-25
### Fixed
- [Unity][Android][16.0] onNonConfirmedPurchaseReceived callback.

## 2025-08-22
### Added
- [Unity][Android][16.0][iOS][16.2] billing.
- [Android][16.0] IIR v4.
- [Unity][Android][16.0] support for Android 15 (API level 35).
- [Android][16.0] multi ad unit 2.1.
- [Android][16.0] support 16 KB native library alignment.
- [Android][16.0] Line network.
### Updated
- [Android][16.0] frameworks and networks.
### Refactored
- [Android][16.0] init logic to avoid fake sessions.
### Renamed
- [Android][16.0] runtime.sk_ofw_type.

## 2025-08-20
### Added
- [Unity] IsBannerEnabled method.
### Removed
- [Unity][iOS][15.2] 16.3 and 16.4 xcode versions from blacklist.

## 2025-08-19
### Added
- [iOS][15.2] pod ‘Adjust/AdjustGoogleOdm’ for attribution improvement.
- [iOS][15.2] IIR v4.
- [iOS][15.2] multi ad unit 2.1.
### Changed
- [iOS][15.2] default toggle value for sk_lazy_mediation.

## 2025-08-12
### Added 
- [Unity] facebook event logging by default.

## 2025-08-11
### Fixed
- [iOS][15.1] ssl for events.

## 2025-08-07
### Added
- [iOS][15.0] display time in param2 for close events.
### Updated
- [iOS][15.0] frameworks and networks.
### Removed
- [iOS][15.0] old configs at init start.

## 2025-07-30
### Added
- [Unity][Android][14.6] sk_google_license_key to strings.xml.

## 2025-07-30
### Added
- [iOS][13.23] runtime.disable_ads_internet_check logic to maxbannermanager.
### Renamed
- [iOS][13.23] runtime.sk_ofw_type.

## 2025-07-30
### Added
- [Android][14.6] analytics improvements.
### Refactored
- [Android][14.6] analytics.

## 2025-07-29
### Added
- [Unity] OpenCustomUrl method.

## 2025-07-16
### Updated
- [iOS][13.21][Android][14.5] Facebook Audience, Adjust, Firebase.
### Added
- [iOS][13.21] analytics improvements.

## 2025-07-14
### Removed
- [Unity] external dependency manager.

## 2025-07-10
### Added
- [Unity] extend trackPurchase and trackPurchaseOffer methods.

## 2025-07-09
### Fixed
- [iOS][13.20] support crash.
- [Android][14.4] loading banners when premium state changed.
### Added
- [iOS][13.20][Android][14.4] offerwall.
- [iOS][13.20][Android][14.4] unify ad_unit param in all events.

## 2025-07-08
### Fixed
- [iOS][13.20] support crash.
- [Android][14.4] loading banners when premium state changed.
### Added
- [iOS][13.20][Android][14.4] offerwall.
- [iOS][13.20][Android][14.4] unify ad_unit param in all events.

## 2025-07-08
### Added 
- [Unity] canvasSortingOrder to SayKit config.
- [Unity] supports scripting settings API for Unity 6000.
- [Unity] using saykit.androidlib for Unity 2020+.
### Fixed 
- [Unity] missing spaces in AndroidManifest.
### Changed
- [Unity] AddBuildProperty to SetBuildProperty for ENABLE_BITCODE setting.

## 2025-07-03
### Fixed 
- [Android][14.3] OOR crash.

## 2025-07-01
### Updated 
- [iOS][13.17] BigoAds adapter to version 4.8.0.0

## 2025-06-27
### Added
- [Unity][Android][14.2][iOS][13.16] callback for support request submission.
- [Android][14.2] double interstitial.
### Removed
- [Android][14.2][iOS][13.16] events interstitial_tag and rewarded_tag from trackPlacement logic.
### Removed
- [iOS][13.16] feature toggle default value for context menu.

## 2025-06-24
### Fixed
- [Android][14.1] localization deserialization error.

## 2025-06-18
### Added 
- [iOS][13.15] double interstitial. 

## 2025-06-17
### Fixed
- [Android][14.0] live attribution conditions.
### Updated 
- [Android][14.0] networks and frameworks.
### Removed 
- [Android][14.0] google-ad-manager-adapter.

## 2025-06-17
### Added 
- [iOS][13.14] Line network.
### Updated 
- [iOS][13.14] Firebase and Google related dependancies.

## 2025-06-12
### Added
- [Unity] support Unity 6000.

## 2025-06-10
### Fixed
- [Unity] RegisterURLSchemes method.

## 2025-06-09
### Added 
- [Unity][Android][13.4][iOS][13.13] localization imrovements.
- [iOS][13.13][Android][13.4] context field validation.

## 2025-06-06
### Added 
- [Unity] Xcode versions 16.3 and 16.4 to blacklist.

## 2025-06-04
### Added 
- [Unity][Android][13.2][iOS][13.12] support deep link. 
- [Unity][Android][13.2][iOS][13.12] SetFirebaseUserId method.
- [Android][13.2] skip timer works after interstitial_failed.

## 2025-05-29
### Fixed
- [Android][13.1] ShowMaxMediationDebug api call without mediation debug checkmark.

## 2025-05-27
### Migrated 
- [Android][13.0] applovin initialization to use new api.
### Updated 
- [Android][13.0] ads error extra format.
### Added 
- [Android][13.0] unit type to events extra.
- [Android][13.0] banner floors logic v2.
- [Android][13.0] show video ad from different units.
- [Android][13.0] isolating usage of mediation logic.
- [Android][13.0] extract mediation interface.
### Fixed
- [Android][13.0] interstitial skip rules.
- [Android][13.0] directories initialization deadlock.
### Refactored 
- [Android][13.0] ads manager.

## 2025-05-23
### Added 
- [Unity][iOS][13.11] facebook auto app event logging.
- [iOS][13.11] additional check for localization size.
- [iOS][13.11] showing ad from different ad_unit.
- [iOS][13.11] native vs banner plus floor v2.

## 2025-05-23
### Fixed
- [Unity] init for Editor mode. 

## 2025-05-23
### Fixed
- [Android][12.7] issue with localization parsing.
- [Unity] logic for Chinese codes in SayKitLanguageConverter.

## 2025-05-16
### Changed
- [Unity] int to long type in the event params.

## 2025-05-15
### Fixed 
- [Unity][Android][12.6] setup remote localizations.
- [Unity] loading localizations from resources.

## 2025-05-14
### Added 
- [Unity] turned off AudioListener stop/start for Android platform.
### Fixed 
- [iOS][13.8] skip after first start.

## 2025-05-13
### Fixed
- [Android][12.5] updating pre bid config ads settings.
- [Android][12.5] invalid context parameter.

## 2025-05-12
### Added
- [Unity][iOS][13.7][Android][12.4] context parameter to API and event manager.
- [Android][12.4] application session id.

## 2025-05-07
### Added
- [Unity] extended trackEvent method.

## 2025-05-06
### Fixed
- [Unity][iOS][13.6] sound issue after ads.
### Downgrade
- [iOS][13.5] google admob network to 11.13.0.

## 2025-05-05
### Added
- [Unity] support old gpgs plugin.

## 2025-04-30
### Added
- [Unity] runtime loading localizations for editor mode.

## 2025-04-25
### Added
- [iOS][13.4] feature toggle for enabling context menu.
- [iOS][13.4] application session id.
- [iOS][13.4] network condtition to sk_initialized event.
- [Unity] Scripting.Preserve annotation for SKLanguageLocalization.

## 2025-04-17
### Removed
- [Android][12.3] Tencent and Pubmatic networks.
### Fixed 
- [iOS][13.3] interstital instead of reward flow.

## 2025-04-16
### Added
- [Android][12.2] missing ad networks to disable_networks.

## 2025-04-16
### Removed
- [iOS][13.1] Tencent and Pubmatic networks.

## 2025-04-16
### Added
- [Android][12.1] pre bidding.
- [Android][12.1] pre bidding V2.

## 2025-04-15
### Updated
- [iOS][13.0] networks and frameworks.
### Refactored 
- [iOS][13.0] Ads manager.
### Removed
- [iOS][13.0] tiktok business sdk.

## 2025-04-11
### Added 
- [Unity][iOS][12.0][Adnroid][12.0] config v3.
- [iOS][12.0][Adnroid][12.0] config v3 improvements.
- [Android][12.0] tracking Max ads device id.
- [iOS][12.0] ad_unit_type to events and changed floorExtra.

## 2025-04-03
### Changed
- [Unity][Android][11.7] int type to long in trackEvent methods.

## 2025-03-31
### Downgraded
- [iOS][11.11] Applovin and networks.

## 2025-03-28
### Updated
- [Unity] SGS repository token.

## 2025-03-27
### Added
- [iOS][11.10] feature flag for selective init.
- [iOS][11.10] crash detector.
- [iOS][11.10] recieve and save applovin’s data (MAAd).

## 2025-03-25
### Fixed
- [iOS][11.8] cmp consent to avoid build failures.

## 2025-03-24
### Added 
- [Unity] SAYKIT_APPLOVIN_QUALITY_IOS_DISABLE define.

## 2025-03-24
### Removed
- [Android][11.6] tiktok business sdk.

## 2025-03-24
### Added
- [Android][11.5] counters for rewarded and interstitital ads.

## 2025-03-13
### Added
- [iOS][11.7] pre bidding.
- [iOS][11.7] pre bidding v2.
- [iOS][11.7] counters for rewarded and interstitital ads.

## 2025-03-13
### Added 
- [Unity][iOS][11.6][Android][11.4] GetAttributionData method.
- [Android][11.4] device type to device_info event.
- [Android][11.4] selective init.
- [Android][11.4] ads loading freeze tracker.
- [iOS][11.6] lazy mediation init step.
### Fixed 
- [Android][11.4] banner_click event for banner ads.

## 2025-02-27
### Fixed 
- [Unity] patching plugins gradle block.
### Added  
- [Unity] missed meta files.

## 2025-02-21
### Fixed  
- [Android][11.3] for providing a reward in IIR.

## 2025-02-20
### Added  
- [iOS][11.5] post bidding improvements.

## 2025-02-20
### Added  
- [Android][11.2] post bidding improvements.

## 2025-02-20
### Added 
- [Android][11.1] post bidding.

## 2025-02-19
### Updated 
- [iOS][11.3] AppLovinSDK to 13.1.0 for fixing audio issues.
### Added  
- [iOS][11.3] post bidding.

## 2025-02-18
### Removed 
- [iOS][11.2] Google Ad Manager adapter.

## 2025-02-18
### Changed
- [Unity] displaying versions in SGS manager.

## 2025-02-13
### Added
- [Android][11.0] PubMatic ad network.
- [Android][11.0] “experiments” param to the config and event. 
### Fixed 
- [Android][11.0] banner_click event.
- [Android][11.0] date time formatter.
### Updated
- [Android][11.0] networks and frameworks.

## 2025-02-13
### Added 
- [iOS][11.1] PubMatic ad network.
### Updated 
- [iOS][11.1] networks and frameworks.
- [Unity] min supported iOS version to 15.
### Refactored 
- [iOS][11.1] logger.
- [iOS][11.1] AttributionManager.

## 2025-02-07
### Fixed 
- [Unity] JDK path for UNITY_CLOUD_BUILD.

## 2025-02-06
### Added 
- [Android][10.7] interstital instead of reward v3.
- [Android][10.7] sending fb_mobile_first_app_launch event to facebook.

## 2025-02-06
### Added
- [iOS][10.11] sending fb_mobile_first_app_launch event to facebook.
- [iOS][10.11] interstital instead of reward v3.

## 2025-02-05
### Fixed 
- [iOS][10.10] live update value for floor extra param.

## 2025-02-04
### Changed
- [Unity][Android][10.6] types from int to long.
### Fixed
- [Android][10.6] countdown parameter.
### Removed 
- [iOS][10.9] PlatformManager.

## 2025-01-30
### Fixed
- [Unity] conflicting namespace.

## 2025-01-29
### Added
- [Unity] saving link.xml from SGS package.
- [Unity] support for installing SGS versions with postfixes.

## 2025-01-29
### Added
- [iOS][10.7] banners with multi ad units.
- [iOS][10.7] applied_experiments logic.
- [iOS][10.7] device_type to device info event.

## 2025-01-29
### Added
- [Android][10.5] banners with multi ad units.
### Updated
- [Android][10.5] Fyber to 8.3.5.

## 2025-01-24
### Added
- [Unity][iOS][10.6] script to compare minimum supported Xcode version.

## 2025-01-23
### Added
- [Unity] support Unity 2023+.

## 2025-01-23
### Fixed
- [Unity] user name for repository.

## 2025-01-22
### Removed 
- [Unity] custom json converter.
### Fixed
- [iOS][10.6] Native_logFirebaseEvent method.

## 2025-01-21
### Updated
- [Unity][Android][10.4] repository token.

## 2025-01-21
### Added
- [Unity] HttpClientHandler with dynamic proxy.
- [iOS][10.5] GetSystemProxy method.

## 2025-01-09
### Updated
- [Unity] SGS repository token.

## 2025-01-08
### Added
- [Android][10.4] custom data value for ad showing.
- [Android][10.4] moloco ad network.
### Fixed
- [Android][10.4] first start parameter.

## 2025-01-08
### Added
- [iOS][10.4] custom data value for ad showing.

## 2025-01-06
### Updated
- [Android] repository token.

## 2024-12-20
### Added
- [Unity][Android][10.3] support gradle build for custom plugins.

## 2024-12-17
### Added
- [Android][10.3] Adjust Meta referrer.
- [Android][10.3] threads optimization.
- [Android][10.3] toggle for app minimize event.
### Updated 
- [Android][10.3] ptime param in level events.

## 2024-12-16
### Added
- [Unity][iOS][10.3] LSMinimumSystemVersion key to Info.plist.

## 2024-12-16
### Added
- [Unity][iOS][10.3] toggle for app minimize event.

## 2024-12-10
### Added
- [Unity][Android][10.2] support JDK 17 and Gradle 8.8.

## 2024-12-09
### Added
- [iOS][10.2] YSO network.
- [iOS][10.2] session to config requests.
- [iOS][10.2] track volume events.

## 2024-12-09
### Added
- [Android][10.2] YSO network.
- [Android][10.2] Interstital instead of reward v3.
- [Android][10.2] session to config requests.
- [Android][10.2] free memory to ads events.
- [Android][10.2] track volume events. 
### Removed 
- [Android][10.2] ad loading from displaying.

## 2024-12-06
### Updated 
- [Unity][Android][10.1] adjust-android-signature lib to 3.35.2.

## 2024-12-04
### Fixed
- [Unity] remoteConfigUpdated action.

## 2024-12-03
### Added 
- [iOS][10.1] BigoAds ad network.
- [iOS][10.1] free memory to imp and close ad events.
- [iOS][10.1] quic to SayPromo.
- [iOS][10.1] AdsFreezeManager logic.

## 2024-12-03
### Added 
- [Android][10.1] BigoAds network.

## 2024-11-27
### Updated 
- [Android][10.0] frameworks and networks.

## 2024-11-27
### Updated 
- [iOS][10.0] frameworks and networks.
- [iOS][10.0] ptime param in level events.

## 2024-11-26
### Added 
- [iOS][9.13] sk_app_minimized event.
- [iOS][9.13] wait until application is active before show idfa popup.

## 2024-11-26
### Added 
- [iOS][9.12] check celluar/wifi is detected correctly.

## 2024-11-22
### Fixed
- [Unity] NRE in CrossPromo.
### Updated
- [Unity] SKAdNetworkItems.

## 2024-11-21
### Added 
- [Android][9.12] live update v2.
- [Android][9.12] fps_optimisation_enable to runtime config.

## 2024-11-20
### Added 
- [iOS][9.11] live update v2.
- [iOS][9.11] fps_optimisation_enable to runtime config.
- [iOS][9.11] bugs option to context menu.
### Refactored 
- [iOS][9.11] Migrants/Analytics.

## 2024-11-15
### Fixed
- [Android][9.10] chromium crash on amazon devices.
### Updated 
- [Android][9.10] Adjust to 5.0.0.

## 2024-11-05
### Fixed
- [iOS] internet tracking.

## 2024-11-05
### Fixed
- [Android] Firebase initialization.
- [Android] obfuscation.

## 2024-11-04
### Fixed
- [Android] disable_ad_overlap_check flag type.
### Removed 
- [Android] CurrentActivity.
### Refactored 
- [Android] models to data class.
### Removed 
- [Android] trackDelayed method.

## 2024-10-31
### Fixed
- [Unity] modifying podfile on post processing.

## 2024-10-29
### Removed
- [iOS] TopOn mediation.

## 2024-10-29
### Added
- [Unity] check SayKit initialization for trackInterstitialOffer and trackRewardedOffer methods.

## 2024-10-29
### Added 
- [iOS][Android] no_fill events.
- [iOS][Android] extra data to ad events.

## 2024-10-25
### Added 
- [Android] ad overlay event.
### Refactored
- [iOS][Android] Remote config loading process.

## 2024-10-16
### Fixed
- [iOS] crash caused number of floors returned by server.
### Added 
- [iOS] sk_error event for ads overlaps.
### Refactored
- [iOS] UserStateService.

## 2024-10-16
### Added 
- [Unity] GPGS queries for SAYKIT_PLAY_GAMES_SERVICES define.
### Refactored
- [Unity] SKManifestInjector.

## 2024-10-15
### Removed
- [Unity] unused directive.

## 2024-10-15
### Added 
- [iOS][Unity] support build for Xcode 16+.

## 2024-10-11
### Updated 
- [iOS] frameworks and networks.

## 2024-10-08
### Added 
- [Unity] age verification for China builds.

## 2024-10-08
### Fixed
- [Android] AppEventsLogger crash.

## 2024-10-08
### Removed
- [Android] Huawei flavor.

## 2024-10-04
### Updated 
- [Android] frameworks and networks.
### Fixed 
- [Android] empty ids issue.

## 2024-09-25
### Added 
- [Android][iOS] sending client_time param to all server urls.
### Updated 
- [Android][iOS] interstitials instead rewarded ads v2.

## 2024-09-24
### Fixed 
- [Unity] check enterPlayModeOptionsEnabled on Unity 2021-2022.

## 2024-09-11
### Removed 
- [Android] consent mode v2.
### Added 
- [Android] quic protocol.

## 2024-09-11
### Removed 
- [iOS] consent mode v2.

## 2024-09-09
### Fixed 
- [Unity] reseting init count for custom EnterPlayModeOptions.

## 2024-09-09
### Fixed 
- [iOS] sending negative value in events (param4).

## 2024-09-09
### Fixed
- [Unity] android build on Linux.

## 2024-09-05
### Refactored
- [Unity] GetScriptingDefineSymbols method.

## 2024-09-05
### Updated 
- [Android][iOS] banners min refresh timeout.
### Removed
- [Android] Xiaomi flavor.

## 2024-08-30
### Added
- [Unity][Android][iOS] getSessionId method.
- [iOS] skad_popup_error event.

## 2024-08-30
### Updated
- [Unity][iOS] native lib to 8.3.

## 2024-08-29
### Added
- [Unity][Android][iOS] SetExperimentDeviceId method.

## 2024-08-29
### Fixed
- [Unity] android build on windows.

## 2024-08-28
### Removed
- [Android] googleLight flavor.

## 2024-08-26
### Updated
- [Unity][Android][iOS] SayKit versioning.

## 2024-08-22
### Added
- [Unity] SGS version manager.

## 2024-08-21
### Added
- [Unity] extented trackPurchase methods.

## 2024-08-14
### Updated 
- [Unity][Android] target API to 34.
### Added 
- [Android][iOS] transmitting consent data to Adjust.
- [Android][iOS] timeout consent loading.
- [Android][iOS] TikTok SDK.

## 2024-08-08
### Added
- [Unity] extented trackPurchase method.

## 2024-08-06
### Removed
- [Unity][Android][iOS] custom consent.

## 2024-08-05
### Fixed
- [iOS] device_name divergence.

## 2024-07-25
### Added 
- [Android] Google consent mode V2.

## 2024-07-17
### Added 
- [iOS] quic protocol.
- [iOS] split SayKit to several pods.
 
## 2024-07-12
### Fixed
- [Unity] build gradle for Unity 2022.

## 2024-07-11
### Added
- [Unity][Android] Applovin Quality Service.

# 2024-07-10
### Added
- [Unity][iOS] saving SayKit libraries versions.

# 2024-07-10
### Added
- [Android] uninstall and reinstall measurement.
### Refactored
- [Android] InAppManager.

# 2024-07-02
### Added
- [Unity] safe init.

# 2024-07-02
### Added
- [Unity] gitattributes.

## 2024-07-01
### Fixed
- [Unity][Android] build crash with flag split application binary Unity 2020.

## 2024-06-21
### Updated
- [Android][iOS] frameworks and networks.

## 2024-06-17
### Added
- [Android] request id to saypromo.
- [Android] BOM artifact.

## 2024-06-10
### Added
- [iOS] request id to saypromo.
### Fixed 
- [iOS] store opening via cross promo.

## 2024-06-10
### Added
- [Android] multi-ad-units V3.

## 2024-06-06
### Added
- [Unity] storage service for Editor.

## 2024-05-30
### Updated
- [Android] frameworks and networks.
### Fixed 
- [Unity] GetRuntimeInfo method for Editor.
 
## 2024-05-28
### Updated
- [Unity][iOS] system localisation languages.

## 2024-05-28
### Added
- [Unity][Android] uploading build symbols to Firebase.

## 2024-05-28
### Added
- [iOS] multi-ad-units V3.
- [iOS] ios_build event.

## 2024-05-22
### Added
- [iOS][Android][Unity] live request.
- [iOS] open browser via SafariViewController.

## 2024-05-15
### Updated
- [iOS] firebase and google libs.

## 2024-05-07
### Fixed
- [Unity] settings Gradle injection.
### Refactored
- [Unity] errors on prebuid.

## 2024-05-02
### Fixed
- [Android] crash FirebaseApp.getInstance.
- [Android] ANR getExternalFilesDir/getExternalFilesDirs.

## 2024-04-26
### Added
- [Unity] processing SAYKIT_BANNER_DISABLED define.

## 2024-04-26
### Refactored
- [Unity] cross promo events.

## 2024-04-18
### Added
- [iOS] multi-ad-units.
- [iOS] interstitials instead rewarded ads.
- [iOS] Google consent mode V2.
### Fixed 
- [iOS] crash when try to send event(sk_exception) before remote config inited.

## 2024-04-18
### Refactored
- [Unity][iOS] adding push notification and game center capability.

## 2024-04-17
### Added
- [Unity] performance tracker.

## 2024-04-17
### Added
- [Android] multi-ad-units.
- [Android] Adjust MIR.
- [Android] interstitials instead rewarded ads.

## 2024-04-16
### Added
- [Unity][Android] leakcanary lib.

## 2024-04-12
### Added
- [Android] improvements for native banners.
- [Android] WebView version event.
### Fixed
- [Android] native banners.
- [Android] java.lang.IllegalStateException - Already resumed.
- [Android] JSON validity error.

## 2024-04-11
### Added
- [Unity][iOS] unity privacy manifest.
- [iOS] improvements for native banners.
### Updated
- [iOS] max mediation.
### Fixed
- [iOS] native banners.

## 2024-04-08
### Added
- [Unity][iOS] ITSAppUsesNonExemptEncryption to Info.plist.
- [Android] logging Player Prefs/User Default size on start.
### Fixed
- [Unity] Editor warnings.
- [Android] DynamiteModuleLoadingException.

## 2024-04-03
### Refactored
- [Unity] remote settings.

## 2024-04-03
### Added
- [Unity][Android] saving SayKit libraries versions.

## 2024-04-01
### Refactored
- [iOS] config/live requests to POST.

## 2024-03-19
### Added 
- [Unity] SAYKIT_IOS_GAME_CENTER define for supporting Game Center.

## 2024-03-18
### Added
- [Android] support link encryption.
- [Android] improve attribution.
### Refactored
- [Android] InstallReferrer.
- [Android] config/live requests to POST
- [Android] init Adjust on start app.
### Updated
- [Android] max mediation.

## 2024-03-14
### Added
- [iOS] support link encryption.
- [iOS] logging Player Prefs/User Default size on start.
### Updated
- [Unity] Pre and Post build process.
- [Unity] external dependency manager to 1.2.178.
- [iOS] Deployment target to 13.0.
- [iOS] max mediation.
- [Android] minimum API to 26.
### Fixed
- [iOS] long init on iPad.

## 2024-03-12
### Added
- [Unity] track IAP package version.

## 2024-03-06
### Added
- [Unity] optimization IDFA alert textures.

## 2024-02-28
### Added
- [Unity][iOS] IDFA alert.
### Refactored
- [Unity] internal logging.

## 2024-02-26
### Added
- [Android] remove old configs on update.
- [Android] using Chrome tabs.
### Refactored
- [Android] SayPromo sdk.
- [Android] NetworkClient.
### Removed
- [Android] waitUntil method.

## 2024-02-19
### Fixed
- [iOS] show_banner, hide_banner events.

## 2024-02-14
### Fixed
- [Android] background banner update.
- [Unity] serialization exception
### Refactored
- [Android] internal modules.

## 2024-02-12
### Fixed
- [iOS] trackSoftIncome method.

## 2024-02-07
### Fixed
- [Unity] inapp validation callback.

## 2024-02-05
### Added
- [iOS] Remove old configs on update.
### Updated
- [iOS] SayPromo sdk.

## 2024-01-30
### Added
- [Android] minor improvements to the process of sending events before initialization.
### Updated
- [Android] SayPromo sdk.
### Removed
- [Unity][Android] unused skad flags.

## 2024-01-29
### Refactored
- [Unity] thread service.

## 2024-01-24
### Added
- [Unity] OnAdRevenuePaid and OnAdDisplayed actions to API.
- [Unity][iOS] UNITY_EDITOR defines to AppLovinPostProcessiOS.cs.
- [Unity] more file extensions in PreBuild check. [.cs, .java, .c, .cpp, .h].
### Refactored
- [Unity] internal exceptions to sk_unity_exception.
### Fixed
- [Unity] UnityEditor.PackageManager.Client calls.
- [Unity] removed old saykit_notification_icon.

## 2024-01-19
### Fixed
- [iOS] player time (ptime) calculation. 
### Added
- [iOS] brotli compression support.
- [iOS][Android] added sk_version_native event.

## 2024-01-12
### Fixed
- [Unity] Fixed imports.
- [Unity] Unity 2022 build process.
- [Unity] purchasing dependency for CrossPromo.
### Added
- [Unity] SGS services framework.
- [Android] largeHeap parameter to the manifest.

## 2024-01-09
### Refactored
- [Unity] CrossPromo loading flow. (Moved to v1)
- [Unity] Cleaned old cashed saykit configs.
### Added
- [Unity] Android SAYKIT_BACKUP_DISABLE symbol. (disable android cache for SharedPreferences)
- [iOS] sending session id to Firebase Crashlytics
- [Android] brotli compression support.

## 2024-01-04
### Removed
- [iOS][Android] AdColony sdk.

## 2023-12-28
### Fixed
- [Unity] logFBAppEvent and logFirebaseEvent API methods.

## 2023-12-20
### Added
- [Unity] ui sprite atlas.

## 2023-12-19
### Updated
- [iOS][Android] Mintegral and Pangle sdks.
### Added
- [Unity] namespace to the UnityEditor.PackageManager.Client class.
- [iOS] Facebook deep link check to the AppDelegate.
- [Android] smart-inter branch merge. (SAYKIT_SMART_INTER)
### Fixed
- [Android] migration Storage data.
- [Android] costAmount param in attribution data json.
### Removed
- [Android] android.enableJetifier flag.
### Refactored
- [Android] bridge callbacks.

## 2023-12-11
### Updated
- [iOS] Deployment target to 12.0.

## 2023-12-11
### Added
- [iOS][Android] Polish, Turkish, Indonesian, Thai and Vietnamese languages.
### Fixed
- [iOS] AdMob banner resize for some native banners.

## 2023-12-08
### Added
- [iOS][Android] GetPlayingTime method to API.

## 2023-11-20
### Updated
- [iOS][Android] network sdks.
- [iOS][Android] max mediation. (Consent TCFv2 support)
- [iOS][Android] method for passing the consent flag to Max.
- [iOS][Android] other dependencies.

## 2023-11-20
### Added
- [iOS][Android] AdMob Google Consent.
- [iOS][Android] SayKit version to events endpoint.
- [Android] support player id.
### Updated
- [Android] saypromo sdk.
### Fixed
- [Android] returned crashlytics ndk dependency.
- [Android] internal crash.
### Refactored
- [iOS][Android] language param in the remote config request.

## 2023-11-13
### Refactored
- [Unity] batch mode check.
### Fixed
- [Unity] Unity 2022 merge bug.

## 2023-11-10
### Fixed
- [Android] notification icon merge bug.

## 2023-11-09
### Added
- [Unity] ShowMaxMediationDebug method to API.
- [Unity] check Unity license for batchmode on PreBuild.
- [Unity] IsDebugUser method to API.

## 2023-11-07
### Added
- [iOS][Android] native banner ad.

## 2023-11-01
### Added
- [Unity] debug menu landscape UI.
### Fixed
- [Unity] remote config download with base version.

## 2023-10-31
### Fixed
- [Android] remote config parsing bug.

## 2023-10-26
### Updated
- [iOS][Android] network sdks.
- [iOS][Android] max mediation.
### Fixed
- [iOS] device_id in start events.

## 2023-10-20
### Added
- [Unity] Unity 2022 support.
### Fixed
- [iOS][Android] TrackHardIncome method.

## 2023-10-19
### Fixed
- [Android] Adjust attribution conversion for empty traffic.

## 2023-10-16
### Fixed
- [Android] Notification icon.

## 2023-10-11
### Fixed
- [Unity] Removed embedded param from remote config url.

## 2023-10-10
### Fixed
- [iOS] skad conversion bug.

## 2023-10-06
### Added
- [iOS][Android] disableInterstitial param.
- [iOS] IsAppStoreAvailable method.

## 2023-10-02
### Added
- [iOS][Android] null checker for string arguments in the bridge.
### Fixed
- [iOS][Android] SAYKIT_BANNER_DISABLED symbol.

## 2023-09-28
### Fixed
- [iOS][Android] OverrideSystemLanguage method.

## 2023-09-25
### Fixed
- [iOS] GetBackgroundBannerSize method.

## 2023-09-22
### Fixed
- [iOS][Android] interstitial popup.

## 2023-09-21
### Fixed
- [Android] requestConfigMigration bug.
### Refactoring
- [iOS][Android] init localization from the build-in config.

## 2023-09-20
### Fixed
- [iOS][Android] remoteConfigUpdated action call.
### Added
- [Android] SAYKIT_PLAY_GAMES_SERVICES define for com.google.android.gms:play-services-games-v2:+.

## 2023-09-19
### Fixed
- [iOS] build version property.

## 2023-09-13
### Fixed
- [iOS] ukrainian localization.

## 2023-09-12
### Fixed
- [Unity] build process on Window OS.

## 2023-09-07
### Fixed
- [Unity] build process with useAPKExpansionFiles flag on Android.

## 2023-09-06
### Fixed
- [iOS][Android] remote config manager Initialized flag init.

## 2023-09-05
### Fixed
- [Unity] creation of SayKit directories for Android.

## 2023-09-04
### Added
- [iOS][Android] IsRateAppPopupShown method to SayKit API.

## 2023-08-31
### Fixed
- [Unity] Unity 2019 build process.

## 2023-08-30
### Added
- [iOS][Android] thread check for Unity-Native bridge.
- [Android] Notification permission request for Android 13+.
- [iOS][Android] trackInAppOffer method.
- [iOS][Android] dynamic timeouts for interstitial ads.
- [iOS][Android] max mediation waterfall tracking.
- [iOS][Android] BidMachine ad network.
- [iOS][Android] GoogleAdManager ad network.
- [iOS][Android] segment to the support url.
### Updated
- [iOS] min iOS version to 12.
- [Android] target API to 33.
### Refactored
- [iOS][Android] most of the SDK code has been ported from Unity to iOS/Android.
- [iOS][Android] interstitial_*/rewarded_* events.
- [Android] build process and injection of dependencies.
- [Android] consent popup.
- [iOS][Android] improved logging of caught errors in Firebase.
### Removed
- [Unity] SayKitBanner prefub.

## 2023-07-28
### Fixed
- [iOS][Android] CultureInfo bug in the meditaion wrapper.

## 2023-07-06
### Fixed
- [iOS] crashes on iOS 13.

## 2023-06-20
### Fixed
- [Android] getHistoricalProcessExitReasons exception.

## 2023-06-19
### Fixed
- [iOS][Android] deactivated SayKitUI prefab.

## 2023-06-14
### Refactored
- [iOS][Android] disabled facebook iap/cpm check.

## 2023-06-13
### Removed
- [iOS][Android] atyp fonts.

## 2023-06-12
### Added
- [iOS][Android] SayCatalogue.
### Refactored
- [iOS][Android] device_info event.

## 2023-05-23
### Updated
- [iOS][Android] network sdks.
- [iOS][Android] max mediation.
### Refactored
- [iOS][Android] debug menu.
### Fixed
- [iOS][Android] empty config init.

## 2023-05-15
### Fixed
- [iOS] entitlements for a china builds.

## 2023-04-11
### Added
- [Android] SAYKIT_DISABLE_AD_SKIP_BUG symbol.
### Removed
- [iOS][Android] unused logs.
- [iOS][Android] realtimeSinceStartup time from debug messages.

## 2023-04-10
### Fixed
- [iOS] AddPushNotificationCapability method.

## 2023-04-05
### Fixed
- [Android] isTablet method in the SayMaxManager class.

## 2023-04-03
### Added
- [iOS][Android] SetUnityExceptionContext method to API.
- [iOS][Android] DisableSayKitLogs to API.
- [iOS][Android] LogLevel to SayKit logs.
- [iOS] SAYKIT_PLIST_LOCALIZATION_DISABLE
### Refactored
- [Android] Disabled unity3d logs.
- [Unity] Removed minifyWithR8 check.
### Fixed
- [iOS] notification entitlements path.

## 2023-03-29
### Fixed
- [iOS] conflict with NiceVibration package.

## 2023-03-28
### Refactored
- [iOS] Updated SaySwizzle order. 
- [iOS] Removed CheckNiceVibrationPackage method. 
- [iOS] Added adjust_skad_enabled, facebook_skad_enabled flags.

## 2023-03-22
### Refactored
- [Unity] Turned off config v2 for the Editor.

## 2023-03-21
### Refactored
- [Unity] Added package name and namespace to Client class call.

## 2023-03-01
### Fixed
- [Android] Max callbacks without internet connection.

## 2023-02-27
### Fixed
- [Editor] errors in the Unity editor.

## 2023-02-24
### Added
- [Android] compileClasspath setting for Unity 2020+.
### Fixed
- [Android] rewards after rewarded ads. [Bug from 2023022200]

## 2023-02-21
### Fixed
- [Android] downgraded mintegral, ironsource and unity network sdks.

## 2023-02-16
### Fixed
- [iOS][Android] iap, imp conversion manager requests.

## 2023-02-15
### Added
- [Android] free memory check for interstitial ads. [Optional]

## 2023-02-13
### Added
- [iOS][Android] isRewardedPlacementAvailable method to public API.
### Refactored
- [iOS][Android] removed overrideDeviceId param.

## 2023-02-10
### Refactored
- [iOS][Android] Removed onCloseCallback in showInterstitial method for Premium users.

## 2023-02-07
### Fixed
- [iOS] NiceVibration package.

## 2023-02-06
### Added
- [iOS][Android] device_os param to remote config request.

## 2023-01-31
### Refactored
- [iOS][Android] Renamed sk_app_close to sk_app_exit and fixed client_time.

## 2023-01-27
### Added
- [Android] AdMarketplace adapter.

## 2023-01-26
### Fixed
- [iOS][Android] network service exception handling.
### Refactored
- [iOS][Android] renamed Command class.

## 2023-01-24
### Fixed
- [iOS][Android] Fixed click events.

## 2023-01-23
### Added
- [iOS][Android] APS rewarded.
- [iOS][Android] last fps list to fps event. 
- [iOS][Android] SetFPSGameContext method. 
### Fixed
- [iOS] SK ad newtork flow.
- [iOS][Android] Fixed RequestConfigMigration method.
### Removed
- [Android] sending ANRs to Firebase.
### Refactored
- [iOS] post build order for AppLovin.

## 2023-01-19
### Added
- [Android] Chartboost network.
- [iOS] sk_app_close event.
- [iOS][Android] version_update.
- [Android] saypromo domain to network_security_config.
- [iOS][Android] trackItem/trackItemLoss events with string customData.
### Refactored
- [iOS][Android] remote config load.
- [iOS][Android] TrackApplicationLoaded method.
- [iOS][Android] Removed overrideDeviceId param from SayKitConfig.
### Fixed
- [iOS][Android] getLanguage method.
- [iOS][Android] InitializedProgress param.
- [iOS][Android] InternetReachability check.
- [iOS] Turned off Adjust SKAd network handling.
### Updated
- [iOS][Android] Cross promo UI.
- [iOS][Android] saypromo framework.

## 2023-01-13
### Added
- [Android][iOS] GetTotalMemory method to public API.
- [Android][iOS] GetFreeMemory method to public API.

## 2023-01-12
### Updated
- [Android][iOS] Max mediation.
- [Android][iOS] network sdks.
- [Android][iOS] Adjust sdk.
- [Android][iOS] External Dependency Manager libs.
### Fixed
- [Android][iOS] Fixed PostWithHeaders method in the NetworkService.

## 2023-01-10
### Fixed
- [iOS][Android] rcm_remote_exc event call bug.

## 2023-01-10
### Added
- [iOS][Android] in-app purchase validation.
- [iOS][Android] trackPurchaseOffer method

## 2023-01-04
### Added
- [iOS][Android] remote commands.

## 2022-12-29
### Added
- [iOS][Android] overrideDeviceId param to SayKitConfig.

## 2022-12-28
### Added
- [iOS][Android] extra param to OpenSupportPage method.
- [Android] sk_app_close event.
- [Android] debug option of network_security_config with SAYKIT_DEBUG symbol.
### Fixed
- [Android] non-fatal crash in SayMaxManager.

## 2022-12-15
### Fixed
- [iOS][Android] Fixed critical bug in work without internet connection. (Affected version: 2022120801, 2022121200, 2022121201.)


## 2022-12-12
### Added
- [iOS] new Consent popup.
- [Android] new Consent popup. (Disabled by default)
### Updated
- [iOS][Android] saypromo network to 11.0.20

## 2022-12-08
### Refactored
- [iOS][Android] Network service.
- [iOS][Android] cross promo initialize process.
- [iOS][Android] SayKit initialize process.

## 2022-11-30
### Refactored
- [Android] CheckAssetPacks method.

## 2022-11-29
### Added
- [iOS][Android] GetInitState method.
### Refactored
- [iOS][Android] InitState enum.

## 2022-11-24
### Added
- [iOS] Fixed ad libraries linking for UnityFramework. (Affected some old iOS versions)

## 2022-11-22
### Added
- [iOS] Chartboost network sdk.

## 2022-11-07
### Added
- [iOS][Android] Firebase ad_impression event.

## 2022-11-02
### Added
- [iOS] AdSupport framework.

## 2022-11-01
### Added
- [Editor] SayKit purchase log.
### Updated
- [iOS] SkAdNetwork list. 
### Removed
- [iOS][Android] some debug logs. 

## 2022-10-27
### Refactored
- [Android][iOS] Turned off adapter load fail analytic events and banner load analytic events.

## 2022-10-24
### Updated
- [Android][iOS] Max mediation.
- [Android][iOS] network sdks.
### Added
- [Android][iOS] manual network adapters.
### Refactored
- [Android][iOS] consent flow. [Disabled]

## 2022-10-21
### Added
- [Android][iOS] request GUID id to OpenSupportPage method.
- [Android] hotfix to PlayingTimeService for developing builds.

## 2022-10-20
### Fixed
- [Editor] SayKitPowerManager entry point exception.

## 2022-10-17
### Added
- [Android][iOS] ukrainian language support.
### Fixed
- [Android] gradle compression bug (LZ4, LZ4HC) on Unity 2021.

## 2022-10-15
### Added
- [Android][iOS] AdInfo to "rewarded/interstitial_tag" events.
- [Android][iOS] "sk_save_mode", "sk_thermal_state" events.
### Refactored
- [Android][iOS] internet reachability service.
- [Unity] all warnings.
- [Android][iOS] show banner flow.
- [iOS] dateFormatter method.
- [Android][iOS] event params check.

## 2022-10-11
### Added
- [iOS] Added Facebook DeepLink verify.

## 2022-10-07
### Added
- [Android][iOS] SAYKIT_CLOUD_BUILD symbol.

## 2022-09-27
### Added
- [Android][iOS] interstitial popup.
- [Android][iOS] app_update event.
### Updated
- [Android] saypromo network.
- [Android] network_security_config.
### Refactored
- [Android] idfa flow.

## 2022-09-27
### Added
- [Android] Added OpenGooglePlaySubscriptionCenter method.

## 2022-09-26
### Fixed
- [iOS][Android] manual control of APS banner.

## 2022-09-13
### Removed
- [iOS] MyTarget, Yandex sdk.
- [Android] MyTarget sdk.

## 2022-08-25
### Added
- [iOS][Android] notificationTokenReceived action to SayKitConfig.
- [iOS][Android] disableAutoBannerTimeouts param to SayKitConfig.
- [iOS][Android] GetNotificationToken method.
- [iOS][Android] show_banner, hide_banner events.
- [iOS][Android] override_language event.
- [iOS][Android] config_migration, rate_app, support_page events.
### Refactored
- [iOS][Android] DebugMenu. Added reloadConfig button.
- [iOS][Android] SayKitBridge. Removed SayKitBridge prefub.
- [iOS][Android] app_start and language events.
- [iOS][Android] notification_token event.
- [iOS] SKAdNetwork conversion flow.
### Updated
- [Android] network sdks.
### Fixed
- [Android] InstallReferrer ANR.

## 2022-08-18
### Removed
- [iOS][Android] UNITY_CLOUD_BUILD symbol.

## 2022-07-25
### Added
- [Editor] SAYKIT_ASSETPACKS symbol.

## 2022-07-21
### Added
- [Editor] SAYKIT_AUTOBUILD_KEYSTORE symbol.

## 2022-07-20
### Fixed
- [Editor] NotificationManager error.
- [Editor] build process on Windows for Unity 2019.

## 2022-07-18
### Fixed
- [Android] showing banner after hide call.

### Removed
- [Android] legacy gradle file.

## 2022-07-15
### Added
- [iOS][Android] log crashlytics methods.
- [iOS][Android] Max verbose logs when debug_mediation is turned on.
- [iOS][Android] extra string parameters to trackLevel methods.

### Fixed
- [iOS][Android] trackLevelExtra methods with extra int and string parameters.

### Updated
- [iOS][Android] network sdks.
- [iOS][Android] adjust sdk.
- [iOS][Android] max mediation sdk.
- [Android] gradle version to 4.0.1.
- [Android] target API to 31, Unity configuration guides:

	<a href='https://docs.google.com/document/d/1BV-Ocn8PfRrFFvGDjYERDHJS9IqrJeQbjXk4_zyFj8Q/edit'> - [RU] API 31 guide.</a> 
	
	<a href='https://docs.google.com/document/d/127FLIEgS6vLLnSpM6zvfynDLpTjs57OTyxSaNbWW21U/edit'> - [EN] API 31 guide.</a> 

### Removed
- [iOS] support Unity 2018.
- [Android] location permissions in the manifest.
- [Android] legacy activities in the manifest.

### Refactored
- [iOS] NotificationManager.

## 2022-07-07
### Removed
- [Android] Yandex sdk.

### Updated 
- [iOS] saypromo framework.

## 2022-06-30
### Refactored
- [iOS] downgraded unity-ads framework.

## 2022-06-23
### Added
- [iOS][Android] trackLevelExtra methods with extra int and string parameters.

## 2022-06-22
### Fixed
- [iOS][Android] UIManager double initialization bug.

## 2022-06-15
### Added
- [iOS][Android] support custom GDPR popup.

### Removed
- [iOS][Android] Smaato network sdk.
## 2022-06-14
### Refactored
- [iOS][Android] max mediation to native implementation.

### Updated
- [iOS][Android] network sdks.
- [Android] tapjoy/smaato repository links.

### Added
- [iOS][Android] Tapjoy network sdk.
- [iOS][Android] Smaato network sdk.
- [Android] smart anr check.

### Removed
- [Android] facebook-bidding sdk.

## 2022-07-13
### Added
- [iOS][Android] manual language control.

## 2022-05-26
### Fixed
- [iOS][Android] turned off SayKit folder name check.

## 2022-05-24
### Added
- [iOS][Android] string param to level_extra events.
- [iOS][Android] string param to soft/hard_income/outcome events.

## 2022-05-18
### Added
- [iOS][Android] minification flag check for Unity 2020.1+.
- [iOS][Android] sort order check for SayKitUI prefab.
- [iOS][Android] attempts to load the config for the build machine.
- [iOS][Android] SayKit folder name check.

### Fixed
- [iOS][Android] minimum iOS version check for Windows OS.

## 2022-05-18
### Added
- [iOS][Android] Playing time param to events.

### Updated
- [iOS][Android] Adjust plugin.

### Refactored
- [iOS][Android] SayKit.cs file.

### Fixed
- [iOS][Android] Facebook AppEventsLogger for some China devices.

## 2022-05-13
### Fixed
- [iOS][Android] manual banner control.
- [Android] disabled Facebook automatically logged events for Unity 2020+. 

## 2022-05-05
### Added
- [iOS][Android] recalculating background banner size if screen resolution is changed.
- [iOS][Android] CMP sum event to Facebook.
- [iOS][Android] saykit version to SayPromo mini.
- [iOS][Android] result_url support in SayPromo mini.
- [iOS][Android] check multiply calls to showInterstitial/Rewarded.

### Refactored
- [iOS][Android] debug menu flow.
- [iOS][Android] interstitial/rewarded_click events. Added count of clicks to param1

### Fixed
- [iOS][Android] ad_time user state event.

## 2022-04-28
### Fixed
- [iOS][Android] duration param in level_extra_completed and level_extra_failed events.

## 2022-04-21
### Added
- [iOS][Android] RequestRemoteConfigUpdate method.

## 2022-04-14
### Added
- [iOS][Android] GetSubscriptionExpirationTimestamp method.
- [iOS][Android] GetRequestConfigTimestamp method.

## 2022-04-12
### Added
- [iOS][Android] skip_after_first_app_start param support.

### Refactored
- [iOS][Android] Hide banner method.

## 2022-04-07
### Refactored
- [Android] Turned off GDPR request when user is offline.

## 2022-04-06
### Fixed
- [iOS][Android] Fixed SayPromo mini. (Bug from 2022-03-17)

## 2022-04-05
### Added
- [iOS][Android] store_product_id parameter to iap events.

## 2022-04-04
### Fixed
- [iOS] Fixed China/Japan "app tracking" permission localization.

## 2022-03-17
### Added
- [iOS][Android] APS network framework.
- [Android] Notification icon.
- [Android] AD_ID permission for Android 12.
- [Android] android:exported flag to SayKitActivity.
- [iOS][Android] override_segment event.

### Refactored
- [iOS][Android] AdsManager.
- [iOS][Android] File manager.
- [iOS] Swift version.

### Fixed
- [Android] Unity 2021 gradle version.
- [iOS][Android] Version manager.

### Removed
- [Android] ANR watchdog sdk.

## 2022-02-28
### Updated
- [Android] SayPromo network sdk.

## 2022-02-21
### Added
 - [iOS] Applovin Ad Review.
 - [iOS][Android] Waterfall latency to ad loaded events.

## 2022-02-17
### Updated
 - [iOS][Android] trackRewardedOffer/trackInterstitialOffer events. Added extra string parameter.

## 2022-02-14
### Updated
 - [iOS][Android] AppLovin plugin.
 - [iOS][Android] All third party libraries.
 
### Removed
- [Android] exoplayer framework.

### Fixed 
- [Android] InstallReferrer ANR.

## 2022-02-04
### Added
 - [iOS] User tracking permission popup localization.

## 2022-02-03
### Added
 - [iOS][Android] play_store event.

## 2022-01-27
### Added
 - [iOS][Android] Debug menu.

## 2022-01-26
### Added
 - [iOS][Android] Notification event if the application was opened from a notification.
 ### Fixed
 - [iOS][Android] Timestamps in Mexican and Arabic localization events.

## 2022-01-18
### Added
 - Unity 2021 support.
 
### Updated
 - [Android] showCustomRateAppPopup method.
 - External dependency manager.
 - [iOS][Android] trackTagEvent method


## 2022-01-17
### Fixed
 - [iOS][Android] Facebook purchase events.
 
## 2022-01-13
### Removed
 - [Android] jcenter repository.
 - [Android] amazon adapter.

## 2022-01-12
### Added
 - [iOS][Android] Localized privacy policy link.


## 2022-01-06
### Updated
 - [iOS] The number of dsyms upload attempts to Firebase.

## 2021-12-22
### Updated
- [Android] saypromo framework.
### Removed
- [Android] jcenter links. 
- [Android] amazon adapter.

## 2021-12-22

### Added
- [iOS][Android] Namespace to Utils class calls.
- [iOS][Android] User state events to facebook/firebase. 
- [iOS][Android] trackTutorialCompleted method to SayKit.
- [iOS][Android] OpenSupportPage method.
- [iOS] Tencent network sdk.
- [iOS][Android] adjust_id event.
- [iOS][Android] SayKit UI autoinitialize.
- [iOS][Android] fb-content-type to facebook purchase event.
- [iOS] registerAppForAdNetworkAttribution call.
- [iOS] China bundle localisation.
- [iOS] Ogury sdk.

### Fixed
- [iOS] Banner background size.
- [iOS] Black screen bug for Unity 2019.3+.
- [iOS] UnityPause bug for AdColony network.

### Updated
- [iOS][Android] Network sdks and 3rd party sdks.
- [iOS] Conversion flow.
- [Android][iOS] Localisation init. Added null check to getLocalizedMessage method.

### Removed
- [iOS][Android] Facebook automatically logged events. 
- [iOS][Android] inAppPurchaseServerCheck flag.
- [iOS][Android] Tenjin sdk.



## 2021-10-05
### Added
- [iOS][Android] FPS tag methods.
- [iOS] NSAdvertisingAttributionReportEndpoint property.
- [iOS][Android] GetGdprStatus method.
- [Android] disableAutoBanner flag to SayKitConfig.
- [Android] Activity event.
- [iOS] Flush calls for facebook purchase event.
- [iOS] Upload symbol script for UCB.
- [iOS][Android] Ad duration to close events.
- [iOS] Check iOS version on PreBuild. 
- [iOS][Android] Sending ad_revenue tp facebook purchase.

### Fixed
- [iOS] Store on iOS 12 and below for unity saypromo.
- [Android] Showing a banner before rate popup.
- [iOS] Unity 2020+ duplicate swift libraries bug.
- [Editor] IDFA initialise process.

### Updated
- [Android] Screen sleep timeout.
- [iOS][Android] MaxSdk plugin.
- [iOS][Android] Network sdks.

### Removed
- [Android] com.android.vending.BILLING permission in AndroidManifest.


## 2021-08-12
### Added
- [iOS][Android] Build Machine EDM config.
- [iOS][Android] External Dependency Manager config check on PreBuild. (Use SAYKIT_EDM_CHECK_DISABLE symbol to disable)
- [iOS][Android] getCurrentLanguage method.
- [iOS][Android] reloadConfigManually method to SayKit.
- [iOS] iOS version check on PreBuild. Updated CHANGE LOG.

### Fixed
- [iOS] saypromo-resources bug.

### Updated
- [iOS] Saypromo network sdk.
- [iOS] firebase_symbols.sh script.
- [iOS][Android] Min supported version of iOS to 10.0.
- [iOS][Android] Adjust sdk.
- [iOS] Network sdks.
- [iOS] Firebase post build scripts. Added SAYKIT_UPLOAD_SYMB_DISABLE symbol.
- [iOS][Android] Adjust attribution flow.

### Removed
- [iOS][Android] Max sdk post build methods.
- [iOS][Android] Amazon adapter.


## 2021-07-05
### Added
- [Android] Native review popup.
- [iOS][Android] ExternalDependencyManager. (Disabled for Android features.)
- [iOS][Android] getCurrentLanguage method.
- [iOS][Android] Yandex sdk.

### Updated
- [iOS][Android] Network sdks.
- [Android] Maven repositories.

### Removed
- [iOS] iOS frameworks. Added cocoapods links.
- [iOS] Unused fonts.
- [iOS][Android] MaxSdk banner background.

### Fixed
- [iOS][Android] GDPR flow.


## 2021-06-10
### Fixed
- [Android] Advertising id event.
- [iOS][Android] Facebook auto events.

### Updated
- [iOS][Android] Max sdk Unity plugin.

### Added
- [iOS][Android] Manuall inapp manager.
- [iOS] 'controller' event.
- [2021053102][iOS] UnityPause check to ad callbacks.
- [iOS][Android] First start check for attribution manager.


## 2021-06-08
### Added
- [iOS][Android] InApp purchase manager.

## 2021-05-31
### Updated
- [iOS][Android] All network libraries.
- [iOS][Android] All network mediations.


## 2020-08-03
### Added
- [iOS|Android] APS ad network

## 2020-07-22
### Updated
- [iOS|Android] SayKitApp.cs required params.

```
   /* Features */
    public const bool notificationsEnabled = false;
    
    /* App settings */
    public const string APP_NAME_CHINA_IOS = "<APP_NAME_CHINA_IOS or empty>";
    public const string APP_NAME_IOS = "<APP_NAME_IOS>";

    public const string APP_BUNDLE_CHINA_IOS = "<APP_BUNDLE_CHINA_IOS or empty>";
    public const string APP_BUNDLE_IOS = "<APP_BUNDLE_IOS>";


#if SAYKIT_CHINA_VERSION
    public static bool purchasesEnabled = false;

    public const string APP_KEY_IOS = "<APP_KEY_CHINA_IOS or empty>";
    public const string APP_SECRET_IOS = "<APP_SECRET_CHINA_IOS or empty>";

#else
    public static bool purchasesEnabled = true;

    public const string APP_KEY_IOS = "<APP_KEY_IOS>";
    public const string APP_SECRET_IOS = "<APP_SECRET_IOS>";

#endif
```
- Updated README. 

## 2020-05-18
### Updated
- [Android] All network libraries.
- [Android] All network mediations.

## 2020-05-04
### Added
- [Android] SayMediation supports SayPromo bidding and Mintegral ad network

## 2020-04-06
### Added
- [Android] New ad mediation

## 2020-03-25
### Updated
- [iOS] MoPub to 5.11. 
- [iOS] All network libraries.
- [iOS] All network mediations.

### Deleted
- [iOS] UIWebView.


## 2020-02-12
### Added
- [iOS|Android] Facebook events.
- [iOS|Android] Hindi localization.
- [Android] Adapter black list.

### Updated
- [Android] MoPub to 5.10. 
- [Android] All network libraries.


## 2019-12-20
### SayEndpoint
- Strict events delivery with persistent cache.

## 2019-12-18
### Updated
- [iOS] Mediation libraries.
- [Android] Tenjin sdk.

## 2019-12-10
### Fixed
- [Android] Fixed MoPub rewardedVideo bug.

## 2019-12-06
### Added
- [iOS][Android] Tenjin DeepLink log.
- [iOS][Android] interstitial_click, interstitial_close, rewarded_click, rewarded_close events.

### Updated
- [iOS|Android] Level events.

## 2019-11-29
### Fixed
- [iOS|Android] hasInterstitialCachedStatus and hasRewardedCachedStatus.

### Updated
- [iOS|Android] Updated Vungle SDK
- [Android] Saypromo to 6.0.26.

### Added
- [iOS|Android] InMobi Ads
- [iOS|Android] Debug log flag
- [Android] Exception handler
- [iOS|Android] Custom properties to Crashlytics

## 2019-11-05
### Fixed
- [Android] Fixed IronSource broadcastReceiver bug.

### Updated
- [Android] Updated Unity-Ads to 3.3.

## 2019-09-14
### Fixed
- [iOS] Fixed SayKitVersionManager bug.

### Updated
- [Android] Bidding
- [iOS] Bidding

### Added
- [iOS] Added extra to rewarded_load and interstitial_load events.
- [Android] Added extra to rewarded_load and interstitial_load events.

## 2019-09-13
### Deleted
- [Android] GameAnalytics
- [iOS] GameAnalytics

### Updated
- [iOS] saypromo to 6.5

## 2019-09-13
### Added
- [Android] Added log SDK versions.
- [Android] Updated SayPromo adapters.
- [iOS] Added log SDK versions.
- [iOS] Updated SayPromo adapters.

## 2019-09-06
### Updated
- [Android] Updated MoPub to 5.8. 
- [Android] Updated all network libraries.

## 2019-09-05
### Added
- [iOS] Waterfall tracking.

## 2019-09-04
### Fixed
- [iOS] AVPlayer bugs. 

### Updated
- [iOS] progress animation.
- [iOS] saypromo to 6.3


## 2019-08-30
### Fixed
- Storage bugs.
- [Android] MoPub network name in impression data.
- [iOS] MPSayMediationSerializeConfiguration log for MoPub adapters.
- [iOS] NSLocation warning.

### Added
- RemoteConfigManager crash handling.

### Updated
- [Android] Saypromo to 5.0.22.0. Added remote debug log.
- [iOS] Updated MoPub to 5.8. 
- [iOS] Updated all network libraries.
- [iOS] Updated saypromo to version 6.1.


## 2019-07-24
### Added
- Autoconfigurator for Android.
- `isInterstitialAvailable` method to SayKit.cs.
- SayKit folders check.
- `UNITY_CLOUD_BUILD` flag to Notifications.cs file. [iOS]
- `APP_SECRET_IOS` and `APP_SECRET_ANDROID` to SayKit.cs.
- Support last Unity versions: **2018.4.4f1** and **2018.4.4f1**
- Remote configurations for `facebook_app_id`, `facebook_app_name`, `gameAnalyticsGameKey`, `gameAnalyticsGameSecret`.

### Updated
- Library versions. [Android] 

### Migration steps:
- Delete `<uses-sdk android:minSdkVersion="16"/>` line from a _Assets/Plugins/Android/saykit/AndroidManifest.xml_ file.
- Update gradle version in a _Assets/Plugins/Android/mainTemplate.gradle_ file:

	Change `classpath 'com.android.tools.build:gradle:3.0.1'` to `classpath 'com.android.tools.build:gradle:3.2.1'`

 **[Important]** You can check all configurations in a _Assets/SayKit/Internal/Plugins/Settings_ folder or delete all android specific files if you didn't customize them. All necessary files will be configured automatically at the pre-build time.
 
- Delete `gameAnalyticsGameKey`, `gameAnalyticsGameSecret`, `FACEBOOK_APP_ID` and `FACEBOOK_APP_NAME` from a SayKitApp.cs file.
- Delete banner init from SayKitApp.cs:
 
    ```
    if (SayKit.isPremium == false) {
    	SayKit.showBanner();
    }
    ```

## 2019-06-03
### Added
- Updated MoPub framework to 5.7.

## 2018-11-26
### Added
- Changelog was added.

### Fixed
- Rate App popup crashes on Android 4-6.
