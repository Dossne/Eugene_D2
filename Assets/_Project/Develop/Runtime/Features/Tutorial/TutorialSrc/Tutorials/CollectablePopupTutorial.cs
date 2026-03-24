using Cysharp.Threading.Tasks;
using Features.Collectables;
using Features.Level;
using Features.LevelConfiguration;
using Infrastructure.Localization;
using Infrastructure.Popups;
using Infrastructure.SceneManagement;
using Infrastructure.SpriteAtlasControl;
using System.Threading;

namespace Features.Tutorial
{
    public class CollectablePopupTutorial : TutorialBase
    {
        private LevelService levelService;
        private CollectableType collectableType;
        private PopupService popupService;
        private SceneLoadController sceneLoadController;
        private SpriteAtlasService spriteAtlasService;
        private CollectableTutorialPopup collectableTutorialPopup;
        private string descriptionKey;
        private CollectablesConfig collectablesConfig;
        private CancellationTokenSource cts;

        public CollectablePopupTutorial(string tutorialId, 
                                        PopupService popupService,
                                        LevelService levelService,
                                        SceneLoadController sceneLoadController,
                                        SpriteAtlasService spriteAtlasService,
                                        CollectablesConfig collectablesConfig,
                                        CollectableType collectableType,
                                        string descriptionKey) : base(tutorialId)
        {
            this.levelService = levelService;
            this.popupService = popupService;
            this.collectableType = collectableType;
            this.sceneLoadController = sceneLoadController;
            this.spriteAtlasService = spriteAtlasService;
            this.descriptionKey = descriptionKey;
            this.collectablesConfig = collectablesConfig;
        }

        public override bool CanBeEqueued()
        {
            return sceneLoadController.IsGameSceneActive;
        }

        public override bool CanBeStarted()
        {
            return sceneLoadController.IsGameSceneActive && levelService.CurrentLevelHasCollectable(collectableType);
        }

        public override void Start()
        {
            cts = new CancellationTokenSource();
            ShowPopupAsync(cts.Token).Forget();            
        }

        

        private async UniTaskVoid ShowPopupAsync(CancellationToken cancellationToken)
        {
            if (collectableTutorialPopup == null)
            {
                collectableTutorialPopup = await popupService.GetAsync<CollectableTutorialPopup>(cancellationToken, isInstantiateAsync: false);
                collectableTutorialPopup.Initialize();   
                
            }
            await UniTask.WaitForSeconds(0.5f);
            collectableTutorialPopup.Setup(LocalizationService.I.Get(LocKeys.CollectableTutorialPopup.Header),
                                           LocalizationService.I.Get(descriptionKey),
                                           LocalizationService.I.Get(LocKeys.CollectableTutorialPopup.Button),
                                           spriteAtlasService.GetFromMain(collectablesConfig.Get(collectableType).iconName));

            collectableTutorialPopup.Open();
            collectableTutorialPopup.OnChangeState += CollectableTutorialPopup_OnChangeState;
        }

        private void CollectableTutorialPopup_OnChangeState(PopupBase obj, PopupState state)
        {
            if(state is not PopupState.Closed)
                return;
            
            collectableTutorialPopup.OnChangeState -= CollectableTutorialPopup_OnChangeState;
            OnComplete?.Invoke(TutorialId);
        }

        public override void Stop()
        {
            if (collectableTutorialPopup == null)
                return;

            collectableTutorialPopup.OnChangeState -= CollectableTutorialPopup_OnChangeState;
            collectableTutorialPopup.Close();
        }
    }
}