using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Events;
using Features.LevelConfiguration;
using Features.LevelTime;
using Infrastructure.WalletSystem;
using Infrastructure.Ads;
using Infrastructure.Configs;
using Infrastructure.CurrencyHud;
using Infrastructure.Popups;
using Infrastructure.SceneManagement;
using Infrastructure.Utilities;
using R3;
using Features.Level;
using Features.CollectableCurrency;
using Features.LavaQuest;
using Features.RewardTrack;
using Features.WinStreak;
using Features.JellyHoleUi;
using Infrastructure.AudioControl;
using Infrastructure.ApplicationInterrupt;
using Features.SuperSpeedMode;

namespace Features.LevelComplete
{
    public class LevelCompleteManager
    {
        private readonly SceneLoadController sceneLoadController;
        private readonly PopupService popupService;
        private readonly CancellationTokenSource cts;
        private readonly LevelCompleteConfig levelCompleteConfig;
        private readonly LevelFinishEvent levelEndEvent;
        private readonly AllTaskCompleteEvent allTaskCompleteEvent;
        private readonly InterstitialService interstitialService;
        private readonly Wallet wallet;
        private readonly LevelTimeManager timeManager;
        private readonly CompositeDisposable disposables;
        private readonly CurrencyHudFxService currencyHudFxService;
        private readonly LevelService levelService;
        private readonly SuperSpeedController superSpeedController;
        private readonly CollectableCurrencyService collectableCurrencyService;
        private readonly AppInterruptObserver appInterruptObserver;
        private readonly WinStreakStateController winStreakStateController;
        private readonly RewardTrackCollectController rewardTrackCollectController;
        private readonly LavaQuestStateController lavaQuestStateController;
        private readonly LavaQuestEventPopupController lavaQuestEventPopupController;
        private JellyHolePopup jellyHolePopup;
        private LevelCompletePopup levelCompletePopup;
        private LevelData wonLevelData;

        private bool isExitSceneRequested;
        private bool isInit;



        public LevelCompleteManager(
            SceneLoadController sceneLoadController,
            ConfigProvider configProvider,
            PopupService popupService,
            LevelFinishEvent levelEndEvent,
            AllTaskCompleteEvent allTaskCompleteEvent,
            InterstitialService interstitialService,
            Wallet wallet,
            LevelTimeManager timeManager,
            LevelService levelService,
            CollectableCurrencyService collectableCurrencyService,
            CurrencyHudFxService currencyHudFxService,
            WinStreakStateController winStreakStateController,
            RewardTrackCollectController rewardTrackCollectController, 
            LavaQuestStateController lavaQuestStateController,
            LavaQuestEventPopupController lavaQuestEventPopupController,
            AppInterruptObserver appInterruptObserver, 
            SuperSpeedController superSpeedController)
        {
            this.levelEndEvent = levelEndEvent;
            this.allTaskCompleteEvent = allTaskCompleteEvent;
            this.interstitialService = interstitialService;
            this.wallet = wallet;
            this.timeManager = timeManager;
            this.sceneLoadController = sceneLoadController;
            this.levelCompleteConfig = configProvider.LevelCompleteConfig;
            this.popupService = popupService;
            this.currencyHudFxService = currencyHudFxService;
            this.winStreakStateController = winStreakStateController;
            this.rewardTrackCollectController = rewardTrackCollectController;
            this.lavaQuestStateController = lavaQuestStateController;
            this.lavaQuestEventPopupController = lavaQuestEventPopupController;
            this.levelService = levelService;
            this.superSpeedController = superSpeedController;
            this.collectableCurrencyService = collectableCurrencyService;
            this.appInterruptObserver = appInterruptObserver;

            this.cts = new CancellationTokenSource();
            this.disposables = new CompositeDisposable();
        }

        public void Initialize()
        {
            if (isInit)
                return;

            allTaskCompleteEvent.Subscribe(_ => { Win(); }).AddTo(disposables);
            appInterruptObserver.Interrupt += AppInterruptObserver_Interrupt;
            isExitSceneRequested = false;
            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            appInterruptObserver.Interrupt -= AppInterruptObserver_Interrupt;
            cts.Cancel();
            cts.Dispose();
            disposables.Dispose();

            lavaQuestEventPopupController.OnClosePopup -= LavaQuestPopupController_OnClosePopup;

            isInit = false;
        }


        private void Win()
        {
            timeManager.StopTimer();
            levelService.ScheduleIncrement();
            lavaQuestStateController.ScheduleActionWin();
            wonLevelData = levelService.GetCurrentLevelData();
            
            winStreakStateController.HandleWin();
            
            int levelDifficultyMultiplier = GetLevelDifficiltyMultiplier(wonLevelData.difficulty);
            
            int startValue = wallet.GetCount(CurrencyType.Coins);
            int amount = LevelCompleteReward(levelDifficultyMultiplier);
            wallet.Increase(CurrencyType.Coins, amount, Reason.In.LevelComplete, false);
            currencyHudFxService.Schedule(CurrencyType.Coins, amount, startValue);

            int specialItemsCount = GetMultipliedRewardTrackItemsCount();
            levelEndEvent.ExecuteWithWin(rewardTrackCollectController.CollectedCount, specialItemsCount, winStreakStateController.RewardMultiplier, levelDifficultyMultiplier);
            superSpeedController.HandleLevelFinish(true);

            OpenCompletePopupsAsync().Forget();
        }


        private async UniTaskVoid OpenCompletePopupsAsync()
        {
            AudioService.I.StopMusic();

            if (levelCompleteConfig.JellyHolePopupEnabled)
            {
                jellyHolePopup = await popupService.GetAsync<JellyHolePopup>(cts.Token);
                jellyHolePopup.Initialize();
                jellyHolePopup.Open();
                await UniTask.WaitUntil(() => jellyHolePopup.IsOpened, cancellationToken: cts.Token);
                await UniTask.WaitUntil(() => !jellyHolePopup.IsOpened, cancellationToken: cts.Token);
            }            

            levelCompletePopup = await popupService.GetAsync<LevelCompletePopup>(cts.Token, true);
            string rewardBtnText = $"x{levelCompleteConfig.AdsMultiplier}";
            string timeLeft = TimeUtils.GetTimeString(timeManager.SecondsLeft);
            string coins = LevelCompleteReward(wonLevelData.difficulty).ToString();
            string coinsRewarded = (LevelCompleteReward(wonLevelData.difficulty) + WatchAdReward(wonLevelData.difficulty)).ToString();
            levelCompletePopup.Construct(coins, coinsRewarded, timeLeft, AdsKeys.Rewarded.LvlCompleteMulti, rewardBtnText, MultiplyByRewardedAds, OnClickNext);
            levelCompletePopup.Initialize();
            levelCompletePopup.Open();
            ShowWinStreakBanner();
        }


        public int LevelCompleteReward(LevelDifficulty levelDifficulty)
        {
            return (levelCompleteConfig.RewardCoinCount + collectableCurrencyService.GetCollectedCount(CurrencyType.Coins)) * GetLevelDifficiltyMultiplier(levelDifficulty);
        }

        public int LevelCompleteReward(int levelCompleteMultiplier)
        {
            return (levelCompleteConfig.RewardCoinCount + collectableCurrencyService.GetCollectedCount(CurrencyType.Coins)) * levelCompleteMultiplier;
        }
        
        private int GetLevelDifficiltyMultiplier(LevelDifficulty levelDifficulty)
        {
            return levelCompleteConfig.GetMultiplierByDifficulty(levelDifficulty);
        }

        private int WatchAdReward(LevelDifficulty levelDifficulty)
        {
            return LevelCompleteReward(levelDifficulty) * levelCompleteConfig.AdsMultiplier;
        }


        private int GetMultipliedRewardTrackItemsCount()
        {
            int calcMultiplier = levelCompleteConfig.GetMultiplierByDifficulty(wonLevelData.difficulty) * winStreakStateController.RewardMultiplier;
            int multiplier = calcMultiplier < 1 ? 1 : calcMultiplier;
            return rewardTrackCollectController.GetMultipliedCollectedItems(multiplier);
        }


        private async UniTaskVoid ReturnToMainMenuAsync()
        {
            if(isExitSceneRequested)
                return;
            
            isExitSceneRequested =  true;

            await levelService.ApplyScheduledAsync(cts.Token);
            sceneLoadController.LoadNextScene();
        }


        private void ShowWinStreakBanner()
        {
            WinStreakUIBanner banner = levelCompletePopup.WinStreakUIBanner;

            if (!winStreakStateController.IsFeatureEnabled)
            {
                banner.SetObjectActive(false);
                return;
            }

            banner.Construct(winStreakStateController.CurrentLevel,
                             winStreakStateController.MaxLevel,
                             winStreakStateController.UnlockLevel,
                             levelService.CurrentLevelNumber >= winStreakStateController.UnlockLevel,
                             winStreakStateController.RewardMultiplier,
                             winStreakStateController.IsMultiplierEnabled);

            banner.Initialize();
            banner.PlayOpenAsync(winStreakStateController.IsIncremented, cts.Token).Forget();
        }


        private void ShowLavaQuestPopupOrReturnMetaScene()
        {
            if (lavaQuestEventPopupController.TryOpenPopupOnScheduledAction())
            {
                lavaQuestEventPopupController.OnClosePopup += LavaQuestPopupController_OnClosePopup;
            }
            else
            {
                ReturnToMainMenuAsync().Forget();
            }
        }


        private void MultiplyByRewardedAds()
        {
            int startValue = wallet.GetCount(CurrencyType.Coins);
            int coinsRewarded = WatchAdReward(wonLevelData.difficulty);
            wallet.Increase(CurrencyType.Coins, coinsRewarded, Reason.In.LevelCompleteMulti, false);
            currencyHudFxService.Schedule(CurrencyType.Coins, coinsRewarded, startValue);

            ShowLavaQuestPopupOrReturnMetaScene();
        }


        private void OnClickNext()
        {
            interstitialService.ShowOnWinLevel(ShowLavaQuestPopupOrReturnMetaScene);
        }


        private void LavaQuestPopupController_OnClosePopup()
        {
            ReturnToMainMenuAsync().Forget();
        }

        private void AppInterruptObserver_Interrupt()
        {
            if (levelCompletePopup == null)
                return;

            if (!levelCompletePopup.IsOpened)
                return;

            levelCompletePopup.InstantAnimationComplete();
        }
    }
}