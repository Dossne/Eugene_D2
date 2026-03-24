using System;
using UnityEngine;

#region ReSharper

// ReSharper disable AccessToStaticMemberViaDerivedType
// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMethodReturnValue.Global

#endregion

namespace SayKitInternal
{
    class AdsManager
    {
        public static AdsManager Instance { get; } = new AdsManager();
        
        private const string ADS_PLACE_STATUS_OFF = "off";
        
        private Action _interstitialCloseCallback;
        
        private Action<bool> _rewardedCallback;

        private long _lastInterstitialCheckTimestampMs;
        private long _lastRewardedCheckTimestampMs;
        
        private bool _hasInterstitialAd;
        private bool _hasRewardedAd;
        
        
        public void ShowBanner()
        {
#if !SAYKIT_BANNER_DISABLED
            SKBridgeManager.Instance.ShowBanner();
#endif
        }

        public void HideBanner()
        {
#if !SAYKIT_BANNER_DISABLED
            SKBridgeManager.Instance.HideBanner();
#endif
        }

        public bool IsInterstitialAvailable(string place, int countdown = 0)
        {
            if (SKUtils.currentTimestampMs - _lastInterstitialCheckTimestampMs < 500)
            {
                SayKitDebug.LogError($"Method {nameof(IsInterstitialAvailable)} is called more than once within 500 ms.");
                return _hasInterstitialAd;
            }

            _lastInterstitialCheckTimestampMs = SKUtils.currentTimestampMs;

            _hasInterstitialAd = SayKitBridge.Instance.IsInterstitialAvailable(place, countdown);

            return _hasInterstitialAd;
        }

        public bool ShowInterstitialWithTimerPopUp(string place, int countdown, Action onShowCallback, Action onCloseCallback)
        {
            if (!IsInterstitialAvailable(place, countdown))
            {
                return false;
            }

            _interstitialCloseCallback = onCloseCallback;
            onShowCallback?.Invoke();

            SayKitInterstitialCirclePopup.GetInstance().ShowPopup(place, _interstitialCloseCallback, countdown);

            return true;
        }

        public bool ShowInterstitial(string place, Action onCloseCallback = null, Action onShowCallback = null)
        {
            if (!IsInterstitialAvailable(place))
            {
                return false;
            }

            _interstitialCloseCallback = onCloseCallback;
            onShowCallback?.Invoke();

            AudioPause();

            _hasInterstitialAd = false;

            var result = SKBridgeManager.Instance.ShowInterstitial(place);
            if (!result)
            {
                AudioResume();
                _interstitialCloseCallback = null;
               
            }

            return result;
        }

        public bool IsRewardedAvailable(string place)
        {
            if (SKUtils.currentTimestampMs - _lastRewardedCheckTimestampMs < 500)
            {
                return _hasRewardedAd;
            }

            _lastRewardedCheckTimestampMs = SKUtils.currentTimestampMs;
            _hasRewardedAd = SKBridgeManager.Instance.IsRewardedAvailable(place);
            
            return _hasRewardedAd;
        }

        public bool IsRewardedPlacementAvailable(string place)
        {
            if (IsPlaceStatusOff(place, isRewarded: true))
            {
                return false;
            }

            return true;
        }

        public void ShowRewarded(string place, Action<bool> onCloseCallback)
        {
            if (IsRewardedAvailable(place))
            {
                _rewardedCallback = onCloseCallback;

                AudioPause();
                
                var result = SKBridgeManager.Instance.ShowRewarded(place);
                if (result)
                {
                    _hasRewardedAd = false;
                }
                else
                {
                    AudioResume();
                    
                    _rewardedCallback = null;
                    onCloseCallback?.Invoke(false);
                }
            }
            else
            {
                onCloseCallback?.Invoke(false);
            }
        }

        private bool IsPlaceStatusOff(string place, bool isRewarded)
        {
            if (!RemoteConfigManager.Instance.Initialized)
            {
                return false;
            }

            var placeConfig = SKManager.Instance.RemoteConfig.findAdsPlace(place);
            if (placeConfig == null)
            {
                return false;
            }

            if (placeConfig.status == ADS_PLACE_STATUS_OFF)
            {
                return true;
            }

            return false;
        }

        private void AudioPause()
        {
            if (Application.platform != RuntimePlatform.IPhonePlayer) return;
            
            SayKitDebug.Log("[SayKit] AudioPause " + AudioListener.pause);
            AudioListener.pause = true;
        }
        private void AudioResume()
        {
            if (Application.platform != RuntimePlatform.IPhonePlayer) return;
            
            SayKitDebug.Log("[SayKit] AudioResume " + AudioListener.pause);
            AudioListener.pause = false;
        }
        
        #region Callbacks

        internal void OnInterstitialClosed()
        {
            SayKitDebug.Log("AdsManager.OnInterstitialClosed");
            
            AudioResume();
            
            if (_interstitialCloseCallback != null)
            {
                _interstitialCloseCallback();
                _interstitialCloseCallback = null;
            }
        }

        internal void OnRewardedClosed(bool rewarded)
        {
            SayKitDebug.Log("AdsManager.OnRewardedClosed: rewarded=" + rewarded);

            AudioResume();
            
            if (_rewardedCallback != null)
            {
                _rewardedCallback(rewarded);
                _rewardedCallback = null;
            }
        }

        #endregion

    }
}