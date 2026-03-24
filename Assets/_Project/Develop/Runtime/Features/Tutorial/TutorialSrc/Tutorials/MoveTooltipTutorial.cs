using Cysharp.Threading.Tasks;
using Features.Level;
using Infrastructure.InputControl;
using Infrastructure.MainUICanvasControl;
using Infrastructure.SceneManagement;
using Infrastructure.TooltipControl;
using System;
using System.Threading;
using UnityEngine;


namespace Features.Tutorial
{
    public class MoveTooltipTutorial : TutorialBase
    {
        private const string TOOLTIP_ASSET_ID = "MoveTutorialTooltip";

        private SceneLoadController sceneLoadController;
        private TooltipService tooltipService;
        private MainUIProvider mainUIProvider;
        private InputService inputService;
        private CustomTooltipConfiguration customTooltipConfiguration;
        private LevelService levelService;
        private MoveTutorialTooltip tooltip;
        private CancellationTokenSource cts;

        public MoveTooltipTutorial(string tutorialId,
                                   SceneLoadController sceneLoadController,
                                   TooltipService tooltipService, 
                                   MainUIProvider mainUIProvider,
                                   InputService inputService,
                                   LevelService levelService,
                                   CustomTooltipConfiguration customTooltipConfiguration) : base(tutorialId)
        {
            this.sceneLoadController = sceneLoadController;
            this.tooltipService = tooltipService;
            this.mainUIProvider = mainUIProvider;
            this.inputService = inputService;
            this.customTooltipConfiguration = customTooltipConfiguration;
            this.levelService = levelService;
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
            cts = new CancellationTokenSource();
            ShowTooltipAsync(cts.Token).Forget();
        }

        private async UniTaskVoid ShowTooltipAsync(CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.WaitForSeconds(0.5f);
                tooltip = await tooltipService.CreateCustomTooltipAsync<MoveTutorialTooltip>(TOOLTIP_ASSET_ID, mainUIProvider.TooltipRoot, cancellationToken, false, true);
                tooltip.Initialize();
                tooltip.Show(customTooltipConfiguration.GetData(TOOLTIP_ASSET_ID));
                await UniTask.WaitUntil(inputService.HasInput, cancellationToken: cancellationToken);                
                await UniTask.WaitForSeconds(0.5f);
            }
            catch (OperationCanceledException e)
            {
                Debug.LogWarning($"[CODE] Operation was cancelled: {e.Message}");
                return;
            }
            
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
        }

        public override void Stop()
        {
            cts.Cancel();
            cts.Dispose();

            if (tooltip != null) 
            {
                tooltip.OnClose -= Complete;
                tooltip.Dispose();
            }
        }
    }
}