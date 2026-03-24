using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.AssetManagement;
using Infrastructure.Localization;
using Infrastructure.Popups;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.TooltipControl;
using UnityEngine;

namespace Features.RewardTrack
{
    public class RewardTrackStatePopupController
    {
        private readonly PopupService popupService;
        private readonly RewardTrackStateController stateController;
        private readonly AssetProvider assetProvider;
        private readonly SpriteAtlasService spriteAtlasService;
        private readonly CancellationTokenSource cts;

        private RewardTrackStatePopup rewardTrackStatePopup;
        private int trackIdOnLastOpen;
        private int currentStepNumber;

        private readonly List<RewardTrackRewardData> configs = new();
        private readonly List<RewardTrackStateUiElement> views = new();


        public RewardTrackStatePopupController(PopupService popupService, RewardTrackStateController stateController, AssetProvider assetProvider,
                                               SpriteAtlasService spriteAtlasService)
        {
            this.popupService = popupService;
            this.stateController = stateController;
            this.assetProvider = assetProvider;
            this.spriteAtlasService = spriteAtlasService;
            cts = new CancellationTokenSource();
            trackIdOnLastOpen = -1;
            currentStepNumber = -1;
        }


        private bool isInit;


        public void Initialize()
        {
            if (isInit || !stateController.IsEnabledByConfig())
                return;

            stateController.OnTrackStateChanged += RewardTrackStateController_OnTrackStateChanged;

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            views.Clear();
            cts.Cancel();
            cts.Dispose();
            stateController.OnTrackStateChanged -= RewardTrackStateController_OnTrackStateChanged;

            if (rewardTrackStatePopup != null)
            {
                rewardTrackStatePopup.Deinitialize();
                rewardTrackStatePopup.OnInfoRequested -= RewardTrackStatePopup_OnInfoRequested;
            }

            isInit = false;
        }


        public void Open()
        {
            OpenAsync().Forget();
        }


        private async UniTaskVoid OpenAsync()
        {
            List<RewardTrackRewardData> actualConfig = stateController.GetRewardsConfig();
            int currentTrackId = stateController.GetCurrentTrackConfig().id;
            RewardTargetData currentTargetConfig = stateController.GetCurrentTargetConfig();

            bool needItemsFullRefresh = configs.Count == 0 || trackIdOnLastOpen != currentTrackId;
            stateController.TryGetCurrentProgress(out ProgressDto currentProgress);

            bool isProgressChanged = currentStepNumber != currentProgress.stepNumber;
            trackIdOnLastOpen = currentTrackId;
            currentStepNumber = currentProgress.stepNumber;

            if (rewardTrackStatePopup == null)
            {
                rewardTrackStatePopup = await popupService.GetAsync<RewardTrackStatePopup>(cts.Token, false, false);
                rewardTrackStatePopup.Initialize();
                rewardTrackStatePopup.OnInfoRequested += RewardTrackStatePopup_OnInfoRequested;
            }

            if (needItemsFullRefresh)
            {
                configs.Clear();
                configs.AddRange(actualConfig);
                await SetThemeImage(currentTargetConfig);

                CreateViewItems();
                ConstructViewItems(currentProgress);
            }
            else if (isProgressChanged)
            {
                UpdateStateOnProgressChange(currentProgress);
            }

            var timeLeft = (float)stateController.GetTimeUntilReset().TotalSeconds;

            rewardTrackStatePopup.Refresh(currentProgress.targetIcon,
                                          currentProgress.rewardVisual.icon,
                                          currentProgress.rewardVisual.amountText,
                                          currentProgress.rewardVisual.isDisplayRibbon,
                                          currentProgress.rewardVisual.isDisplayInfinityIcon,
                                          currentProgress.collectedCountOnEnd,
                                          currentProgress.targetCountByConfig,
                                          timeLeft);

            rewardTrackStatePopup.Open();
            await UniTask.NextFrame(cts.Token);
            ScrollToCurrentSlot(currentProgress.configIdx);
        }


        private async UniTask SetThemeImage(RewardTargetData currentTargetConfig)
        {
            Sprite themeImage = null;
            try
            {
                themeImage = await assetProvider.AddressableLoadAssetAsync<Sprite>(currentTargetConfig.themeImageName, cts.Token);
            }
            catch (System.Exception)
            {
                Debug.LogError($"[RewardTrackStatePopup] {nameof(RewardTrackConfig)} target: {currentTargetConfig.item}. " +
                               $"Image '{currentTargetConfig.themeImageName}' is not found in addressable");
            }
            finally
            {
                rewardTrackStatePopup.SetThemeImage(themeImage);
                assetProvider.ReleaseAsset(currentTargetConfig.themeImageName);
            }
        }


        private void ScrollToCurrentSlot(int idx)
        {
            float targetPosY = GetScrollPosition(idx);
            rewardTrackStatePopup.StopScrollMovement();
            rewardTrackStatePopup.SetContentRootPosition(targetPosY);
        }


        private float GetScrollPosition(int viewIdx)
        {
            float scrollMinPos = rewardTrackStatePopup.ViewportHeight - rewardTrackStatePopup.ContentHeight;
            float slotViewHeight = views[0].Root.rect.height;

            float result = -(slotViewHeight * viewIdx + rewardTrackStatePopup.Spacing * (viewIdx - 1) - rewardTrackStatePopup.CustomOffset);

            if (result > 0)
                return 0;

            if (result < scrollMinPos)
                return scrollMinPos;

            return result;
        }


        private void CreateViewItems()
        {
            int createDiff = configs.Count - views.Count;
            for (int i = 0; i < createDiff; i++)
            {
                RewardTrackStateUiElement element = rewardTrackStatePopup.CreateSlotView();
                views.Add(element);
            }
        }


        private void ConstructViewItems(ProgressDto currentProgress)
        {
            for (int i = 0; i < views.Count; i++)
            {
                RewardTrackStateUiElement view = views[i];

                if (i >= configs.Count)
                {
                    view.SetObjectActive(false);
                    continue;
                }

                var progress = stateController.ConvertToProgressDto(configs[i], i, -1, -1, i == configs.Count - 1);
                Sprite bgSprite = spriteAtlasService.GetFromMain(progress.isLast ? view.LastSlotBgName : view.DefaultSlotBgName);
                
                view.Construct(progress.rewardVisual.icon, 
                               bgSprite, 
                               progress.rewardVisual.amountText, 
                               progress.stepNumber, 
                               progress.rewardVisual.isDisplayRibbon, 
                               progress.rewardVisual.isDisplayInfinityIcon,
                               progress.isLast, 
                               i);
                
                view.OnClick += RewardTrackStateUiElement_OnClick;
                RefreshItemState(view, currentProgress);
                view.SetObjectActive(true);
            }
        }


        private void RefreshItemState(RewardTrackStateUiElement view, ProgressDto currentProgress)
        {
            bool isCurrent = currentProgress.stepNumber == view.StepNumber;
            bool isComplete = currentProgress.stepNumber > view.StepNumber;
            view.RefreshState(isCurrent, isComplete);
        }


        private void UpdateStateOnProgressChange(ProgressDto currentProgress)
        {
            for (int i = 0; i < views.Count; i++)
            {
                RefreshItemState(views[i], currentProgress);
            }
        }


        private void RewardTrackStateController_OnTrackStateChanged()
        {
            if (rewardTrackStatePopup != null)
                rewardTrackStatePopup.Close();
        }


        private void RewardTrackStateUiElement_OnClick(int idx)
        {
            Tooltip tooltip = rewardTrackStatePopup.Tooltip;
            string text = LocalizationService.I.Get(LocKeys.RewardTrack.Tooltip);
            tooltip.Show(text, views[idx].TooltipTarget.position);
        }


        private void RewardTrackStatePopup_OnInfoRequested()
        {
            popupService.OpenAsync<RewardTrackInfoPopup>(cts.Token).Forget();
        }
    }
}