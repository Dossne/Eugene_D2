using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.LavaQuest;
using Infrastructure.InputControl;
using Infrastructure.Localization;
using Infrastructure.Popups;
using Infrastructure.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Tutorial
{
    public class LavaQuestStartTutorial : TutorialBase
    {
        private readonly List<LavaQuestStartData> lavaQuestStartData;
        private readonly InputUiService inputUiService;
        private readonly PopupService popupService;
        private readonly SceneLoadController sceneLoadController;
        private readonly MaskTutorialPanel maskTutorialPanel;
        private readonly LavaQuestStateController lavaQuestStateController;
        private readonly List<string> maskIds = new();
        private LavaQuestEventPopup eventPopup;
        private CancellationTokenSource cts;
        private string tooltipId;
        private string buttonId;


        public LavaQuestStartTutorial(string tutorialId,
                                      InputUiService inputUiService,
                                      PopupService popupService,
                                      SceneLoadController sceneLoadController,
                                      List<LavaQuestStartData> configData,
                                      MaskTutorialPanel maskTutorialPanel,
                                      LavaQuestStateController lavaQuestStateController) :
            base(tutorialId)
        {
            this.lavaQuestStartData = configData;
            this.maskTutorialPanel = maskTutorialPanel;
            this.lavaQuestStateController = lavaQuestStateController;

            this.inputUiService = inputUiService;
            this.popupService = popupService;
            this.sceneLoadController = sceneLoadController;
        }

        public override bool CanBeEqueued()
        {
            return lavaQuestStateController.IsUnlocked()
                && lavaQuestStateController.IsFeatureEnabled()
                && lavaQuestStateController.CurrentStepIdx == 0
                && sceneLoadController.IsMetaSceneActive;
        }

        public override bool CanBeStarted()
        {
            return popupService.IsPopupOpened(typeof(LavaQuestEventPopup))
                && lavaQuestStateController.IsUnlocked()
                && lavaQuestStateController.IsFeatureEnabled()
                && lavaQuestStateController.CurrentStepIdx == 0
                && sceneLoadController.IsMetaSceneActive;
        }


        public override void Start()
        {
            cts = new CancellationTokenSource();
            inputUiService.DisableInput();
            Step1_ShowTooltipOnIconsAsync().Forget();
        }


        public override void Stop()
        {
            cts?.Cancel();
            cts?.Dispose();
            maskIds.Clear();
        }


        private async UniTaskVoid Step1_ShowTooltipOnIconsAsync()
        {
            eventPopup = await popupService.GetAsync<LavaQuestEventPopup>(cts.Token);
            if (!eventPopup.TryGetActiveEventZone(out LavaQuestEventZone zone))
                return;

            LavaQuestStartData configData = lavaQuestStartData[0];

            await UniTask.WaitForSeconds(configData.stepDelay, true, cancellationToken: cts.Token);
            await UniTask.WaitUntil(() => eventPopup.IsOpenAnimationFinished, cancellationToken: cts.Token);
            maskTutorialPanel.Acquire(this);

            maskIds.Clear();

            Image firstPlatform = zone.Platform[0].VisualRootImage;
            string platformId = maskTutorialPanel.AddNewMask(firstPlatform.transform as RectTransform, firstPlatform);
            maskIds.Add(platformId);

            RectTransform character = null;
            for (var i = 0; i < eventPopup.CharacterIcons.Count; i++)
            {
                var icon = eventPopup.CharacterIcons[i];
                var id = maskTutorialPanel.AddNewMask(icon.MainRoot, icon.IconImage);
                maskIds.Add(id);

                if (i == 0)
                    character = icon.MainRoot;
            }

            for (int i = 0; i < eventPopup.PlayerImages.Count; i++)
            {
                var img = eventPopup.PlayerImages[i];
                var id = maskTutorialPanel.AddNewMask(img.transform as RectTransform, img);
                maskIds.Add(id);
            }
            
            string text = LocalizationService.I.Get(LocKeys.LavaQuest.Tutorial_1);

            tooltipId = maskTutorialPanel.AddNewTooltip(text, character, configData.tooltipOrientation,
                                                        new(configData.tooltipOffsetX, configData.tooltipOffsetY),
                                                        new(configData.tooltipSizeHorizontal, configData.tooltipSizeVertical));

            buttonId = maskTutorialPanel.AddNewButton(maskTutorialPanel.ButtonParent, Step2_ShowTooltipOnPlatform);
            inputUiService.EnableInput();
        }


        private void Step2_ShowTooltipOnPlatform()
        {
            CompleteStep();

            if (!eventPopup.TryGetActiveEventZone(out LavaQuestEventZone zone))
                return;

            inputUiService.DisableInput();

            for (int i = 0; i < eventPopup.LevelImages.Count; i++)
            {
                var img = eventPopup.LevelImages[i];
                var id = maskTutorialPanel.AddNewMask(img.transform as RectTransform, img);
                maskIds.Add(id);
            }

            int centerIdx = Mathf.CeilToInt(zone.Platform.Count * 0.5f);
            Image centerPlatform = zone.Platform[centerIdx].VisualRootImage;
            var platformId = maskTutorialPanel.AddNewMask(centerPlatform.transform as RectTransform, centerPlatform);
            maskIds.Add(platformId);

            string text = LocalizationService.I.Get(LocKeys.LavaQuest.Tutorial_2);
            LavaQuestStartData configData = lavaQuestStartData[1];

            tooltipId = maskTutorialPanel.AddNewTooltip(text, centerPlatform.transform as RectTransform, configData.tooltipOrientation,
                                                        new(configData.tooltipOffsetX, configData.tooltipOffsetY),
                                                        new(configData.tooltipSizeHorizontal, configData.tooltipSizeVertical));

            buttonId = maskTutorialPanel.AddNewButton(maskTutorialPanel.ButtonParent, Step3_ShowTooltipOnTreasure);
            inputUiService.EnableInput();
        }


        private void Step3_ShowTooltipOnTreasure()
        {
            CompleteStep();

            inputUiService.DisableInput();

            for (int i = 0; i < eventPopup.TreasureImage.Count; i++)
            {
                var id = maskTutorialPanel.AddNewMask(eventPopup.TreasureImage[i].transform as RectTransform, eventPopup.TreasureImage[i]);
                maskIds.Add(id);
            }


            string text = LocalizationService.I.Get(LocKeys.LavaQuest.Tutorial_3);
            LavaQuestStartData configData = lavaQuestStartData[2];

            tooltipId = maskTutorialPanel.AddNewTooltip(text, eventPopup.TreasureImage[0].transform as RectTransform, configData.tooltipOrientation,
                                                        new(configData.tooltipOffsetX, configData.tooltipOffsetY),
                                                        new(configData.tooltipSizeHorizontal, configData.tooltipSizeVertical));

            buttonId = maskTutorialPanel.AddNewButton(maskTutorialPanel.ButtonParent, CompleteTutor);
            inputUiService.EnableInput();
        }


        private void CompleteStep()
        {
            maskTutorialPanel.RemoveComplexMask(maskIds);
            maskTutorialPanel.RemoveTooltip(tooltipId);
            maskTutorialPanel.RemoveButton(buttonId);
            maskIds.Clear();
            tooltipId = null;
            buttonId = null;
        }


        private void CompleteTutor()
        {
            CompleteStep();
            inputUiService.EnableInput();
            maskTutorialPanel.Release(this);
            OnComplete?.Invoke(TutorialId);
        }
    }
}