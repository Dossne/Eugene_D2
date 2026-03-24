using System;
#pragma warning disable CS0414 // Field is assigned but its value is never used

namespace Infrastructure.Ads
{
    /// <summary>
    /// Wrapper for advertisement service
    /// </summary>
    public static class Advertisement
    {
        public static bool CheatAdsDisabled { get; private set; } = false;
        private static bool cachePremium;
        private static bool isPremiumCached;

        public static void CheatSwitchAdsIsOff()
        {
            CheatAdsDisabled = !CheatAdsDisabled;
            if (CheatAdsDisabled)
                HideBanner();
            else
                ShowBanner();
        }

        public static bool IsInterstitialAvailable(string place, int countdown = 0)
        {
#if PR_SAYKIT_ENABLED
            return SayKit.isInterstitialAvailable(place, countdown);
#else
            return false;
#endif
        }

        public static void ShowInterstitial(string place, Action onCloseCallback = null, int countdown = 0)
        {
            if (IsPremium() || !IsInterstitialAvailable(place, countdown) || CheatAdsDisabled)
            {
                onCloseCallback?.Invoke();
                return;
            }
#if PR_SAYKIT_ENABLED
            SayKit.showInterstitial(place, onCloseCallback);
#endif
        }

        public static bool IsRewardedAvailable(string place)
        {
#if PR_SAYKIT_ENABLED
            return SayKit.isRewardedAvailable(place);
#else
            return false;
#endif
        }

        public static void ShowRewarded(string place, Action<bool> onCloseCallback)
        {
            if (CheatAdsDisabled)
            {
                onCloseCallback?.Invoke(true);
                return;
            }
#if PR_SAYKIT_ENABLED
            SayKit.showRewarded(place, onCloseCallback);
#endif
        }

        public static bool IsPremium()
        {
#if PR_SAYKIT_ENABLED
            cachePremium = isPremiumCached ? cachePremium : SayKit.isPremium;
#endif
            isPremiumCached = true;
            return cachePremium;
        }

        public static void EnablePremium()
        {
#if PR_SAYKIT_ENABLED
            SayKit.enablePremium();
#endif
            isPremiumCached = true;
            cachePremium    = true;
        }

        public static void DisablePremium()
        {
#if PR_SAYKIT_ENABLED
            SayKit.disablePremium();
#endif
            isPremiumCached = true;
            cachePremium    = false;
        }

        public static bool ShowRateAppPopup()
        {
#if PR_SAYKIT_ENABLED
            return SayKit.showRateAppPopup();
#else
            return false;
#endif
        }

        public static bool ShowCustomRateAppPopup(int rate = 0)
        {
#if PR_SAYKIT_ENABLED
            return SayKit.showCustomRateAppPopup(rate);
#else
            return false;
#endif
        }

        public static bool IsRateAppPopupShown()
        {
#if PR_SAYKIT_ENABLED
            return SayKit.IsRateAppPopupShown();
#else
            return false;
#endif
        }

        public static void RevokeGdprConsent()
        {
#if PR_SAYKIT_ENABLED
            SayKit.revokeGdprConsent();
#endif
        }

        public static bool GetGdprStatus()
        {
#if PR_SAYKIT_ENABLED
            return SayKit.GetGdprStatus();
#else
            return false;
#endif
        }

        public static bool? IsGdprApplicable()
        {
#if PR_SAYKIT_ENABLED
            return SayKit.isGdprApplicable();
#else
            return false;
#endif
        }

        public static void ShowBanner()
        {
            if (CheatAdsDisabled)
                return;
#if PR_SAYKIT_ENABLED
            SayKit.showBanner();
#endif
        }

        public static void HideBanner()
        {
#if PR_SAYKIT_ENABLED
            SayKit.hideBanner();
#endif
        }

        public static float GetBackgroundBannerSize()
        {
#if PR_SAYKIT_ENABLED
            return SayKit.GetBackgroundBannerSize();
#else
            return 0;
#endif
        }
    }
}