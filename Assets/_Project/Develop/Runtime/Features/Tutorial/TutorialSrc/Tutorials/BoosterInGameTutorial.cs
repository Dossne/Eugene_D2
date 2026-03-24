using Cysharp.Threading.Tasks;
using Features.Boosters;
using Features.Level;
using Infrastructure.Localization;
using Infrastructure.MainUICanvasControl;
using Infrastructure.SceneManagement;
using Infrastructure.TooltipControl;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Features.Tutorial
{
    public class BoosterInGameTutorial : TutorialBase
    {
        private MainUIProvider mainUIProvider;
        private SceneLoadController sceneLoadController;
        private LevelService levelService;
        private BoosterData boosterData;
        private MaskTutorialPanel maskTutorialPanel;
        private List<string> complexMaskId = new();
        private string tooltipId = string.Empty;
        private string pointerId = string.Empty;
        private InGameBoosterSlotView boosterSlotView;
        private CancellationTokenSource cts;
        private UiOrientationType tooltipOrientationType;
        private InGameBoosterUseData inGameBoosterUseData;

        public BoosterInGameTutorial(string tutorialId,
                                     MainUIProvider mainUIProvider,
                                     SceneLoadController sceneLoadController,
                                     LevelService levelService,
                                     TooltipService tooltipService,
                                     BoosterData boosterData,
                                     InGameBoosterUseData inGameBoosterUseData) : base(tutorialId)
        {
            this.mainUIProvider = mainUIProvider;
            this.sceneLoadController = sceneLoadController;
            this.levelService = levelService;
            this.boosterData = boosterData;
            maskTutorialPanel = mainUIProvider.MaskTutorialPanel;

            this.inGameBoosterUseData = inGameBoosterUseData;
        }


        public override bool CanBeEqueued()
        {
            return CanBeStarted();
        }


        public override bool CanBeStarted()
        {
            return levelService.CurrentLevelNumber == boosterData.unlockLevel
                && sceneLoadController.IsGameSceneActive;
        }

        public override void Start()
        {
            cts = new CancellationTokenSource();
            StartAsync(inGameBoosterUseData, cts.Token).Forget();
        }

        private void BoosterSlot_OnClick(BoosterType boosterType)
        {
            if (boosterType != boosterData.type)
                return;

            Complete();
        }

        public override void Stop()
        {
            cts.Cancel();
            cts.Dispose();

            if (boosterSlotView != null)
                boosterSlotView.OnClick -= BoosterSlot_OnClick;
            RemoveDynamicElements();
            maskTutorialPanel.Release(this);
        }

        private void Complete()
        {
            if (boosterSlotView != null)
                boosterSlotView.OnClick -= BoosterSlot_OnClick;
            RemoveDynamicElements();
            maskTutorialPanel.Release(this);
            OnComplete?.Invoke(TutorialId);
        }

        private async UniTaskVoid StartAsync(InGameBoosterUseData inGameBoosterUseData, CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.WaitForSeconds(0.5f);
                boosterSlotView = mainUIProvider.HudProvider.BoostersPanelHud.GetSlotView(boosterData.type);
                var rectTransform = boosterSlotView.GetComponent<RectTransform>();

                maskTutorialPanel.Acquire(this);
                AddDynamicElements(rectTransform, inGameBoosterUseData);
                boosterSlotView.OnClick += BoosterSlot_OnClick;
            }
            catch (OperationCanceledException e)
            {
                Debug.LogWarning($"[CODE] Operation was cancelled: {e.Message}");
            }            
        }

        private void AddDynamicElements(RectTransform rectTransform, InGameBoosterUseData inGameBoosterUseData) 
        {
            complexMaskId = maskTutorialPanel.AddNewComplexMask(rectTransform);

            if (inGameBoosterUseData == null)
                return;

            tooltipId = maskTutorialPanel.AddNewTooltip(LocalizationService.I.Get(boosterData.descriptionTutorialKey, boosterData.durationSec.ToString()), 
                                                        rectTransform,
                                                        inGameBoosterUseData.tooltipOrientation,
                                                        new(inGameBoosterUseData.tooltipOffsetX, inGameBoosterUseData.tooltipOffsetY),
                                                        new(inGameBoosterUseData.tooltipSizeHorizontal, inGameBoosterUseData.tooltipSizeVertical));
            pointerId = maskTutorialPanel.AddNewPointer(rectTransform, 
                                                        inGameBoosterUseData.pointerOrientation,
                                                        new(inGameBoosterUseData.pointerOffsetX, inGameBoosterUseData.pointerOffsetY),
                                                        new(inGameBoosterUseData.pointerSizeHorizontal, inGameBoosterUseData.pointerSizeVertical));
        }

        private void RemoveDynamicElements()
        {
            maskTutorialPanel.RemoveComplexMask(complexMaskId);
            maskTutorialPanel.RemoveTooltip(tooltipId);
            maskTutorialPanel.RemovePointer(pointerId);
        }
    }
}