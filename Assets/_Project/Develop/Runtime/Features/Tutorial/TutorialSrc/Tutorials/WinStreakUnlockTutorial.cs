using Cysharp.Threading.Tasks;
using Features.Boosters;
using Features.Level;
using Features.WinStreak;
using Infrastructure.InputControl;
using Infrastructure.MainUICanvasControl;
using Infrastructure.Popups;
using Infrastructure.SceneManagement;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Features.Tutorial
{
    public class WinStreakUnlockTutorial : TutorialBase
    {
        private PopupService popupService;
        private WinStreakStateController winStreakStateController;
        private SceneLoadController sceneLoadController;
        private LevelService levelService;
        private InputUiService inputUiService;
        private MaskTutorialPanel maskTutorialPanel;
        private List<string> complexMaskIds = new();
        private string pointerId = string.Empty;
        private string buttonId = string.Empty;
        private PreBoostersPopup preBoostersPopup;
        private CancellationTokenSource cts;
        private WinStreakUnlockData winStreakUnlockData;

        public WinStreakUnlockTutorial(string tutorialId,
                                       PopupService popupService,
                                       MainUIProvider mainUIProvider,
                                       LevelService levelService,
                                       InputUiService inputUiService,
                                       WinStreakStateController winStreakStateController,
                                       SceneLoadController sceneLoadController,
                                       WinStreakUnlockData winStreakUnlockData) : base(tutorialId)
        {
            this.popupService = popupService;
            this.winStreakStateController = winStreakStateController;
            this.sceneLoadController = sceneLoadController;
            this.levelService = levelService;
            this.inputUiService = inputUiService;

            maskTutorialPanel = mainUIProvider.MaskTutorialPanel;
            this.winStreakUnlockData = winStreakUnlockData;
        }


        public override bool CanBeEqueued()
        {
            return winStreakStateController.IsUnlocked
                && winStreakStateController.UnlockLevel == levelService.CurrentLevelNumber
                && sceneLoadController.IsMetaSceneActive;
        }


        public override bool CanBeStarted()
        {
            return popupService.IsPopupOpened(typeof(PreBoostersPopup))
                && winStreakStateController.IsUnlocked
                && winStreakStateController.UnlockLevel == levelService.CurrentLevelNumber
                && sceneLoadController.IsMetaSceneActive;
        }

        public override void Start()
        {
            cts = new CancellationTokenSource();
            StartAsync(cts.Token).Forget();
        }

        public override void Stop()
        {
            cts?.Cancel();
            cts?.Dispose();

            RemoveDynamicElements();
            maskTutorialPanel.Release(this);
        }

        private async UniTaskVoid StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                preBoostersPopup = await popupService.GetAsync<PreBoostersPopup>(cancellationToken);
                inputUiService.DisableInput();
                await UniTask.WaitForSeconds(0.5f, true, cancellationToken: cancellationToken);
                var panelRectTransform = preBoostersPopup.WinStreakUIPanel.PanelRect;
                var infoRectTransform  = preBoostersPopup.WinStreakUIPanel.InfoButtonRect;
                maskTutorialPanel.Acquire(this);
                AddDynamicElements(panelRectTransform, infoRectTransform, winStreakUnlockData);
                inputUiService.EnableInput();
            }
            catch (OperationCanceledException e)
            {
                inputUiService.EnableInput();
                Debug.LogWarning($"[CODE] Operation was cancelled: {e.Message}");
            }
        }

        private void Complete()
        {
            RemoveDynamicElements();
            maskTutorialPanel.Release(this);
            OnComplete?.Invoke(TutorialId);
        }

        private void AddDynamicElements(RectTransform panelRectTransform, RectTransform infoRectTransform, WinStreakUnlockData winStreakUnlockData)
        {
            complexMaskIds = maskTutorialPanel.AddNewComplexMask(panelRectTransform);
            buttonId = maskTutorialPanel.AddNewButton(null, TutorialButtonHandler);

            if (winStreakUnlockData == null)
                return;

            pointerId = maskTutorialPanel.AddNewPointer(infoRectTransform,
                                                        winStreakUnlockData.pointerOrientation,
                                                        new(winStreakUnlockData.pointerOffsetX,        winStreakUnlockData.pointerOffsetY),
                                                        new(winStreakUnlockData.pointerSizeHorizontal, winStreakUnlockData.pointerSizeVertical));
        }

        private void RemoveDynamicElements()
        {
            maskTutorialPanel.RemoveComplexMask(complexMaskIds);
            maskTutorialPanel.RemovePointer(pointerId);
            maskTutorialPanel.RemoveButton(buttonId);
        }

        private void TutorialButtonHandler()
        {
            preBoostersPopup.WinStreakUIPanel.OpenTooltip();
            Complete();
        }
    }
}