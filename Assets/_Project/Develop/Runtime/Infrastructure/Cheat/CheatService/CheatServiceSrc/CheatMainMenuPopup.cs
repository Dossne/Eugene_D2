#if PR_CHEAT

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.LevelUp;
using Infrastructure.WalletSystem;
using Infrastructure.AssetManagement;
using Infrastructure.Cheat.CheatGameProgress;
using Infrastructure.Cheat.Debug;
using Infrastructure.Events;
using Infrastructure.PersistentProgress;
using Infrastructure.Popups;
using Infrastructure.SceneManagement;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Infrastructure.Ads;
using Features.Life;
using Features.Boosters;
using Infrastructure.Cheat.CheatLavaQuest;
using Infrastructure.Utilities;
using Infrastructure.Cheat.CheatPurchase;
using Infrastructure.DateTimeControl;
using Features.Tutorial;
using Features.JellyHoleUi;
using Features.SuperSpeedMode;
using Features.PlayerProfile;
using Features.Level;

namespace Infrastructure.Cheat
{
    public class CheatMainMenuPopup : PopupBase
    {
        [Header("Buttons")]
        [SerializeField] private Button progressPanelBtn;
        [SerializeField] private Button debugPanelBtn;
        [SerializeField] private Button dateTimeBtn;
        [SerializeField] private Button timeCyclesBtn;
        [SerializeField] private Button timeScaleBtn;
        [SerializeField] private Button loadFromInputFieldBtn;
        [SerializeField] private Button addCoinsBtn;
        [SerializeField] private Button delCoinsBtn;
        [SerializeField] private Button addLifeBtn;
        [SerializeField] private Button reduceLifeCdBtn;
        [SerializeField] private Button reduceLevelTimeBtn;
        [SerializeField] private Button freezeLevelTimeBtn;
        [SerializeField] private Button switchBombDamageBtn;
        [SerializeField] private Button zoomInBtn;
        [SerializeField] private Button zoomOutBtn;
        [SerializeField] private Button winLevelBtn;
        [SerializeField] private Button resetLifeBtn;
        [SerializeField] private Button adsBtn;
        [SerializeField] private Button infinitePreBoosersOnBtn;
        [SerializeField] private Button infiniteLifeOnBtn;
        [SerializeField] private Button cheatPurchaseBtn;
        [SerializeField] private Button plusOneMinuteBtn;
        [SerializeField] private Button plusFiveMinutesBtn;
        [SerializeField] private Button plusThirtyMinutesBtn;
        [SerializeField] private Button plusFiveHoursBtn;
        [SerializeField] private Button winStreakBtn;
        [SerializeField] private Button rewardTrackBtn;
        [SerializeField] private Button lavaQuestBtn;
        [SerializeField] private Button resetTutorialsBtn;
        [SerializeField] private Button startTutorialsBtn;
        [SerializeField] private Button activateSuperSpeedBtn;
        [SerializeField] private Button loadTestLevelBtn;
        [SerializeField] private Button tmpCheatBtn;
        [SerializeField] private Button competitionBtn;
        [SerializeField] private Button socialBtn;

        [Header("Level")]
        [SerializeField] private Button lvlUpBtn;
        [SerializeField] private Button resetUpBtn;
        [SerializeField] private Button returnButton;
        [SerializeField] private TMP_InputField inputField;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI adsBtnText;
        [SerializeField] private TextMeshProUGUI currentLevelIdTxt;
        [SerializeField] private TextMeshProUGUI cheatPurchaseTxt;
        [SerializeField] private TextMeshProUGUI timeScaleTxt;
        [SerializeField] private TextMeshProUGUI testLevelTxt;

        [Header("Panels")]
        [SerializeField] private CheatGameProgressPanel progressPanel;
        [SerializeField] private DebugPanel debugPanel;
        [SerializeField] private WinStreakPanel cheatWinStreakPanel;
        [SerializeField] private CheatPurchasePanel cheatPurchasePanel;
        [SerializeField] private CheatRewardTrackPanel cheatRewardTrackPanel;
        [SerializeField] private CheatLavaQuestPanel cheatLavaQuestPanel;
        [SerializeField] private CheatSuperSpeedPanel cheatSuperSpeedPanel;
        [SerializeField] private CheatCompetitionPanel cheatCompetitionPanel;

        private CheatDateTimePopup dateTimePopup;
        private CheatTimeCyclesPopup timeCyclesPopup;
        private CheatSocialPopup cheatSocialPopup;
        private Instantiator instantiator;
        private SaveStorage saveStorage;
        private SoftResetEvent softResetEvent;
        private Wallet wallet;
        private SceneLoadController sceneLoadController;
        private CancellationToken cancellationToken;
        private LevelProgressChangeRequestEvent levelProgressEvent;
        private LifeController lifeController;
        private BoostersManager boostersManager;
        private CheatService cheatService;
        private PopupService popupService;
        private TutorialService tutorialService;
        private SuperSpeedController superSpeedController;
        private PlayerProfileController playerProfileController;
        private LevelService levelService;

        [Inject]
        public void Construct(
            Instantiator instantiator,
            SaveStorage saveStorage,
            SoftResetEvent softResetEvent,
            Wallet wallet,
            SceneLoadController sceneLoadController,
            LevelProgressChangeRequestEvent levelProgressEvent,
            LifeController lifeController,
            BoostersManager boostersManager,
            CheatService cheatService,
            PopupService popupService,
            TutorialService tutorialService,
            SuperSpeedController superSpeedController,
            PlayerProfileController playerProfileController,
            LevelService levelService)
        {
            this.sceneLoadController = sceneLoadController;
            this.levelProgressEvent = levelProgressEvent;
            this.instantiator = instantiator;
            this.saveStorage = saveStorage;
            this.softResetEvent = softResetEvent;
            this.wallet = wallet;
            this.lifeController = lifeController;
            this.boostersManager = boostersManager;
            this.cheatService = cheatService;
            this.popupService = popupService;
            this.tutorialService = tutorialService;
            this.superSpeedController = superSpeedController;
            this.playerProfileController = playerProfileController;
            this.levelService = levelService;
        }


        protected override void OnInitialize()
        {
            cancellationToken = gameObject.GetCancellationTokenOnDestroy();

            dateTimeBtn.onClick.AddListener(OpenDateTimePopup);
            timeCyclesBtn.onClick.AddListener(OpenTimeCyclesPopup);
            timeScaleBtn.onClick.AddListener(SetNextTimeScale);

            addCoinsBtn.onClick.AddListener(AddCoins);
            delCoinsBtn.onClick.AddListener(DelCoins);
            addLifeBtn.onClick.AddListener(AddLife);
            resetLifeBtn.onClick.AddListener(ResetLife);
            loadFromInputFieldBtn.onClick.AddListener(LoadLevel);
            progressPanelBtn.onClick.AddListener(OpenProgressPanel);
            debugPanelBtn.onClick.AddListener(OpenDebugPanel);

            reduceLifeCdBtn.onClick.AddListener(ReduceLifeCd);
            reduceLevelTimeBtn.onClick.AddListener(ReduceLevelTime);
            freezeLevelTimeBtn.onClick.AddListener(FreezeLevelTime);
            switchBombDamageBtn.onClick.AddListener(SwitchBombDamage);
            zoomInBtn.onClick.AddListener(ZoomIn);
            zoomOutBtn.onClick.AddListener(ZoomOut);

            winLevelBtn.onClick.AddListener(WinLevel);

            adsBtn.onClick.AddListener(SwitchAds);
            UpdateAdsBtn();

            lvlUpBtn.onClick.AddListener(() => levelProgressEvent.Request(new LevelProgressArgs(Source.None, ActionType.Add, -1, 1)));
            resetUpBtn.onClick.AddListener(() => levelProgressEvent.Request(new LevelProgressArgs(Source.None, ActionType.Set, -1, 1)));
            returnButton.onClick.AddListener(ReturnToMetaScene);

            infinitePreBoosersOnBtn.onClick.AddListener(InfinitePreBoosterOn);
            infiniteLifeOnBtn.onClick.AddListener(InfiniteLifeOn);
            cheatPurchaseBtn.onClick.AddListener(OpenPurchasePanel);

            plusOneMinuteBtn.onClick.AddListener(() => PlusMinutes(1));
            plusFiveMinutesBtn.onClick.AddListener(() => PlusMinutes(5));
            plusThirtyMinutesBtn.onClick.AddListener(() => PlusMinutes(30));
            plusFiveHoursBtn.onClick.AddListener(() => PlusMinutes(300));
            winStreakBtn.onClick.AddListener(OpenWinStreakPanel);
            rewardTrackBtn.onClick.AddListener(OpenRewardTrackPanel);
            lavaQuestBtn.onClick.AddListener(OpenLavaQuestPanel);

            resetTutorialsBtn.onClick.AddListener(ResetTutorials);
            startTutorialsBtn.onClick.AddListener(StartTutorials);
            activateSuperSpeedBtn.onClick.AddListener(OpenSuperSpeedPanel);
            competitionBtn.onClick.AddListener(OpenCompetitionPanel);
            socialBtn.onClick.AddListener(OpenSocialPanel);

            UpdateTestLevelText();
            loadTestLevelBtn.onClick.AddListener(ScheduleTestLevel);
            tmpCheatBtn.onClick.AddListener(TmpCheat);

            SetObjectActive(true);
        }

        protected override void OnDeinitialize()
        {
            DeinitializeProgressPanel();
            DeinitializePurchasePanel();
            DeinitializeDebugPanel();

            tmpCheatBtn.onClick.RemoveListener(TmpCheat);

            loadTestLevelBtn.onClick.RemoveListener(ScheduleTestLevel);
            activateSuperSpeedBtn.onClick.RemoveListener(OpenSuperSpeedPanel);

            resetTutorialsBtn.onClick.RemoveListener(ResetTutorials);
            startTutorialsBtn.onClick.RemoveListener(StartTutorials);

            dateTimeBtn.onClick.RemoveAllListeners();
            timeCyclesBtn.onClick.RemoveAllListeners();
            timeScaleBtn.onClick.RemoveAllListeners();

            winLevelBtn.onClick.RemoveListener(WinLevel);
            reduceLifeCdBtn.onClick.RemoveListener(ReduceLifeCd);
            reduceLevelTimeBtn.onClick.RemoveListener(ReduceLevelTime);
            freezeLevelTimeBtn.onClick.RemoveListener(FreezeLevelTime);
            switchBombDamageBtn.onClick.RemoveListener(SwitchBombDamage);
            zoomInBtn.onClick.RemoveListener(ZoomIn);
            zoomOutBtn.onClick.RemoveListener(ZoomOut);

            addLifeBtn.onClick.RemoveListener(AddLife);
            resetLifeBtn.onClick.RemoveListener(ResetLife);
            loadFromInputFieldBtn.onClick.RemoveListener(LoadLevel);
            adsBtn.onClick.RemoveListener(SwitchAds);

            lvlUpBtn.onClick.RemoveAllListeners();
            resetUpBtn.onClick.RemoveAllListeners();
            returnButton.onClick.RemoveAllListeners();
            infiniteLifeOnBtn.onClick.RemoveAllListeners();
            infinitePreBoosersOnBtn.onClick.RemoveAllListeners();
            plusOneMinuteBtn.onClick.RemoveAllListeners();
            plusFiveMinutesBtn.onClick.RemoveAllListeners();
            plusThirtyMinutesBtn.onClick.RemoveAllListeners();
            plusFiveHoursBtn.onClick.RemoveAllListeners();

            winStreakBtn.onClick.RemoveAllListeners();
            cheatWinStreakPanel.Deinitialize();
            cheatSuperSpeedPanel.Deinitialize();

            rewardTrackBtn.onClick.RemoveAllListeners();
            cheatRewardTrackPanel.Deinitialize();

            lavaQuestBtn.onClick.RemoveAllListeners();
            cheatLavaQuestPanel.Deinitialize();
            
            competitionBtn.onClick.RemoveAllListeners();
            socialBtn.onClick.RemoveAllListeners();
            cheatCompetitionPanel.Deinitialize();
            
            if(cheatSocialPopup != null)
                cheatSocialPopup.Deinitialize();
        }


        public void SetCurrentLevelId(string value)
        {
            currentLevelIdTxt.text = value;
        }


        private void LoadLevel()
        {
            if (Int32.TryParse(inputField.text, out int result))
            {
                saveStorage.Progress.gameState.levelIdx = result - 1;
                saveStorage.Save();
                softResetEvent.Execute(Unit.Default);
            }
        }


        private void ReturnToMetaScene()
        {
            sceneLoadController.LoadMetaScene();
        }


        private void SetObjectActive(bool value)
        {
            if (gameObject.activeSelf == value)
                return;

            gameObject.SetActive(value);
        }


        private void AddCoins()
        {
            wallet.Increase(CurrencyType.Coins, 100, WalletSystem.Reason.In.Cheat);
        }


        private void DelCoins()
        {
            wallet.Set(CurrencyType.Coins, 0, WalletSystem.Reason.In.Cheat);
        }


        private void AddLife()
        {
            wallet.Increase(CurrencyType.Life, 1, WalletSystem.Reason.In.Cheat);
        }


        private void ResetLife()
        {
            wallet.Set(CurrencyType.Life, 0, WalletSystem.Reason.Out.Cheat);
        }


        private void OpenProgressPanel()
        {
            if (!progressPanel.IsInit)
            {
                instantiator.InjectObject(progressPanel);
                progressPanel.Initialize();
            }

            progressPanel.SetObjectActive(true);
        }


        private void OpenPurchasePanel()
        {
            if (!cheatPurchasePanel.IsInit)
            {
                instantiator.InjectObject(cheatPurchasePanel);
                cheatPurchasePanel.Initialize();
            }

            cheatPurchasePanel.SetObjectActive(true);
        }


        private void OpenDebugPanel()
        {
            if (!debugPanel.IsInit)
            {
                instantiator.InjectObject(debugPanel);
                debugPanel.Initialize();
            }

            debugPanel.SetObjectActive(true);
        }


        private void OpenDateTimePopup()
        {
            OpenDateTimePopupAsync().Forget();
        }


        private async UniTaskVoid OpenDateTimePopupAsync()
        {
            if (dateTimePopup == null)
            {
                dateTimePopup = await popupService.GetAsync<CheatDateTimePopup>(cancellationToken, true, false);
                dateTimePopup.Initialize();
            }

            Close();
            dateTimePopup.Open();
        }


        private void OpenTimeCyclesPopup()
        {
            OpenTimeCyclesPopupAsync().Forget();
        }


        private async UniTaskVoid OpenTimeCyclesPopupAsync()
        {
            if (timeCyclesPopup == null)
            {
                timeCyclesPopup = await popupService.GetAsync<CheatTimeCyclesPopup>(cancellationToken, true, false);
                timeCyclesPopup.Initialize();
            }

            Close();
            timeCyclesPopup.Open();
        }


        private void DeinitializeProgressPanel()
        {
            progressPanelBtn.onClick.RemoveListener(OpenProgressPanel);

            if (progressPanel != null)
                progressPanel.Deinitialize();
        }


        private void DeinitializePurchasePanel()
        {
            cheatPurchaseBtn.onClick.RemoveListener(OpenPurchasePanel);

            if (cheatPurchasePanel != null)
                cheatPurchasePanel.Deinitialize();
        }


        private void DeinitializeDebugPanel()
        {
            debugPanelBtn.onClick.RemoveListener(OpenDebugPanel);

            if (debugPanel != null)
                debugPanel.Deinitialize();
        }


        private void SwitchAds()
        {
            Advertisement.CheatSwitchAdsIsOff();
            UpdateAdsBtn();
        }


        private void UpdateAdsBtn()
        {
            adsBtnText.text = Advertisement.CheatAdsDisabled ? "Ads is OFF" : "Ads is ON";
        }


        private void ReduceLifeCd()
        {
            lifeController.CheatReduceLifeCd();
        }


        private void ReduceLevelTime()
        {
            cheatService.ReduceLevelTime();
        }


        private void FreezeLevelTime()
        {
            cheatService.FreezeLevelTime();
        }


        private void ZoomIn()
        {
            cheatService.ZoomIn();
        }


        private void ZoomOut()
        {
            cheatService.ZoomOut();
        }


        private void SwitchBombDamage()
        {
            cheatService.SwitchBombDamage();
        }


        private void WinLevel()
        {
            Close();
            cheatService.WinLevel();
        }


        private void InfinitePreBoosterOn()
        {
            boostersManager.AddInfiniteBooster(BoosterType.BoostBottle, 10);
            boostersManager.AddInfiniteBooster(BoosterType.BonusClock, 10);
        }


        private void InfinitePreBoosterOff()
        {

        }


        private void InfiniteLifeOn()
        {
            lifeController.AddInfiniteLifeTime(10);
        }


        private void InfiniteLifeOff()
        {

        }


        private void PlusMinutes(int minutes)
        {
            boostersManager.AddInfiniteBooster(BoosterType.BoostBottle, -minutes);
            boostersManager.AddInfiniteBooster(BoosterType.BonusClock, -minutes);
            lifeController.AddInfiniteLifeTime(-minutes);
            lifeController.CheatAddMinutes(-minutes);
        }


        private void OpenWinStreakPanel()
        {
            if (!cheatWinStreakPanel.IsInit)
            {
                instantiator.InjectGameObject(cheatWinStreakPanel.gameObject);
                cheatWinStreakPanel.Initialize();
            }

            cheatWinStreakPanel.SetObjectActive(true);
        }

        private void OpenSuperSpeedPanel()
        {
            if (!cheatSuperSpeedPanel.IsInit)
            {
                instantiator.InjectGameObject(cheatSuperSpeedPanel.gameObject);
                cheatSuperSpeedPanel.Initialize();
            }

            cheatSuperSpeedPanel.SetObjectActive(true);
        }


        private void OpenRewardTrackPanel()
        {
            if (!cheatRewardTrackPanel.IsInit)
            {
                instantiator.InjectGameObject(cheatRewardTrackPanel.gameObject);
                cheatRewardTrackPanel.Construct(this);
                cheatRewardTrackPanel.Initialize();
            }

            cheatRewardTrackPanel.SetObjectActive(true);
        }


        private void OpenLavaQuestPanel()
        {
            if (!cheatLavaQuestPanel.IsInit)
            {
                instantiator.InjectGameObject(cheatLavaQuestPanel.gameObject);
                cheatLavaQuestPanel.Initialize();
            }

            cheatLavaQuestPanel.Open();
        }

        
        private void OpenCompetitionPanel()
        {
            if (!cheatCompetitionPanel.IsInit)
            {
                instantiator.InjectGameObject(cheatCompetitionPanel.gameObject);
                cheatCompetitionPanel.Construct(this);
                cheatCompetitionPanel.Initialize();
            }

            cheatCompetitionPanel.SetObjectActive(true);
        }


        private void OpenSocialPanel()
        {
            OpenSocialPanelAsync().Forget();
        }


        private async UniTaskVoid OpenSocialPanelAsync()
        {
            if (cheatSocialPopup == null)
            {
                cheatSocialPopup = await popupService.GetAsync<CheatSocialPopup>(cancellationToken, true, false);
                cheatSocialPopup.Initialize();
            }

            Close();
            cheatSocialPopup.Open();
        }


        private void SetNextTimeScale()
        {
            switch (DateTimeService.DefaultTimeScale)
            {
                case 1.0f:
                    DateTimeService.SetDefaultTimeScale(1.5f);
                    break;
                case 1.5f:
                    DateTimeService.SetDefaultTimeScale(2f);
                    break;
                case 2f:
                    DateTimeService.SetDefaultTimeScale(3f);
                    break;
                default:
                    DateTimeService.SetDefaultTimeScale(1.0f);
                    break;
            }

            timeScaleTxt.text = $"TimeScale x{DateTimeService.DefaultTimeScale}";
        }

        private void StartTutorials()
        {
            Close();
            UnityEngine.Debug.LogWarning("StartTutorials cheat not work!");
            //tutorialService.EnqueueAllTutorials();
        }

        private void ResetTutorials()
        {
            tutorialService.ResetTutorials();
        }


        private void ScheduleTestLevel()
        {
            ScheduleTestLevelAsync(cancellationToken).Forget();
        }


        private async UniTaskVoid ScheduleTestLevelAsync(CancellationToken cancellationToken) 
        {
            if (sceneLoadController.IsGameSceneActive)
                return;

            var old = levelService.ForcedLevelId;
            levelService.ForcedLevelId = old == "Test_Level" ? string.Empty : "Test_Level";
            await levelService.SetCurrentLevelAsync(levelService.CurrentLevelNumber - 1, cancellationToken);
            if (levelService.ForcedLevelId == "Test_Level")
                sceneLoadController.LoadGameScene();
            UpdateTestLevelText();
        }

        private void UpdateTestLevelText() 
        {
            testLevelTxt.text = levelService.ForcedLevelId == "Test_Level" ? "Unschedule test level" : "Schedule test level";
        }

        private void TmpCheat()
        {
            //For tmp cheat
            playerProfileController.OpenProfilePopup();
        }
    }
}

#endif