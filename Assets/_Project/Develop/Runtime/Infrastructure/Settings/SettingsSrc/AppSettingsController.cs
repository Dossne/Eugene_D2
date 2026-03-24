using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.HapticControl;
using Infrastructure.MainUICanvasControl;
using Infrastructure.PersistentProgress;
using Infrastructure.Popups;
using Infrastructure.QualityGraphicsControl;
using Infrastructure.AudioControl;
using Infrastructure.Configs;
using Infrastructure.SceneManagement;
using Infrastructure.SystemModules;
using Progress = Infrastructure.PersistentProgress.Progress;
using Features.LifeUi;
using Infrastructure.CameraControl;
using Infrastructure.PurchaseSystem;
using UnityEngine;
using QualityLevel = Infrastructure.QualityGraphicsControl.QualityLevel;
using Features.Social;
using R3;


namespace Infrastructure.Settings
{
    public class AppSettingsController : ISavable
    {
        private readonly SystemSettingsConfig config;
        private readonly IPurchaseManager purchaseManager;
        private readonly PopupService popupService;
        private readonly GraphicsQualityHandler graphicsHandler;
        private readonly CameraService cameraService;
        private readonly Overlay3dCameraService overlay3dCameraService;
        private readonly SettingsUIButton settingsUIButton;
        private readonly BuildInformationService buildInfoService;
        private readonly SdkService sdkService;
        private readonly CancellationTokenSource cts;
        private readonly SceneLoadController sceneLoadController;
        private readonly LifeUiController lifeUiController;
        private readonly SocialService socialService;
        private readonly UserAuthenticateEvent userAuthenticateEvent;
        private readonly CompositeDisposable disposables;

        private SettingsPopup popup;

        private bool isMusicOn;
        private bool isSoundOn;
        private bool isHapticOn;
        private int graphicsLevel;
        private int fps;
        private int launchCount;


        public AppSettingsController(
            IPurchaseManager        purchaseManager,
            ConfigProvider          configProvider,
            GraphicsQualityHandler  graphicsHandler,
            CameraService           cameraService,
            Overlay3dCameraService  overlay3dCameraService,
            MainUIProvider          uiProvider,
            PopupService            popupService,
            BuildInformationService buildInfoService,
            SceneLoadController     sceneLoadController,
            LifeUiController        lifeUiController,
            SdkService        sdkService,
            SocialService socialService,
            UserAuthenticateEvent userAuthenticateEvent)
        {
            this.purchaseManager = purchaseManager;
            this.config = configProvider.SystemSettingsConfig;
            this.sceneLoadController = sceneLoadController;
            this.lifeUiController = lifeUiController;
            this.graphicsHandler = graphicsHandler;
            this.cameraService = cameraService;
            this.overlay3dCameraService = overlay3dCameraService;
            this.settingsUIButton = uiProvider.HudProvider.SettingsUIButton;
            this.popupService = popupService;
            this.buildInfoService = buildInfoService;
            this.sdkService = sdkService;
            this.socialService = socialService;
            this.userAuthenticateEvent = userAuthenticateEvent;
            cts = new CancellationTokenSource();
            disposables = new CompositeDisposable();
        }


        public void Initialize()
        {
            userAuthenticateEvent.Subscribe(_ => { UpdatePopup(); }).AddTo(disposables);

            settingsUIButton.Initialize();
            settingsUIButton.OnClick += SettingsUIButton_OnClick;

            InitializeSystemsBySettings();

            launchCount++;
        }


        public void Deinitialize()
        {
            disposables.Dispose();
            settingsUIButton.Deinitialize();
            settingsUIButton.OnClick -= SettingsUIButton_OnClick;

            if (popup != null)
            {
                popup.OnToggleMusic -= SettingsPopup_OnToggleMusic;
                popup.OnToggleSound -= SettingsPopup_OnToggleSound;
                popup.OnToggleHaptic -= SettingsPopup_OnToggleHaptic;
                popup.OnToggleGraphic -= SettingsPopup_OnToggleGraphic;
                popup.OnToggleFps -= SettingsPopup_OnToggleFps;
                popup = null;
            }

            DeinitializeSystems();
        }

        void ISavable.Load(Progress progress)
        {
            launchCount = progress.appState.launchCount;
            isMusicOn = progress.appState.appSettings.isMusicOn;
            isSoundOn = progress.appState.appSettings.isSoundOn;
            isHapticOn = progress.appState.appSettings.isHapticOn;
            graphicsLevel = progress.appState.appSettings.graphicsLevel;
            fps = progress.appState.appSettings.fps;
        }


        void ISavable.Save(Progress progress)
        {
            progress.appState.launchCount = launchCount;
            progress.appState.appSettings.isMusicOn = isMusicOn;
            progress.appState.appSettings.isSoundOn = isSoundOn;
            progress.appState.appSettings.isHapticOn = isHapticOn;
            progress.appState.appSettings.graphicsLevel = graphicsLevel;
            progress.appState.appSettings.fps = fps;
        }


        public async UniTask OpenPopupAsync(CancellationToken cancellationToken)
        {
            if (popup == null)
            {
                popup = await popupService.GetAsync<SettingsPopup>(cancellationToken);
                popup.Construct(sceneLoadController, 
                    purchaseManager, 
                    lifeUiController, 
                    sdkService, 
                    socialService.GetPlayerName(),
                    buildInfoService.GetBuildInfo(),
                    config.SettingsData.lowFps,
                    config.SettingsData.highFps);
                popup.SetMusic(isMusicOn, config.SettingsData.haveMusic);
                popup.SetSound(isSoundOn, config.SettingsData.haveSound);
                popup.SetHaptic(isHapticOn, config.SettingsData.haveHaptic);
                popup.SetGraphics(graphicsLevel > 0, config.SettingsData.haveGraphicSettings);
                popup.SetFps(fps > config.SettingsData.lowFps, config.SettingsData.haveFpsSettings);

                popup.Initialize();

                popup.OnToggleMusic += SettingsPopup_OnToggleMusic;
                popup.OnToggleSound += SettingsPopup_OnToggleSound;
                popup.OnToggleHaptic += SettingsPopup_OnToggleHaptic;
                popup.OnToggleGraphic += SettingsPopup_OnToggleGraphic;
                popup.OnToggleFps += SettingsPopup_OnToggleFps;
            }

            popup.Open();
        }


        public void UpdatePopup()
        {
            if (popup != null)
            {
                popup.SetPlayerName(socialService.GetPlayerName());
            }
        }


        private void InitializeSystemsBySettings()
        {
            bool isFirstLaunch = launchCount == 0;
            InitializeGraphics(isFirstLaunch);
            InitializeCamera();
            InitializeAudio(isFirstLaunch);
            InitializeHaptics(isFirstLaunch);
            InitializeGameplay();
        }

        private void InitializeGameplay()
        {
            Physics.gravity = Vector3.down * config.SettingsData.gravity;
        }

        private void DeinitializeSystems()
        {
            DeinitializeCamera();
        }


        private void InitializeGraphics(bool isFirstLaunch)
        {
            if (isFirstLaunch)
            {
                fps = config.SettingsData.highFps;
                graphicsLevel = QualityLevel.High;
            }

            graphicsHandler.Initialize(graphicsLevel, fps, isFirstLaunch, config.SettingsData);
            
            if (isFirstLaunch) //after device check in GraphicsQualityHandler
                graphicsLevel = graphicsHandler.CurrentQualityLevel;
        }


        private void InitializeCamera()
        {
            cameraService.Initialize();
            overlay3dCameraService.Initialize();
        }


        private void DeinitializeCamera()
        {
            cameraService.Deinitialize();
            overlay3dCameraService.Deinitialize();
        }


        private void InitializeAudio(bool isFirstLaunch)
        {
            if (isFirstLaunch)
            {
                isMusicOn = config.SettingsData.haveMusic;
                isSoundOn = config.SettingsData.haveSound;
            }
            
            AudioService.I.SetMusicEnabled(isMusicOn && config.SettingsData.haveMusic);
            AudioService.I.SetSoundEnabled(isSoundOn && config.SettingsData.haveSound);
        }


        private void InitializeHaptics(bool isFirstLaunch)
        {
            if (isFirstLaunch)
            {
                isHapticOn = config.SettingsData.haveHaptic;
            }
            
            HapticService.I.SetHapticEnabled(isHapticOn && config.SettingsData.haveHaptic);
        }


        private void SettingsUIButton_OnClick()
        {
            OpenPopupAsync(cts.Token).Forget();
        }


        private void SettingsPopup_OnToggleMusic(bool isOn)
        {
            AudioService.I.SetMusicEnabled(isOn);
            isMusicOn = isOn;
        }


        private void SettingsPopup_OnToggleSound(bool isOn)
        {
            AudioService.I.SetSoundEnabled(isOn);
            isSoundOn = isOn;
        }


        private void SettingsPopup_OnToggleHaptic(bool isOn)
        {
            HapticService.I.SetHapticEnabled(isOn);
            isHapticOn = isOn;
        }


        private void SettingsPopup_OnToggleGraphic(bool isOn)
        {
            int level = isOn ? 1 : 0;
            graphicsHandler.SetGraphics(level);
            overlay3dCameraService.Overlay3DCameraRefreshRt();
            graphicsLevel = level;

            //UltimateJoystick.DisableJoystick(InputData.Joystick);
            //GameManager.Instance.PRQualitySettings.SetGraphics(value);
            // MainUiManager.Instance.CanvasHelper.SetSafeAreaAndBanner(SayKit.isPremium);
            //UltimateJoystick.EnableJoystick(InputData.Joystick);
        }


        private void SettingsPopup_OnToggleFps(bool isOn)
        {
            fps = isOn ? config.SettingsData.highFps : config.SettingsData.lowFps;

            graphicsHandler.SetFps(fps);
        }
    }
}