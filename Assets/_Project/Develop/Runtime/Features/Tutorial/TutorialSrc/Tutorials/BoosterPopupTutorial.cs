using Cysharp.Threading.Tasks;
using Features.Boosters;
using Features.Level;
using Infrastructure.Localization;
using Infrastructure.Popups;
using Infrastructure.SceneManagement;
using Infrastructure.SpriteAtlasControl;
using System;
using System.Threading;
using UnityEngine;

namespace Features.Tutorial
{
    public class BoosterPopupTutorial : TutorialBase
    {
        private LevelService levelService;
        private BoosterData boosterData;
        private PopupService popupService;
        private SceneLoadController sceneLoadController;
        private SpriteAtlasService spriteAtlasService;
        private ItemTutorialPopup itemTutorialPopup;
        private CancellationTokenSource cts;

        public BoosterPopupTutorial(string tutorialId,
                                    PopupService popupService,
                                    LevelService levelService,
                                    SceneLoadController sceneLoadController,
                                    SpriteAtlasService spriteAtlasService,
                                    BoosterData boosterData) : base(tutorialId)
        {
            this.levelService = levelService;
            this.popupService = popupService;
            this.boosterData = boosterData;
            this.sceneLoadController = sceneLoadController;
            this.spriteAtlasService = spriteAtlasService;
        }

        public override bool CanBeEqueued()
        {
            return CanBeStarted();
        }

        public override bool CanBeStarted()
        {
            return levelService.CurrentLevelNumber == boosterData.unlockLevel
                && sceneLoadController.IsMetaSceneActive;
        }

        public override void Start()
        {
            cts = new CancellationTokenSource();
            ShowPopupAsync(cts.Token).Forget();            
        }

        private async UniTaskVoid ShowPopupAsync(CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.WaitUntil(() => !popupService.IsAnyOpened, cancellationToken: cancellationToken);
                if (itemTutorialPopup == null)
                {
                    itemTutorialPopup = await popupService.GetAsync<ItemTutorialPopup>(cancellationToken, isInstantiateAsync: false);
                    itemTutorialPopup.Construct(Complete);
                    itemTutorialPopup.Initialize();
                }
                itemTutorialPopup.SetData(LocalizationService.I.Get(boosterData.nameKey),
                                          LocalizationService.I.Get(LocKeys.ItemTutorialPopup.SubHeader),
                                          LocalizationService.I.Get(boosterData.descriptionKey, boosterData.durationSec.ToString()),
                                          new()
                                          {
                                              icon = spriteAtlasService.GetFromMain(boosterData.shopIconName),
                                              amountText = boosterData.startCount.ToString(),
                                              backgroundSprite = null,
                                              isDisplayInfinityIcon = false,
                                              isDisplayRibbon = false
                                          });
                itemTutorialPopup.Open();
                itemTutorialPopup.OnChangeState += ItemTutorialPopup_OnChangeState;
                await UniTask.WaitUntil(() => itemTutorialPopup.IsOpened, cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException e)
            {
                Debug.LogWarning($"[CODE] Operation was cancelled: {e.Message}");
            }
        }

        private void Complete()
        {
            itemTutorialPopup.Close();
        }

        private void ItemTutorialPopup_OnChangeState(PopupBase obj, PopupState state)
        {
            if(state is not PopupState.Closed)
                return;
            
            itemTutorialPopup.OnChangeState -= ItemTutorialPopup_OnChangeState;
            OnComplete?.Invoke(TutorialId);
        }

        public override void Stop()
        {
            cts?.Cancel();
            cts?.Dispose();

            if (itemTutorialPopup == null)
                return;

            itemTutorialPopup.OnChangeState -= ItemTutorialPopup_OnChangeState;
            itemTutorialPopup.Close();
        }
    }
}