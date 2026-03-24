using System;
using Features.LifeUi;
using Features.Social;
using Infrastructure.Ads;
using Infrastructure.BroTweens;
using Infrastructure.HapticControl;
using Infrastructure.Localization;
using Infrastructure.Popups;
using Infrastructure.PurchaseSystem;
using Infrastructure.SceneManagement;
using Infrastructure.SystemModules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Infrastructure.Settings
{
    public class SettingsPopup : PopupBase
    {
        public event Action<bool> OnToggleMusic;
        public event Action<bool> OnToggleSound;
        public event Action<bool> OnToggleHaptic;
        public event Action<bool> OnToggleGraphic;
        public event Action<bool> OnToggleFps;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI header;

        [Header("Music")]
        [SerializeField] private Toggle music;
        [SerializeField] private ToggleAnimation musicAnim;
        [SerializeField] private TextMeshProUGUI musicTxt;

        [Header("Sound")]
        [SerializeField] private Toggle sounds;
        [SerializeField] private ToggleAnimation soundsAnim;
        [SerializeField] private TextMeshProUGUI soundsTxt;

        [Header("Haptic")]
        [SerializeField] private Toggle haptic;
        [SerializeField] private ToggleAnimation hapticAnim;
        [SerializeField] private TextMeshProUGUI hapticTxt;

        [Header("Graphics")]
        [SerializeField] private Toggle graphics;
        [SerializeField] private ToggleAnimation graphicsAnim;
        [SerializeField] private TextMeshProUGUI graphicTxt;

        [Header("Fps")]
        [SerializeField] private Toggle fps;
        [SerializeField] private ToggleAnimation fpsAnim;
        [SerializeField] private TextMeshProUGUI fpsTxt;

        [Header("Restore Purchases")]
        [SerializeField] private Button restorePurchasesButton;
        [SerializeField] private TextMeshProUGUI restorePurchasesTxt;

        [Header("Withdraw Consent")]
        [SerializeField] private Button withdrawConsentButton;
        [SerializeField] private TextMeshProUGUI withdrawConsentTxt;

        [Header("Support")]
        [SerializeField] private Button supportButton;
        [SerializeField] private TextMeshProUGUI supportTxt;

        [Header("Other")]
        [SerializeField] private TextMeshProUGUI playerNameTxt;
        [SerializeField] private TextMeshProUGUI buildInfoTxt;
        [SerializeField] private TextMeshProUGUI leaveLevelBtnTxt;
        [SerializeField] private Button leaveLevelBtn;

        private SceneLoadController sceneLoadController;
        private IPurchaseManager purchaseManager;
        private LifeUiController lifeUiController;
        private SdkService sdkService;
        private string playerName;
        private string buildInfo;
        private int minFps;
        private int maxFps;


        public void Construct(SceneLoadController sceneLoadController,
                              IPurchaseManager purchaseManager,
                              LifeUiController lifeUiController,
                              SdkService sdkService,
                              string playerName,
                              string buildInfo,
                              int minFps,
                              int maxFps)
        {
            this.sceneLoadController = sceneLoadController;
            this.purchaseManager = purchaseManager;
            this.lifeUiController = lifeUiController;
            this.sdkService = sdkService;
            this.playerName = playerName;
            this.buildInfo = buildInfo;
            this.minFps = minFps;
            this.maxFps = maxFps;
        }


        public void SetMusic(bool isOn, bool isActive)
        {
            SetToggle(music, isOn, isActive);
        }


        public void SetSound(bool isOn, bool isActive)
        {
            SetToggle(sounds, isOn, isActive);
        }


        public void SetHaptic(bool isOn, bool isActive)
        {
            SetToggle(haptic, isOn, isActive);
        }


        public void SetGraphics(bool isOn, bool isActive)
        {
            SetToggle(graphics, isOn, isActive);
        }


        public void SetFps(bool isOn, bool isActive)
        {
            SetToggle(fps, isOn, isActive);
        }


        public void SetPlayerName(string playerName)
        {
            playerNameTxt.text = $"Player Name: {playerName}";
        }


        protected override void OnInitialize()
        {
            header.text = LocalizationService.I.Get(LocKeys.Settings.Title);
            musicTxt.text = LocalizationService.I.Get(LocKeys.Settings.Music);
            soundsTxt.text = LocalizationService.I.Get(LocKeys.Settings.Sound);
            hapticTxt.text = LocalizationService.I.Get(LocKeys.Settings.Haptic);
            graphicTxt.text = LocalizationService.I.Get(LocKeys.Settings.Quality);

            fpsTxt.text = LocalizationService.I.Get(LocKeys.Settings.Fps, minFps.ToString(), maxFps.ToString());

            leaveLevelBtnTxt.text = LocalizationService.I.Get(LocKeys.Settings.LeaveLevel);
            SetPlayerName(playerName);
            buildInfoTxt.text = buildInfo;

            restorePurchasesTxt.text = LocalizationService.I.Get(LocKeys.Settings.RestorePurchases);

            withdrawConsentTxt.text = LocalizationService.I.Get(LocKeys.Settings.WithdrawConsent);
            withdrawConsentButton.gameObject.SetActive(Advertisement.IsGdprApplicable() ?? false);

            supportTxt.text = LocalizationService.I.Get(LocKeys.Support.SupportButton);

            Subscribe();
        }


        protected override void OnDeinitialize()
        {
            Unsubscribe();
        }


        protected override void OnBeginOpen()
        {
            musicAnim.PlayAnimation(music.isOn);
            soundsAnim.PlayAnimation(sounds.isOn);
            hapticAnim.PlayAnimation(haptic.isOn);
            graphicsAnim.PlayAnimation(graphics.isOn);
            fpsAnim.PlayAnimation(fps.isOn);
            leaveLevelBtn.gameObject.SetActive(sceneLoadController.IsShowQuitButton);
        }


        protected override void OnBeginClose()
        {

        }


        protected override void OnDispose()
        {
            OnToggleMusic = null;
            OnToggleSound = null;
            OnToggleHaptic = null;
            OnToggleGraphic = null;
            OnToggleFps = null;
        }


        private void Subscribe()
        {
            haptic.onValueChanged.AddListener(ChangeHapticValue);
            sounds.onValueChanged.AddListener(ChangeSoundsValue);
            music.onValueChanged.AddListener(ChangeMusicValue);
            graphics.onValueChanged.AddListener(ChangeQuality);
            fps.onValueChanged.AddListener(ChangeFps);

            restorePurchasesButton.onClick.AddListener(RestorePurchases);
            withdrawConsentButton.onClick.AddListener(RevokeGDPR);
            supportButton.onClick.AddListener(CallSupport);
            leaveLevelBtn.onClick.AddListener(LeaveLevel);
        }

        private void Unsubscribe()
        {
            music.onValueChanged.RemoveListener(ChangeMusicValue);
            sounds.onValueChanged.RemoveListener(ChangeSoundsValue);
            haptic.onValueChanged.RemoveListener(ChangeHapticValue);
            graphics.onValueChanged.RemoveListener(ChangeQuality);
            fps.onValueChanged.RemoveListener(ChangeFps);

            restorePurchasesButton.onClick.RemoveListener(RestorePurchases);
            withdrawConsentButton.onClick.RemoveListener(RevokeGDPR);
            supportButton.onClick.RemoveListener(CallSupport);
            leaveLevelBtn.onClick.RemoveListener(LeaveLevel);
        }


        private void RestorePurchases()
        {
            BroTween.ClickBounceWithCallBack(restorePurchasesButton, restorePurchasesButton.transform, purchaseManager, target => target.RestorePurchases()).Play();
        }


        private void RevokeGDPR()
        {
            BroTween.ClickBounceWithCallBack(restorePurchasesButton, restorePurchasesButton.transform, this, target => target.RevokeConsent()).Play();
        }


        private void LeaveLevel()
        {
           BroTween.ClickBounceWithCallBack(leaveLevelBtn, leaveLevelBtn.transform, this, target => target.ShowQuitLevelPopup()).Play();
        }

        private void CallSupport()
        {
            BroTween.ClickBounceWithCallBack(supportButton, supportButton.transform, this, target => target.OpenSupportPage()).Play();
        }

        private void OpenSupportPage() 
        {
            sdkService.OpenSupportPage();
        }

        private void ShowQuitLevelPopup()
        {
            Close();
            lifeUiController.ShowQuitLevelPopup();
        }


        private void RevokeConsent()
        {
            Advertisement.RevokeGdprConsent();
        }


        private void Vibrate()
        {
            HapticService.I.HapticLight();
        }


        private void SetToggle(Toggle toggle, bool isOn, bool isActive)
        {
            if (toggle.gameObject.activeSelf != isActive)
                toggle.gameObject.SetActive(isActive);

            if (isActive)
            {
                toggle.isOn = isOn;
            }
        }


        private void ChangeMusicValue(bool value)
        {
            Vibrate();
            musicAnim.PlayAnimation(value);
            OnToggleMusic?.Invoke(value);
        }


        private void ChangeSoundsValue(bool value)
        {
            Vibrate();
            soundsAnim.PlayAnimation(value);
            OnToggleSound?.Invoke(value);
        }


        private void ChangeHapticValue(bool value)
        {
            Vibrate();
            hapticAnim.PlayAnimation(value);
            OnToggleHaptic?.Invoke(value);
        }


        private void ChangeQuality(bool value)
        {
            Vibrate();
            graphicsAnim.PlayAnimation(value);
            OnToggleGraphic?.Invoke(value);
        }


        private void ChangeFps(bool value)
        {
            Vibrate();
            fpsAnim.PlayAnimation(value);
            OnToggleFps?.Invoke(value);
        }
    }
}