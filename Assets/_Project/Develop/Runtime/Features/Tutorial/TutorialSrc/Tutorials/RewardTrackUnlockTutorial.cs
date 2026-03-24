using Cysharp.Threading.Tasks;
using Features.Level;
using Features.RewardTrack;
using Infrastructure.InputControl;
using Infrastructure.Localization;
using Infrastructure.MainUICanvasControl;
using Infrastructure.Popups;
using Infrastructure.SceneManagement;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Features.Tutorial
{
    public class RewardTrackUnlockTutorial : TutorialBase
    {
        private RewardTrackStateController rewardTrackStateController;
        private LevelService levelService;
        private SceneLoadController sceneLoadController;
        private PopupService popupService;
        private InputUiService inputUiService;
        private MaskTutorialPanel maskTutorialPanel;        
        private RewardTrackHudProgress rewardTrackHudProgress;
        private RewardTrackStatePopup rewardTrackStatePopup;
        private List<string> complexMaskIds = new();
        private string pointerId = string.Empty;
        private string tooltipId = string.Empty;
        private CancellationTokenSource cts;
        private RewardTrackUnlockData rewardTrackUnlockData;

        public RewardTrackUnlockTutorial(string tutorialId,
                                       InputUiService inputUiService,
                                       PopupService popupService,
                                       MainUIProvider mainUIProvider,
                                       LevelService levelService,
                                       RewardTrackStateController rewardTrackStateController,
                                       SceneLoadController sceneLoadController,
                                       RewardTrackUnlockData rewardTrackUnlockData) : base(tutorialId)
        {
            this.sceneLoadController = sceneLoadController;
            this.inputUiService = inputUiService;
            this.popupService = popupService;
            this.rewardTrackStateController = rewardTrackStateController;
            this.levelService = levelService;

            maskTutorialPanel      = mainUIProvider.MaskTutorialPanel;
            rewardTrackHudProgress = mainUIProvider.HudProvider.RewardTrackHud.HUDProgress;

            this.rewardTrackUnlockData = rewardTrackUnlockData;
        }

        public override bool CanBeEqueued()
        {
            return CanBeStarted();
        }

        public override bool CanBeStarted()
        {
            return rewardTrackStateController.IsUnlocked()
                && rewardTrackStateController.GetUnlockLevel() == levelService.CurrentLevelNumber
                && sceneLoadController.IsMetaSceneActive;
        }

        public override void Start()
        {
            StartWaitingForPopup();
            rewardTrackHudProgress.OnPopupButtonClick += RewardTrackHudProgress_OnPopupButtonClick;
            cts = new CancellationTokenSource();
            StartAsync(cts.Token).Forget();
        }

        public override void Stop()
        {
            StopWaitingForPopup();
            rewardTrackHudProgress.OnPopupButtonClick -= RewardTrackHudProgress_OnPopupButtonClick;
            cts.Cancel();
            cts.Dispose();

            inputUiService.EnableInput();

            RemoveDynamicElements();
            maskTutorialPanel.Release(this);
        }

        private async UniTaskVoid StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.WaitUntil(() => !popupService.IsAnyOpened, cancellationToken: cancellationToken);
                inputUiService.DisableInput();
                await UniTask.WaitForSeconds(0.5f, true, cancellationToken: cancellationToken);                
                maskTutorialPanel.Acquire(this);
                AddDynamicElements(rewardTrackHudProgress.MainRectTransform,
                                   rewardTrackHudProgress.ItemRectTransform, 
                                   rewardTrackUnlockData);
            }
            catch (OperationCanceledException e)
            {
                Debug.LogWarning($"[CODE] Operation was cancelled: {e.Message}");
            }
            inputUiService.EnableInput();
        }

        private async UniTaskVoid StartInfoAsync(CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.WaitUntil(() => { return rewardTrackStatePopup != null && rewardTrackStatePopup.IsOpened; }, cancellationToken: cancellationToken);
                await UniTask.WaitForSeconds(0.5f, true, cancellationToken: cancellationToken);
                rewardTrackStatePopup.InfoPopupRequest();
                await UniTask.WaitForSeconds(1f, true, cancellationToken: cancellationToken);
                Complete();
            }
            catch (OperationCanceledException e)
            {
                Debug.LogWarning($"[CODE] Operation was cancelled: {e.Message}");
            }
            inputUiService.EnableInput();
        }

        private void Complete()
        {
            rewardTrackHudProgress.OnPopupButtonClick -= RewardTrackHudProgress_OnPopupButtonClick;

            StopWaitingForPopup();

            OnComplete?.Invoke(TutorialId);
        }

        private void RewardTrackHudProgress_OnPopupButtonClick()
        {
            RemoveDynamicElements();
            maskTutorialPanel.Release(this);
            inputUiService.DisableInput();                       
        }

        private void StartWaitingForPopup()
        {
            popupService.OnChangeState += PopupService_OnChangeState;
        }

        private void StopWaitingForPopup()
        {
            popupService.OnChangeState -= PopupService_OnChangeState;
        }

        private void PopupService_OnChangeState(PopupBase popup, PopupState state)
        {
            if (state is not PopupState.BeginOpen || popup is not RewardTrackStatePopup)
                return;
                       
            rewardTrackStatePopup = popup as RewardTrackStatePopup;
            StartInfoAsync(cts.Token).Forget();
        }

        private void AddDynamicElements(RectTransform mainRectTransform, RectTransform itemRectTransform, RewardTrackUnlockData rewardTrackUnlockData)
        {
            complexMaskIds = maskTutorialPanel.AddNewComplexMask(mainRectTransform);

            if (rewardTrackUnlockData == null)
                return;

            tooltipId = maskTutorialPanel.AddNewTooltip(LocalizationService.I.Get(LocKeys.RewardTrackTutorial.UnlockTooltip),
                                                        mainRectTransform,
                                                        rewardTrackUnlockData.tooltipOrientation,
                                                        new(rewardTrackUnlockData.tooltipOffsetX,        rewardTrackUnlockData.tooltipOffsetY),
                                                        new(rewardTrackUnlockData.tooltipSizeHorizontal, rewardTrackUnlockData.tooltipSizeVertical));

            pointerId = maskTutorialPanel.AddNewPointer(itemRectTransform,
                                                        rewardTrackUnlockData.pointerOrientation,
                                                        new(rewardTrackUnlockData.pointerOffsetX,        rewardTrackUnlockData.pointerOffsetY),
                                                        new(rewardTrackUnlockData.pointerSizeHorizontal, rewardTrackUnlockData.pointerSizeVertical));
        }

        private void RemoveDynamicElements()
        {
            maskTutorialPanel.RemoveComplexMask(complexMaskIds);
            maskTutorialPanel.RemovePointer(pointerId);
            maskTutorialPanel.RemoveTooltip(tooltipId);
        }
    }
}