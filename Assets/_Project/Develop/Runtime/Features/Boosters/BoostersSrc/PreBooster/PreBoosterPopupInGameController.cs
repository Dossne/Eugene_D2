using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Boosters.Model;
using Features.Level;
using Features.LevelComplete;
using Features.Life;
using Features.LifeUi;
using Features.WinStreak;
using Infrastructure.Pool.FloatingText;
using Infrastructure.Configs;
using Infrastructure.Localization;
using Infrastructure.Pool;
using Infrastructure.Popups;
using Infrastructure.SceneManagement;
using R3;
using UnityEngine;
using Features.SuperSpeedMode;

namespace Features.Boosters
{
    public class PreBoosterInGamePopupController
    {
        private readonly SceneLoadController sceneLoadController;
        private readonly BoostersManager boostersManager;
        private readonly LevelCompleteConfig levelCompleteConfig;
        private readonly BoosterConfig boosterConfig;
        private readonly LevelService levelService;
        private readonly WinStreakStateController winStreakStateController;
        private readonly FloatingTextPool floatingTextPool;

        private readonly PopupService popupService;
        private readonly LifeController lifeController;
        private readonly LifeUiController lifeUiController;
        private readonly SuperSpeedController superSpeedController;
        private readonly CancellationTokenSource cts;
        private readonly HashSet<BoosterType> selectedPreBoosters = new();
        private readonly CompositeDisposable disposable = new();

        private PreBoostersInGamePopup preBoostersPopup;
        private PoolableFloatingText flyText;
        private int winStreakLevelOnBegin;

        private BoosterType freePreBoosterLocked = BoosterType.None;


        public PreBoosterInGamePopupController(BoostersManager boostersManager,
                                               ConfigProvider configProvider,
                                               SceneLoadController sceneLoadController,
                                               PopupService popupService,
                                               LifeController lifeController,
                                               LifeUiController lifeUiController,
                                               LevelService levelService,
                                               WinStreakStateController winStreakStateController,
                                               PoolService poolService,
                                               SuperSpeedController superSpeedController)
        {
            this.boostersManager = boostersManager;
            this.sceneLoadController = sceneLoadController;
            this.popupService = popupService;
            this.lifeController = lifeController;
            this.lifeUiController = lifeUiController;
            this.levelService = levelService;
            this.winStreakStateController = winStreakStateController;
            this.superSpeedController = superSpeedController;
            this.floatingTextPool = poolService.Get<FloatingTextPool>();
            this.levelCompleteConfig = configProvider.LevelCompleteConfig;
            this.boosterConfig = configProvider.BoosterConfig;
            this.cts = new CancellationTokenSource();
        }


        private bool isInit;        

        public void Initialize()
        {
            if (isInit)
                return;

            selectedPreBoosters.RemoveWhere(x => x != freePreBoosterLocked);

            boostersManager.OnBoosterUpdated.Subscribe(boosterType =>
            {
                if (!boostersManager.TryGetBoosterModel(boosterType, out BoosterModel booster))
                {
                    return;
                }

                RefreshSlotView(booster);
            }).AddTo(disposable);

            lifeUiController.OnQuitHandled += LifeUiController_OnQuitHandled;            

            winStreakLevelOnBegin = winStreakStateController.CurrentLevel;
            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;
            lifeUiController.OnQuitHandled -= LifeUiController_OnQuitHandled;

            if (preBoostersPopup != null)
            {
                preBoostersPopup.Deinitialize();
                preBoostersPopup.OnStartClick -= PreBoostersPopup_OnStartClick;
                preBoostersPopup.OnSlotClick -= PreBoostersPopup_OnSlotClick;
                preBoostersPopup.OnCloseClick -= PreBoostersPopup_OnCloseClick;
                preBoostersPopup.OnSuperSpeedWidgetClick -= PreBoostersPopup_OnSuperSpeedWidgetClick;
                DeinitializeWinStreakUIPanel();
            }

            disposable.Dispose();

            isInit = false;
        }


        public void ShowPopup()
        {
            OpenPreBoosterPopupAsync(levelService.GetCurrentLevelData().difficulty, levelService.CurrentLevelNumber, true).Forget();
        }


        private async UniTask OpenPreBoosterPopupAsync(LevelDifficulty difficulty, int level, bool isOnLose)
        {
            if (preBoostersPopup == null)
            {
                preBoostersPopup = await popupService.GetAsync<PreBoostersInGamePopup>(cts.Token, true);
                preBoostersPopup.Construct(boosterConfig.PreBoosters);
                preBoostersPopup.Initialize();
                preBoostersPopup.OnStartClick += PreBoostersPopup_OnStartClick;
                preBoostersPopup.OnSlotClick += PreBoostersPopup_OnSlotClick;
                preBoostersPopup.OnCloseClick += PreBoostersPopup_OnCloseClick;
                preBoostersPopup.OnSuperSpeedWidgetClick += PreBoostersPopup_OnSuperSpeedWidgetClick;

                InitializeWinStreakUIPanel();
            }

            foreach (var boosterData in boosterConfig.PreBoosters)
            {
                if (!boostersManager.TryGetBoosterModel(boosterData.type, out BoosterModel booster))
                    continue;

                boostersManager.RefreshBoosterState(booster);
                RefreshSlotView(booster);
            }

            preBoostersPopup.RefreshPopup(difficulty, level, levelCompleteConfig.GetMultiplierByDifficulty(difficulty), isOnLose);            
            RefreshWinStreakPanel();
            preBoostersPopup.Open();
            preBoostersPopup.RefreshSuperSpeedWidget(superSpeedController);
        }


        private void RefreshSlotView(BoosterModel boosterModel)
        {
            if (preBoostersPopup == null)
                return;

            float InfiniteBoostersExpires = boostersManager.GetInfinitePreboostersExpiresTime(boosterModel.BoosterType);

            preBoostersPopup.RefreshSlotState(boosterModel.BoosterType,
                                              boosterModel.CurrentState, 
                                              boosterModel.CurrentCount.ToString(),
                                              boosterModel.LockLevelText,
                                              boosterModel.FreeText,
                                              boosterModel.IsSelectable,
                                              InfiniteBoostersExpires);
        }



        private void LoadNextScene()
        {
            sceneLoadController.LoadNextScene();
        }


        private void LoadGameScene()
        {
            sceneLoadController.LoadGameScene();
        }
        
        private void InitializeWinStreakUIPanel()
        {
            WinStreakUIPanel panel = preBoostersPopup.WinStreakUIPanel;
            
            if (!winStreakStateController.IsFeatureEnabled)
            {
                panel.SetObjectActive(false);
                return;
            }
            
            panel.Construct(winStreakStateController.MaxLevel, winStreakStateController.UnlockLevel, winStreakStateController.IsUnlocked, winStreakStateController.IsMultiplierEnabled);
            panel.Initialize();
        }
        
        private void DeinitializeWinStreakUIPanel()
        {
            preBoostersPopup.WinStreakUIPanel.Deinitialize();
        }
        
        
        private void RefreshWinStreakPanel()
        {
            WinStreakUIPanel panel = preBoostersPopup.WinStreakUIPanel;

            if (!winStreakStateController.IsFeatureEnabled)
                return;
            
            (Sprite icon, int level) current = winStreakStateController.GetWinStreakData(winStreakLevelOnBegin);
            (Sprite icon, int level) zero = winStreakStateController.GetWinStreakData(0);
            panel.RefreshStateAnimated(current.icon, current.level, zero.icon, zero.level);
            
#if UNITY_EDITOR
            panel.SetStateController_Editor(winStreakStateController);
#endif
        }


        private void PreBoostersPopup_OnSlotClick(BoosterType type, bool isSelected)
        {
            if (!boostersManager.TryGetBoosterModel(type, out BoosterModel booster))
            {
                return;
            }

            if (booster.CurrentState is BoosterStateType.Infinite)
            {
                ShowInfinitePreBoosterText(type, cts.Token).Forget();
            }
            else if (booster.CurrentState is BoosterStateType.Free)
            {
                if (freePreBoosterLocked == BoosterType.None)
                {
                    freePreBoosterLocked = type;
                    float infiniteExpiresTime = boostersManager.GetInfinitePreboostersExpiresTime(type);
                    preBoostersPopup.RefreshSlotState(type, BoosterStateType.Selected, null, null, null, true, infiniteExpiresTime);
                    selectedPreBoosters.Add(type);
                }
            }
            else if (booster.CurrentState is BoosterStateType.NeedBuy)
            {
                boostersManager.OpenBoosterBuyPopup(booster);
            }
            else if (isSelected && booster.CurrentState is BoosterStateType.Ready)
            {
                float InfiniteBoostersExpires = boostersManager.GetInfinitePreboostersExpiresTime(type);
                preBoostersPopup.RefreshSlotState(type, BoosterStateType.Selected, null, null, null, true, InfiniteBoostersExpires);
                selectedPreBoosters.Add(type);
            }
            else if (booster.CurrentState is not BoosterStateType.Free)
            {
                selectedPreBoosters.Remove(type);
                RefreshSlotView(booster);                
            }
        }


        private async UniTask ShowInfinitePreBoosterText(BoosterType type, CancellationToken cancellationToken)
        {
            if (flyText != null)
            {
                flyText.ForceStop();
            }

            (bool isSuccess, PoolableFloatingText item) poolResult = await floatingTextPool.TryGetItemAsync(FloatingTextType.InfinitePreBooster, cancellationToken);

            if(!poolResult.isSuccess)
                return;
            
            flyText = poolResult.item;
            
            RectTransform boosterViewRectTransform = preBoostersPopup.GetBoosterViewRectTransform(type);
            string boosterName = LocalizationService.I.Get(type.ToString() + LocKeys.Boosters.BoosterName);
            flyText.RequestReleaseToPool += FlyTextReleaseToPool;
            flyText.Show(LocalizationService.I.Get(LocKeys.Boosters.FloatingTextInfiniteBooster, boosterName), boosterViewRectTransform.position,
                         Vector3.one, false);
        }


        private void FlyTextReleaseToPool(PoolableFloatingText floatingText)
        {
            floatingText.RequestReleaseToPool -= FlyTextReleaseToPool;
            flyText = null;
        }


        private void PreBoostersPopup_OnStartClick()
        {
            if (lifeController.IsInfiniteLife()
                || lifeController.LifeAmount >= 1)
            {
                preBoostersPopup.InstantClose();
                boostersManager.SetSelectedPreBoosters(selectedPreBoosters);
                LoadGameScene();
            }
            else
            {
                lifeUiController.ShowBuyLifePopup();
            }
        }


        private void PreBoostersPopup_OnCloseClick()
        {
            preBoostersPopup.InstantClose();
            LoadNextScene();
        }


        private void LifeUiController_OnQuitHandled(bool withLoose)
        {
            if (withLoose)
            {
                ShowPopup();
            }
            else
            {
                LoadNextScene();
            }
        }

        private void PreBoostersPopup_OnSuperSpeedWidgetClick()
        {
            if (superSpeedController.IsUnlocked)
                OpenInfoPopupAsync(cts.Token).Forget();
            else
                preBoostersPopup.ShowSuperSpeedWidgetTooltip(LocalizationService.I.Get(LocKeys.PreBoosterPopup.SuperSpeedWidgetTooltip, superSpeedController.UnlockLevel.ToString()));
        }

        private async UniTaskVoid OpenInfoPopupAsync(CancellationToken token)
        {
            SuperSpeedInfoPopup infoPopup = await popupService.OpenAsync<SuperSpeedInfoPopup>(token);

            string headerTxt = LocalizationService.I.Get(LocKeys.SuperSpeedInfoPopup.Header);
            string levelsTxt = LocalizationService.I.Get(LocKeys.SuperSpeedInfoPopup.Levels, superSpeedController.MaxWinCount.ToString());
            string activateTxt = LocalizationService.I.Get(LocKeys.SuperSpeedInfoPopup.Activation);
            string boostTxt = LocalizationService.I.Get(LocKeys.SuperSpeedInfoPopup.Boost);
            infoPopup.SetTexts(headerTxt, levelsTxt, activateTxt, boostTxt);
        }
    }
}