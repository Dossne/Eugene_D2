using Cysharp.Threading.Tasks;
using Features.Collectables;
using Features.Level;
using Infrastructure.Configs;
using Infrastructure.InputControl;
using Infrastructure.MainUICanvasControl;
using Infrastructure.SceneManagement;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.TooltipControl;
using R3;
using System;
using System.Threading;
using UnityEngine;


namespace Features.Tutorial
{
    public class EatTooltipTutorial : TutorialBase
    {
        private const string TOOLTIP_ASSET_ID = "EatTutorialTooltip";

        private SceneLoadController sceneLoadController;
        private TooltipService tooltipService;
        private MainUIProvider mainUIProvider;
        private InputService inputService;
        private LevelService levelService;
        private SpriteAtlasService spriteAtlasService;
        private EatTutorialTooltip tooltip;
        private CancellationTokenSource cts;
        private TaskCompleteTutorialEvent taskCompleteTutorialEvent;
        private ConfigProvider configProvider;
        private CompositeDisposable disposables;
        private CollectableType targetType = CollectableType.CucumberSliceSmile;

        public EatTooltipTutorial(string tutorialId,
                                  SceneLoadController sceneLoadController,
                                  TooltipService tooltipService,
                                  MainUIProvider mainUIProvider,
                                  InputService inputService,
                                  LevelService levelService,
                                  SpriteAtlasService spriteAtlasService,
                                  ConfigProvider configProvider,
                                  TaskCompleteTutorialEvent taskCompleteTutorialEvent) : base(tutorialId)
        {
            this.sceneLoadController = sceneLoadController;
            this.tooltipService = tooltipService;
            this.mainUIProvider = mainUIProvider;
            this.inputService = inputService;
            this.levelService = levelService;
            this.spriteAtlasService = spriteAtlasService;
            this.taskCompleteTutorialEvent = taskCompleteTutorialEvent;
            this.configProvider = configProvider;
            this.disposables = new CompositeDisposable();
        }

        public override bool CanBeEqueued()
        {
            return CanBeStarted();
        }

        public override bool CanBeStarted()
        {
            return sceneLoadController.IsGameSceneActive && levelService.CurrentLevelNumber == 1;
        }

        public override void Start()
        {
            disposables = new CompositeDisposable();
            cts = new CancellationTokenSource();
            ShowTooltipAsync(cts.Token).Forget();
        }

        private async UniTaskVoid ShowTooltipAsync(CancellationToken cancellationToken)
        {
            var targetData = configProvider.CollectablesConfig.Get(targetType);

            try
            {
                await UniTask.WaitForSeconds(0.5f);
                tooltip = await tooltipService.CreateCustomTooltipAsync<EatTutorialTooltip>(TOOLTIP_ASSET_ID, mainUIProvider.TooltipRoot, cancellationToken, false, true);
                tooltip.Construct(targetData, spriteAtlasService);
                tooltip.Initialize();
                tooltip.Show(configProvider.CustomTooltipConfiguration.GetData(TOOLTIP_ASSET_ID));
                taskCompleteTutorialEvent.Subscribe(CheckComplete).AddTo(disposables);
            }
            catch (OperationCanceledException e)
            {
                Debug.LogWarning($"[CODE] Operation was cancelled: {e.Message}");                
            }            
        }

        private void CheckComplete(CollectableType collectableType) 
        {
            if (collectableType != targetType) 
                return;
            
            tooltip.OnClose += Complete;
            tooltip.Close();
        }


        private void Complete()
        {
            if (tooltip != null)
            {
                tooltip.OnClose -= Complete;
                tooltip.Dispose();
            }
            OnComplete?.Invoke(TutorialId);
            disposables.Dispose();
        }

        public override void Stop()
        {
            if (tooltip != null)
            {
                tooltip.OnClose -= Complete;
                tooltip.Dispose();
                disposables.Dispose();
            }
        }
    }
}