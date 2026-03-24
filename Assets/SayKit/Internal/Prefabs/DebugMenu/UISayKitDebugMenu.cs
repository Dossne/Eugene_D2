using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    public class UISayKitDebugMenu : MonoBehaviour
    {
        private static UISayKitDebugMenu _instance;

        private string _selectedVersion;

        public Text configText;
        public Text sayKitVersionText;
        public Text idfaText;
        public Text ipCountryText;
        public Text localTimeText;
        public Text appKeyText;
        public Text gdprText;
        public Text sayKitLoadTime;
        public Text skAdNetworkStatus;
        public Text thermalState;
        public Text rateApp;

        public Button closeButton;
        public Button closeAndReloadButton;
        public Text selectVersionTitleText;

        public Button rewardedOffBtn;
        public Button interstitialOffBtn;
        public Button bannerOffBtn;

        public GameObject versionScrollViewContent;
        public UISayKitVersionItem versionCardView;
        private List<UISayKitVersionItem> _versionCards = new List<UISayKitVersionItem>();

        public Text minFPSRateText;
        public Text minFPSSpikesText;
        public Text fpsTagsText;

        public Button reloadConfigBtn;

        public Button openConfigsBtn;
        public Button closeConfigBtn;
        public GameObject goConfigsWindow;
        public Button medDebuggerBtn;

        public GameObject goAdsInfoWindow;
        public GameObject adInfoScrollViewContent;
        public UISayKitAdInfoItem adInfoCardView;
        private List<UISayKitAdInfoItem> _adInfoCards = new List<UISayKitAdInfoItem>();
        public Button openAdsInfoBtn;
        public Button closeAdsInfoBtn;
        public GameObject toast;
        private long _lastThermalCheckTimestampMs;

        public static UISayKitDebugMenu GetInstance()
        {
            if (_instance != null) return _instance;

            var components = SayKitUI.getInstance().GetComponentsInChildren(typeof(UISayKitDebugMenu), true);

            foreach (var component in components)
            {
                if (component.GetComponent<UISayKitDebugMenu>() != null)
                {
                    component.name = "[SayKitDebugMenuUI]";

                    DontDestroyOnLoad(component);
                    _instance = component.GetComponent<UISayKitDebugMenu>();

                    _instance.closeButton.onClick.AddListener(_instance.OnCloseBtnClicked);
                    _instance.closeAndReloadButton.onClick.AddListener(_instance.OnCloseAndReloadBtnClicked);

                    _instance.rewardedOffBtn.onClick.AddListener(_instance.OnRewardedOffBtnClick);
                    _instance.interstitialOffBtn.onClick.AddListener(_instance.OnInterstitialOffBtnClick);
                    _instance.bannerOffBtn.onClick.AddListener(_instance.OnBannerOffBtnClick);

                    _instance.reloadConfigBtn.onClick.AddListener(_instance.OnReloadConfigOffBtnClick);

                    _instance.openAdsInfoBtn.onClick.AddListener(_instance.OpenAdsInfoWindow);
                    _instance.closeAdsInfoBtn.onClick.AddListener(_instance.CloseAdsInfoWindow);

                    _instance.openConfigsBtn.onClick.AddListener(_instance.OpenConfigsWindow);
                    _instance.closeConfigBtn.onClick.AddListener(_instance.CloseConfigsWindow);
                    _instance.medDebuggerBtn.onClick.AddListener(_instance.OpenMediationDebugger);
                }
            }

            return _instance;
        }

        private void Update()
        {
            if (SKUtils.currentTimestampMs - _lastThermalCheckTimestampMs < 1000)
            {
                return;
            }

            _lastThermalCheckTimestampMs = SKUtils.currentTimestamp;
            
            if (thermalState)
            {
                thermalState.text = "Thermal: " + SKBridgeManager.Instance.GetThermalState();
            }
        }

        public void ShowPopup()
        {
            SayKitToast.Toast = toast;

            gameObject.SetActive(true);

            if (goAdsInfoWindow)
            {
                goAdsInfoWindow.SetActive(false);
            }

            if (goConfigsWindow)
            {
                goConfigsWindow.SetActive(false);
            }

            if (configText)
            {
                configText.text = "Config version: " + SKManager.Instance.BuildVersion;
                _selectedVersion = SKManager.Instance.BuildVersion;
            }

            if (sayKitVersionText)
            {
                sayKitVersionText.text = "SayKit version: " + SKManager.Instance.Version;
            }

            if (idfaText)
            {
                idfaText.text = "IDFA: " + SKManager.Instance.RuntimeInfo.idfa;
            }

            if (ipCountryText)
            {
                ipCountryText.text = "Country: " + SKManager.Instance.RemoteConfig.runtime.country;
            }

            if (localTimeText)
            {
                localTimeText.text = "Local time: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            }

            if (appKeyText)
            {
                appKeyText.text = "App key: " + SKManager.Instance.Config.appKey;
            }

            if (gdprText)
            {
                gdprText.text = "GDPR: " + SKBridgeManager.Instance.GetGdprStatus();
            }

            if (sayKitLoadTime)
            {
                var loadDuration = SKUtils.currentTimestamp - DebugService.Instance.startInitTimeStamp;
                sayKitLoadTime.text = "SayKit load time: " + loadDuration;
            }

            if (skAdNetworkStatus)
            {
                skAdNetworkStatus.text = "SKad: " + SKBridgeManager.Instance.GetATTStatus();
            }

            if (thermalState)
            {
                thermalState.text = "Thermal state: " + SKBridgeManager.Instance.GetThermalState();
            }

            if (rateApp)
            {
                rateApp.text = "Rate app: " + SKBridgeManager.Instance.GetRateAppShown();
            }

            if (minFPSRateText)
            {
                minFPSRateText.text = "Min FPS rate: " + PerfomanceManager.MinFPSRate;
            }

            if (minFPSSpikesText)
            {
                minFPSSpikesText.text = "Min FPS spikes: " + PerfomanceManager.MinSpikes;
            }

            if (fpsTagsText)
            {
                fpsTagsText.text = "";

                if (PerfomanceManager.LastTrackedTags.Count > 0)
                {
                    var maxLines = 6;

                    for (var i = PerfomanceManager.LastTrackedTags.Count - 1; i >= 0; i--)
                    {
                        if (maxLines == 0)
                        {
                            break;
                        }

                        fpsTagsText.text += PerfomanceManager.LastTrackedTags[i] + "\n";
                        maxLines--;
                    }
                }
                else
                {
                    fpsTagsText.text = "Tags wasn't detected";
                }
            }

            if (reloadConfigBtn != null)
            {
                UpdateReloadConfigBtn();
            }
        }

        public void HidePopup()
        {
            gameObject.SetActive(false);
        }

        private void UpdateSelectedVersionInList()
        {
            foreach (var card in _versionCards)
            {
                var image = card.selectButton.GetComponent<Image>();

                if (image)
                {
                    image.color = card.titleText.text == _selectedVersion ? new Color(3f / 255f, 192f / 255f, 129f / 255f)
                        : new Color(248f / 255f, 202f / 255f, 78f / 255f);
                }
            }
        }

        public void SelectNewConfigVersion(string version)
        {
            _selectedVersion = version;
            UpdateSelectedVersionInList();

            SayKitDebug.Log("Version selected: " + version);

            DebugService.Instance.ChangeConfigVersion(version);

            if (selectVersionTitleText)
            {
                selectVersionTitleText.text = "UPDATING TO " + version;
            }
        }

        public void ConfigUpdated(bool updated)
        {
            if (updated)
            {
                if (configText)
                {
                    configText.text = "Config version: " + SKManager.Instance.BuildVersion;
                }

                if (selectVersionTitleText)
                {
                    selectVersionTitleText.text = "VERSION UPDATED";
                }
            }
            else
            {
                if (selectVersionTitleText)
                {
                    selectVersionTitleText.text = "VERSION UPDATE IS FAILED!";
                }
            }
        }

        private void OnCloseBtnClicked()
        {
            SayKitUI.instance.HideDebugMenu();
            gameObject.SetActive(false);
        }

        private void OnCloseAndReloadBtnClicked()
        {
            Application.Quit();
        }

        private void OnRewardedOffBtnClick()
        {
            var textLabel = rewardedOffBtn.GetComponentInChildren<Text>();
            var image = rewardedOffBtn.GetComponent<Image>();

            DebugService.Instance.RewardedDisabled = !DebugService.Instance.RewardedDisabled;

            if (textLabel && image)
            {
                if (DebugService.Instance.RewardedDisabled)
                {
                    textLabel.text = "Rewarded Off";
                    image.color = Color.gray;
                }
                else
                {
                    textLabel.text = "Rewarded On";
                    image.color = Color.white;
                }
            }
        }

        private void OnInterstitialOffBtnClick()
        {
            var textLabel = interstitialOffBtn.GetComponentInChildren<Text>();
            var image = interstitialOffBtn.GetComponent<Image>();

            DebugService.Instance.InterstitialDisabled = !DebugService.Instance.InterstitialDisabled;

            if (textLabel && image)
            {
                if (DebugService.Instance.InterstitialDisabled)
                {
                    textLabel.text = "Interstitial Off";
                    image.color = Color.gray;
                }
                else
                {
                    textLabel.text = "Interstitial On";
                    image.color = Color.white;
                }
            }
        }

        private void OnBannerOffBtnClick()
        {
            var textLabel = bannerOffBtn.GetComponentInChildren<Text>();
            var image = bannerOffBtn.GetComponent<Image>();

            DebugService.Instance.BannerDisabled = !DebugService.Instance.BannerDisabled;

            if (textLabel && image)
            {
                if (DebugService.Instance.BannerDisabled)
                {
                    textLabel.text = "Banner Off";
                    image.color = Color.gray;

                    SKManager.Instance.HideBanner();
                }
                else
                {
                    textLabel.text = "Banner On";
                    image.color = Color.white;

                    SKManager.Instance.ShowBanner();
                }
            }
        }

        private void OnReloadConfigOffBtnClick()
        {
            if (DebugService.Instance.ReloadConfigEnabled)
            {
                DebugService.Instance.ReloadConfigEnabled = false;
                SayKitUI.instance.StopCoroutine(DebugService.Instance.ReloadRemoteConfig());
            }
            else
            {
                DebugService.Instance.ReloadConfigEnabled = true;
                SayKitUI.instance.StartCoroutine(DebugService.Instance.ReloadRemoteConfig());
            }

            UpdateReloadConfigBtn();
        }

        private void UpdateReloadConfigBtn()
        {
            var textLabel = reloadConfigBtn.GetComponentInChildren<Text>();
            var image = reloadConfigBtn.GetComponent<Image>();

            if (textLabel && image)
            {
                if (DebugService.Instance.ReloadConfigEnabled)
                {
                    textLabel.text = "Reload config On";
                    image.color = Color.white;
                }
                else
                {
                    textLabel.text = "Reload config Off";
                    image.color = Color.gray;
                }
            }
        }

        private void OpenAdsInfoWindow()
        {
            if (goAdsInfoWindow != null)
            {
                if (adInfoScrollViewContent != null)
                {
                    foreach (var t in DebugService.Instance.SayKitAdsInfoBuffer)
                    {
                        if (adInfoCardView == null)
                        {
                            continue;
                        }

                        var versionCard = Instantiate(adInfoCardView, adInfoScrollViewContent.transform, false);

                        if (versionCard)
                        {
                            versionCard.adTime.text = $"AdTime: {t.AdTime}";
                            versionCard.adType.text = $"AdType: {t.AdType}";
                            versionCard.adNetwork.text = $"Ad Network: {t.AdNetwork}";
                            versionCard.creativeId.text = $"Creative Id: {t.CreativeId}";

                            _adInfoCards.Add(versionCard);
                        }
                    }
                }

                goAdsInfoWindow.SetActive(true);
            }
        }

        private void OpenConfigsWindow()
        {
            if (goConfigsWindow != null)
            {
                if (versionScrollViewContent != null)
                {
                    foreach (var versionDTO in DebugService.Instance.VersionList)
                    {
                        var versionCard = Instantiate(versionCardView, versionScrollViewContent.transform, false);
                        versionCard.titleText.text = versionDTO.Version;
                        versionCard.descriptionText.text = versionDTO.Comment;

                        _versionCards.Add(versionCard);
                    }

                    UpdateSelectedVersionInList();
                }

                goConfigsWindow.SetActive(true);
            }
        }

        private void CloseConfigsWindow()
        {
            if (goConfigsWindow != null)
            {
                goConfigsWindow.SetActive(false);

                if (_versionCards.Count > 0)
                {
                    foreach (var card in _versionCards)
                    {
                        Destroy(card.gameObject);
                    }

                    _versionCards = new List<UISayKitVersionItem>();
                }
            }
        }

        private void CloseAdsInfoWindow()
        {
            if (goAdsInfoWindow != null)
            {
                goAdsInfoWindow.SetActive(false);

                if (_adInfoCards.Count > 0)
                {
                    foreach (var card in _adInfoCards)
                    {
                        Destroy(card.gameObject);
                    }

                    _adInfoCards = new List<UISayKitAdInfoItem>();
                }
            }
        }

        private void OpenMediationDebugger()
        {
            SKBridgeManager.Instance.ShowMaxMediationDebug();
        }
        
    }
}