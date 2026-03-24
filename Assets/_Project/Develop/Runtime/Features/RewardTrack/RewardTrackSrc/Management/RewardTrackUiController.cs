using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Tutorial;
using Features.WinStreak;
using Infrastructure.CurrencyHud;
using Infrastructure.DateTimeControl;
using Infrastructure.HapticControl;
using Infrastructure.Localization;
using Infrastructure.MainUICanvasControl;
using Infrastructure.Pool;
using Infrastructure.Popups;
using Infrastructure.Reward;
using Infrastructure.SystemsLifeCycle;
using Infrastructure.Utilities;
using UnityEngine;
using R3;

namespace Features.RewardTrack
{
    public class RewardTrackUiController : ISystemTickable
    {
        private readonly RewardTrackStateController stateController;
        private readonly DateTimeService dateTimeService;
        private readonly PopupService popupService;
        private readonly PoolService poolService;
        private readonly RewardTrackHudProgress progressHud;
        private readonly WinStreakStateController winStreakStateController;
        private readonly CurrencyHudService currencyHudService;
        private readonly RewardTrackStatePopupController rewardTrackStatePopupController;
        private readonly RewardTrackRequestEvent rewardTrackRequestEvent;

        private CancellationTokenSource cts;
        private CompositeDisposable disposable;
        private List<string> hudPanelMaskIds;
        private MaskTutorialPanel maskTutorialPanel;

        private double secondLeftTillReset;
        private float hudRefreshSeconds;
        private bool isInit;


        public RewardTrackUiController(RewardTrackStateController stateController,
                                       MainUIProvider mainUIProvider,
                                       DateTimeService dateTimeService,
                                       PopupService popupService,
                                       PoolService poolService,
                                       WinStreakStateController winStreakStateController,
                                       CurrencyHudService currencyHudService,
                                       RewardTrackStatePopupController rewardTrackStatePopupController,
                                       RewardTrackRequestEvent rewardTrackRequestEvent)
        {
            this.stateController = stateController;
            this.dateTimeService = dateTimeService;
            this.popupService = popupService;
            this.poolService = poolService;
            this.winStreakStateController = winStreakStateController;
            this.currencyHudService = currencyHudService;
            this.rewardTrackStatePopupController = rewardTrackStatePopupController;
            this.rewardTrackRequestEvent = rewardTrackRequestEvent;
            this.progressHud = mainUIProvider.HudProvider.RewardTrackHud.HUDProgress;
            this.hudPanelMaskIds = new List<string>();
            maskTutorialPanel = mainUIProvider.MaskTutorialPanel;

        }


        public void Initialize()
        {
            if (isInit || !stateController.IsEnabledByConfig())
                return;

            cts = new CancellationTokenSource();
            disposable = new CompositeDisposable();

            InitHudProgress();

            rewardTrackRequestEvent.Subscribe(RewardTrackRequestHandle).AddTo(disposable);

            dateTimeService.OnChange += DateTimeService_OnChange;
            stateController.OnTrackStateChanged += RewardTrackStateController_OnTrackStateChanged;
            progressHud.OnPopupButtonClick += RewardTrackHudProgress_OnPopupButtonClick;

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            cts.Cancel();
            cts.Dispose();
            disposable.Dispose();

            stateController.ClearLastCollectedProgress();
            
            DeactivateMaskOnPanelHud();
            progressHud.SetButtonEnabled(true);
            progressHud.Deinitialize();
            progressHud.SetObjectActive(false);

            dateTimeService.OnChange -= DateTimeService_OnChange;
            stateController.OnTrackStateChanged -= RewardTrackStateController_OnTrackStateChanged;
            progressHud.OnPopupButtonClick -= RewardTrackHudProgress_OnPopupButtonClick;
            isInit = false;
        }


        void ISystemTickable.Tick()
        {
            if (!isInit) //feature disabled
                return;

            hudRefreshSeconds -= Time.unscaledDeltaTime;

            if (hudRefreshSeconds > 0)
                return;

            RefreshTimerOnHud();
            ResetHudRefreshSeconds();
        }


        public UniTask ExecuteScheduledAsync(CancellationToken token)
        {
            return ExecuteScheduledAsync(winStreakStateController.IsMaxLevel, winStreakStateController.RewardMultiplier, token);
        }


        private async UniTask ExecuteScheduledAsync(bool isWinStreak, int winStreakMultiplier, CancellationToken cancellationToken)
        {
            (int itemCount, List<ProgressDto> progress) lastCollected = stateController.GetLastCollectedProgress();

            if (lastCollected.itemCount == 0)
                return;

            try
            {
                await UniTask.WaitUntil(() => !popupService.IsAnyOpened, cancellationToken: cancellationToken);
                
                if (!IsValid(lastCollected))
                    throw new ArgumentOutOfRangeException(nameof(lastCollected));
                
                stateController.ApplyLastProgressReward();

                ActivateMaskOnPanelHud();
                SetButtonInteractable(false);

                List<ProgressDto> lastCollectProgress = lastCollected.progress;

                FloatingIconRewardTrackPool iconPool = poolService.Get<FloatingIconRewardTrackPool>();
                iconPool.TryGetItem(out var floatingIcon);

                Sprite targetIcon = lastCollectProgress[0].targetIcon;
                int showCount = isWinStreak ? lastCollected.itemCount / winStreakMultiplier : lastCollected.itemCount;

                await floatingIcon.Show(targetIcon,
                                        $"{showCount.ToString()}",
                                        progressHud.StartFlyPosition,
                                        progressHud.TargetIconPosition,
                                        isWinStreak,
                                        $"{lastCollected.itemCount.ToString()}",
                                        cancellationToken
                                       );

                
                DeactivateMaskOnPanelHud();

                await floatingIcon.Fly(cancellationToken);
                await ApplyVisualOnProgressHudAsync(lastCollectProgress, cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (ArgumentOutOfRangeException ex)
            {
#if PR_CHEAT || UNITY_EDITOR
                Debug.LogWarning(ex);
#endif
            }
            finally
            {
                DeactivateMaskOnPanelHud();
                SetButtonInteractable(true);
            }
        }



        private void InitHudProgress()
        {
            if (stateController.IsCurrentTrackCompleted())
                return;

            var fxPool = poolService.Get<ParticlesPool>();
            progressHud.Construct(fxPool);
            progressHud.Initialize();

            (int itemCount, List<ProgressDto> progress) lastCollected = stateController.GetLastCollectedProgress();

            stateController.TryGetCurrentProgress(out var currentProgress);

            var progress = lastCollected.itemCount == 0 ? currentProgress : lastCollected.progress[0];

            progressHud.SetObjectActive(true);
            SetCurrentTarget(progress);

            if (stateController.IsUnlocked())
            {
                progressHud.RefreshProgress(progress.collectedCountOnStart, progress.targetCountByConfig);
            }
            else
            {
                progressHud.SetLocked(LocalizationService.I.Get(LocKeys.RewardTrack.Lock, stateController.GetUnlockLevel().ToString()));
            }

            RefreshTimerOnHud();
        }


        private async UniTask ApplyVisualOnProgressHudAsync(List<ProgressDto> lastCollectProgress, CancellationToken token)
        {
            progressHud.PlayFxOnTargetIcon();
            PlayHaptic();

            for (var i = 0; i < lastCollectProgress.Count; i++)
            {
                ProgressDto progress = lastCollectProgress[i];

                await progressHud.SliderAnimatedMoveAsync(progress.collectedCountOnStart, progress.collectedCountOnEnd, progress.targetCountByConfig, token);

                if (progress.CanGiveReward())
                {
                    progressHud.ConstructFlyingIcon(progress.rewardVisual.icon,
                                                    progress.rewardVisual.amountText,
                                                    progress.rewardVisual.isDisplayRibbon,
                                                    progress.rewardVisual.isDisplayInfinityIcon);

                    ProgressDto next;

                    if (i < lastCollectProgress.Count - 1)
                    {
                        next = lastCollectProgress[i + 1];
                        progressHud.PlayFxOnRewardIcon(() => SetCurrentTarget(next));
                    }
                    else
                    {
                        stateController.TryGetCurrentProgress(out next);
                    }

                    progressHud.PlayFxOnRewardIcon(() => SetCurrentTarget(next));

                    ApplyComplexRewardVisual(progress.reward);
                    PlayHaptic();
                    await UniTask.WaitForSeconds(progressHud.RewardIconDurationTotal, true, cancellationToken: token);
                }
            }

            RefreshProgressBarWithCurrentProgress();

            if (stateController.IsCurrentTrackCompleted())
            {
                progressHud.SetObjectActive(false);
            }
        }


        private void RefreshProgressBarWithCurrentProgress()
        {
            bool progressExists = stateController.TryGetCurrentProgress(out var progress);

            if (!progressExists)
                return;

            progressHud.RefreshProgress(progress.collectedCountOnEnd, progress.targetCountByConfig);
        }


        private void PlayHaptic()
        {
            HapticService.I.HapticSelection();
        }


        private void SetCurrentTarget(ProgressDto progress)
        {
            progressHud.SetTarget(progress.targetIcon, progress.rewardVisual.icon, progress.rewardVisual.amountText, progress.rewardVisual.isDisplayRibbon,
                                  progress.rewardVisual.isDisplayInfinityIcon);
        }


        //Таймер до окончания длительности ревард трека:
        //    1. Выводится в формате: n дней, m часов.
        //    2. Когда до окончания таймера остается <1 дня, выводится в формате n часов, m минут
        //    3. Когда до окончания таймера остается <1 часа, выводится в формате n минут, m секунд
        //    4. Когда до окончания таймера остается <1 минуты, выводится в формате n секунд
        private void ResetHudRefreshSeconds()
        {
            TimeSpan timeLeftTillReset = stateController.GetTimeUntilReset();

            if (timeLeftTillReset.Days > 0)
            {
                hudRefreshSeconds = 60f * 60f;
                return;
            }

            if (timeLeftTillReset.Hours > 0)
            {
                hudRefreshSeconds = 60f;
                return;
            }

            hudRefreshSeconds = 1;
        }


        private void RefreshTimerOnHud()
        {
            TimeSpan timeLeftTillReset = stateController.GetTimeUntilReset();
            string time = TimeUtils.GetTimeString((float)timeLeftTillReset.TotalSeconds, 100f);
            progressHud.RefreshTime(time);
        }


        private void ApplyComplexRewardVisual(ComplexReward complexReward, float rewardMultiplier = 1)
        {
            for (int i = 0; i < complexReward.currencyOfferRewards.Count; i++)
            {
                var reward = complexReward.currencyOfferRewards[i];

                int giveCount = Mathf.RoundToInt(reward.amount * rewardMultiplier);
                currencyHudService.Refresh(reward.currencyType, giveCount, true, ActionType.Add);
            }
        }


        private void ActivateMaskOnPanelHud()
        {
            maskTutorialPanel.Acquire(this);
            hudPanelMaskIds = maskTutorialPanel.AddNewComplexMask(progressHud.MainRectTransform);
        }


        private void DeactivateMaskOnPanelHud()
        {
            if (hudPanelMaskIds.Count == 0)
                return;

            maskTutorialPanel.RemoveComplexMask(hudPanelMaskIds);
            maskTutorialPanel.Release(this);
            hudPanelMaskIds.Clear();
        }

        private void SetButtonInteractable(bool value)
        {
            progressHud.SetButtonEnabled(value);
        }
        
        private bool IsValid((int itemCount, List<ProgressDto> progress) item)
        {
            return item.itemCount > 0 && item.progress != null && item.progress.Count > 0;
        }

        private void DateTimeService_OnChange(TimeChangeReason reason)
        {
            ResetHudRefreshSeconds();
            RefreshTimerOnHud();
        }


        private void RewardTrackStateController_OnTrackStateChanged()
        {
            Deinitialize();
            Initialize();
        }


        private void RewardTrackHudProgress_OnPopupButtonClick()
        {
            if (stateController.IsUnlocked())
                rewardTrackStatePopupController.Open();
        }


        private void RewardTrackRequestHandle(RTRequest req)
        {
#if PR_CHEAT || UNITY_EDITOR
            if (!stateController.IsEnabledByConfig() || !stateController.IsUnlocked())
                return;

            switch (req.action)
            {
                case RTRequestAction.Add:
                    CheatAdd(req.addCount);
                    break;
                case RTRequestAction.AddMax:
                    CheatAddMax();
                    break;
                case RTRequestAction.Reset:
                    CheatReset();
                    break;
            }
#endif
        }


#if PR_CHEAT || UNITY_EDITOR


        private void CheatAdd(int count)
        {
            bool isExists = stateController.TryGetCurrentProgress(out var cpDto);

            if (!isExists)
                return;

            stateController.ApplyCollectedCount(cpDto.targetItemType, count);
            ExecuteScheduledAsync(winStreakStateController.IsMaxLevel, winStreakStateController.RewardMultiplier, cts.Token).Forget();
        }


        private void CheatAddMax()
        {
            int addCount = stateController.GetCountForMax();
            CheatAdd(addCount);
        }


        private void CheatReset()
        {
            stateController.CheatReset();
            Deinitialize();
            Initialize();
        }
#endif
    }
}