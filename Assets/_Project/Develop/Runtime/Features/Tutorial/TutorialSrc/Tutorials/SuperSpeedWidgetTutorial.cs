using Cysharp.Threading.Tasks;
using Features.Boosters;
using Features.Level;
using Features.SuperSpeedMode;
using Infrastructure.InputControl;
using Infrastructure.MainUICanvasControl;
using Infrastructure.Popups;
using Infrastructure.SceneManagement;
using Infrastructure.TooltipControl;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Features.Tutorial
{
    public class SuperSpeedWidgetTutorial : TutorialBase
    {
        private readonly SceneLoadController sceneLoadController;
        private readonly LevelService levelService;
        private readonly PopupService popupService;
        private readonly InputUiService inputUiService;
        private readonly MaskTutorialPanel maskTutorialPanel;
        private List<string> complexMaskIds = new();
        private string pointerId = string.Empty;
        private CancellationTokenSource cts;
        private PreBoostersPopup preBoostersPopup;
        private readonly SuperSpeedData superSpeedData;
        private readonly SuperSpeedWidgetData superSpeedWidgetData;

        public SuperSpeedWidgetTutorial(string tutorialId,
                                        PopupService popupService,
                                        MainUIProvider mainUIProvider,
                                        SceneLoadController sceneLoadController,
                                        LevelService levelService,
                                        InputUiService inputUiService,
                                        SuperSpeedData superSpeedData,
                                        SuperSpeedWidgetData superSpeedWidgetData) : base(tutorialId)
        {
            this.sceneLoadController = sceneLoadController;
            this.levelService = levelService;
            this.popupService = popupService;
            this.superSpeedWidgetData = superSpeedWidgetData;
            this.superSpeedData = superSpeedData;
            this.inputUiService = inputUiService;
            maskTutorialPanel = mainUIProvider.MaskTutorialPanel;
        }

        public override bool CanBeEqueued()
        {
            return superSpeedData.isFeatureEnabled
                && levelService.CurrentLevelNumber == superSpeedData.unlockLevel
                && sceneLoadController.IsMetaSceneActive;
        }

        public override bool CanBeStarted()
        {
            return popupService.IsPopupOpened(typeof(PreBoostersPopup))
                && superSpeedData.isFeatureEnabled
                && levelService.CurrentLevelNumber == superSpeedData.unlockLevel
                && sceneLoadController.IsMetaSceneActive;
        }

        public override void Start()
        {
            cts = new CancellationTokenSource();
            StartAsync(superSpeedWidgetData, cts.Token).Forget();
        }

        public override void Stop()
        {
            cts?.Cancel();
            cts?.Dispose();

            if (preBoostersPopup != null)
                preBoostersPopup.OnSuperSpeedWidgetClick -= PreBoostersPopup_OnSuperSpeedWidgetClick;

            RemoveDynamicElements();
            maskTutorialPanel.Release(this);
        }

        private void Complete()
        {
            RemoveDynamicElements();
            maskTutorialPanel.Release(this);
            OnComplete?.Invoke(TutorialId);
        }

        private async UniTaskVoid StartAsync(SuperSpeedWidgetData superSpeedWidgetData, CancellationToken cancellationToken)
        {
            try
            {
                preBoostersPopup = await popupService.GetAsync<PreBoostersPopup>(cancellationToken);
                inputUiService.DisableInput();
                await UniTask.WaitForSeconds(0.5f, cancellationToken: cancellationToken);
                var rectTransform = preBoostersPopup.GetSuperSpeedWidgetRectTransform();
                maskTutorialPanel.Acquire(this);
                AddDynamicElements(rectTransform, superSpeedWidgetData);
                inputUiService.EnableInput();
                preBoostersPopup.OnSuperSpeedWidgetClick += PreBoostersPopup_OnSuperSpeedWidgetClick;
            }
            catch (OperationCanceledException e)
            {
                inputUiService.EnableInput();
                Debug.LogWarning($"[CODE] Operation was cancelled: {e.Message}");
            }            
        }

        private void PreBoostersPopup_OnSuperSpeedWidgetClick()
        {
            preBoostersPopup.OnSuperSpeedWidgetClick -= PreBoostersPopup_OnSuperSpeedWidgetClick;
            Complete();
        }
        
        private void AddDynamicElements(RectTransform rectTransform, SuperSpeedWidgetData superSpeedWidgetData) 
        {
            complexMaskIds = maskTutorialPanel.AddNewComplexMask(rectTransform);

            if (superSpeedWidgetData == null)
                return;

            pointerId = maskTutorialPanel.AddNewPointer(rectTransform,
                                                        superSpeedWidgetData.pointerOrientation,
                                                        new(superSpeedWidgetData.pointerOffsetX,        superSpeedWidgetData.pointerOffsetY),
                                                        new(superSpeedWidgetData.pointerSizeHorizontal, superSpeedWidgetData.pointerSizeVertical));
        }

        private void RemoveDynamicElements()
        {
            maskTutorialPanel.RemoveComplexMask(complexMaskIds);
            maskTutorialPanel.RemovePointer(pointerId);
        }
    }
}