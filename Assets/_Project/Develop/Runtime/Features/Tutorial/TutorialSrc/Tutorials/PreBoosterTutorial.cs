using Cysharp.Threading.Tasks;
using Features.Boosters;
using Features.Level;
using Infrastructure.InputControl;
using Infrastructure.Localization;
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
    public class PreBoosterTutorial : TutorialBase
    {
        private SceneLoadController sceneLoadController;
        private LevelService levelService;
        private PopupService popupService;
        private InputUiService inputUiService;
        private BoosterData boosterData;
        private MaskTutorialPanel maskTutorialPanel;
        private List<string> complexMaskIds = new();
        private string tooltipId = string.Empty;
        private string pointerId = string.Empty;
        private CancellationTokenSource cts;
        private PreBoosterUseData preBoosterUseData;
        private PreBoostersPopup preBoostersPopup;

        public PreBoosterTutorial(string tutorialId,
                                  PopupService popupService,
                                  MainUIProvider mainUIProvider,
                                  SceneLoadController sceneLoadController,
                                  LevelService levelService,
                                  InputUiService inputUiService,
                                  BoosterData boosterData,
                                  PreBoosterUseData preBoosterUseData) : base(tutorialId)
        {
            this.sceneLoadController = sceneLoadController;
            this.levelService = levelService;
            this.popupService = popupService;
            this.inputUiService = inputUiService;
            this.boosterData = boosterData;
            maskTutorialPanel = mainUIProvider.MaskTutorialPanel;

            this.preBoosterUseData = preBoosterUseData;
        }

        public override bool CanBeEqueued()
        {
            return levelService.CurrentLevelNumber == boosterData.unlockLevel
                && sceneLoadController.IsMetaSceneActive;
        }

        public override bool CanBeStarted()
        {
            return popupService.IsPopupOpened(typeof(PreBoostersPopup))
                && levelService.CurrentLevelNumber == boosterData.unlockLevel
                && sceneLoadController.IsMetaSceneActive;
        }

        public override void Start()
        {
            cts = new CancellationTokenSource();
            StartAsync(preBoosterUseData, cts.Token).Forget();
        }

        public override void Stop()
        {
            inputUiService.EnableInput();

            cts?.Cancel();
            cts?.Dispose();

            if (preBoostersPopup != null)
                preBoostersPopup.OnSlotClick -= PreBoostersPopup_OnSlotClick;

            RemoveDynamicElements();
            maskTutorialPanel.Release(this);
        }

        private void Complete()
        {
            RemoveDynamicElements();
            maskTutorialPanel.Release(this);
            OnComplete?.Invoke(TutorialId);
        }

        private async UniTaskVoid StartAsync(PreBoosterUseData preBoosterUseData, CancellationToken cancellationToken)
        {
            try
            {
                preBoostersPopup = await popupService.GetAsync<PreBoostersPopup>(cancellationToken);
                inputUiService.DisableInput();
                await UniTask.WaitForSeconds(0.5f, cancellationToken: cancellationToken);
                var rectTransform = preBoostersPopup.GetBoosterViewRectTransform(boosterData.type);
                maskTutorialPanel.Acquire(this);
                AddDynamicElements(rectTransform, preBoosterUseData);
                inputUiService.EnableInput();
                preBoostersPopup.OnSlotClick += PreBoostersPopup_OnSlotClick;
            }
            catch (OperationCanceledException e)
            {
                inputUiService.EnableInput();
                Debug.LogWarning($"[CODE] Operation was cancelled: {e.Message}");
            }            
        }

        private void PreBoostersPopup_OnSlotClick(BoosterType type, bool isSelected)
        {
            if (type != boosterData.type)
                return;

            if (!isSelected)
                return;

            preBoostersPopup.OnSlotClick -= PreBoostersPopup_OnSlotClick;
            Complete();
        }
        
        private void AddDynamicElements(RectTransform rectTransform, PreBoosterUseData preBoosterUseData) 
        {
            complexMaskIds = maskTutorialPanel.AddNewComplexMask(rectTransform);

            if (preBoosterUseData == null)
                return;

            tooltipId = maskTutorialPanel.AddNewTooltip(LocalizationService.I.Get(boosterData.descriptionTutorialKey, boosterData.boostedValue.ToString()), 
                                                        rectTransform,
                                                        preBoosterUseData.tooltipOrientation,
                                                        new(preBoosterUseData.tooltipOffsetX, preBoosterUseData.tooltipOffsetY),
                                                        new(preBoosterUseData.tooltipSizeHorizontal, preBoosterUseData.tooltipSizeVertical));
            pointerId = maskTutorialPanel.AddNewPointer(rectTransform, 
                                                        preBoosterUseData.pointerOrientation,
                                                        new(preBoosterUseData.pointerOffsetX, preBoosterUseData.pointerOffsetY),
                                                        new(preBoosterUseData.pointerSizeHorizontal, preBoosterUseData.pointerSizeVertical));
        }

        private void RemoveDynamicElements()
        {
            maskTutorialPanel.RemoveComplexMask(complexMaskIds);
            maskTutorialPanel.RemoveTooltip(tooltipId);
            maskTutorialPanel.RemovePointer(pointerId);
        }
    }
}