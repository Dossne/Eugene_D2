using System;
using System.Globalization;
using SayKitInternal;
using UnityEngine;
using UnityEngine.UI;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable RedundantNameQualifier

#endregion

public class SayKitInterstitialCirclePopup : MonoBehaviour
{
    private static SayKitInterstitialCirclePopup _instance;

    [SerializeField] private Text timerText;
    [SerializeField] private Image progressBar;

    private static string _place;
    private float _remainingTime;
    public bool isShowing;
    private bool _isPaused;
    private bool _shouldSkipUpdate;
    private float _duration;
    private Action _interstitialCloseCallback;

    public static SayKitInterstitialCirclePopup GetInstance()
    {
        if (!_instance)
        {
            _instance =
                SayKitInternal.SKUtils.FindComponentOnRootObjects<SayKitInterstitialCirclePopup>() ??
                Instantiate(SayKitAssets.Instance.SayKitInterstitialCirclePopup);

            var canvas = _instance.GetComponent<Canvas>();
            SKUtils.SetupCanvas(canvas);

            DontDestroyOnLoad(_instance.gameObject);
            _instance.name = "[SayKitInterstitialCirclePopup]";
        }

        return _instance;
    }

    private void Update()
    {
        if (isShowing && !_isPaused)
        {
            if (_shouldSkipUpdate)
            {
                _shouldSkipUpdate = false;
                return;
            }

            _remainingTime -= Time.unscaledDeltaTime;

            if (_remainingTime <= 0f)
            {
                _remainingTime = 0f;
                isShowing = false;
                HidePopUp("sk_interstitial_popup_hide", false);
            }

            var remainingTime = Mathf.Max(_remainingTime, 0f);
            
            if (progressBar != null)
            {
                progressBar.fillAmount = remainingTime / _duration;
            }
            
            if (timerText != null)
            {
                timerText.text = Mathf.Ceil(remainingTime).ToString(CultureInfo.InvariantCulture);
            }
        }
    }

    public void ShowPopup(string place, Action onInterstitialClosed, int countdown)
    {
        if (isShowing)
        {
            SKBridgeManager.Instance.TrackEvent("sk_interstitial_popup_skip", extra1: place, extra2: _place);
            return;
        }

        _interstitialCloseCallback = onInterstitialClosed;
        _place = place;

        var instance = GetInstance();
        if (instance != null)
        {
            instance.InternalShow(countdown: countdown);
        }
    }

    private void InternalShow(int countdown)
    {
        _duration = countdown > 0 ? countdown : SKManager.Instance.RemoteConfig.ads_settings.sk_interstitial_popup_delay;
        _remainingTime = _duration;

        if (timerText != null)
        {
            timerText.text = Mathf.Ceil(_remainingTime).ToString(CultureInfo.InvariantCulture);
        }
        
        SKBridgeManager.Instance.TrackEvent("sk_interstitial_popup_show", param2: countdown, extra1: _place);

        gameObject.SetActive(true);
        isShowing = true;
        _isPaused = false;
        _shouldSkipUpdate = false;
    }

    private void HidePopUp(string eventName, bool force, string rewardPlace = "")
    {
        gameObject.SetActive(false);
        isShowing = false;

        SKBridgeManager.Instance.TrackEvent(eventName, extra1: _place,
            extra2: string.IsNullOrEmpty(rewardPlace) ? string.Empty : $"rewarded_placement: {rewardPlace}");

        if (!force)
        {
            AdsManager.Instance.ShowInterstitial(place: _place, onCloseCallback: _interstitialCloseCallback);
        }
        else
        {
            _interstitialCloseCallback?.Invoke();
            _interstitialCloseCallback = null;
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (isShowing)
        {
            if (pauseStatus)
            {
                _isPaused = true;
            }
            else
            {
                _isPaused = false;
                _shouldSkipUpdate = true;
            }
        }
    }
}