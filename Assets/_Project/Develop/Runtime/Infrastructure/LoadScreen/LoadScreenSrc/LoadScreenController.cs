using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.MainUICanvasControl;
using Infrastructure.PauseControl;
using Infrastructure.SystemModules;

namespace Infrastructure.LoadScreen
{
    public class LoadScreenController
    {
        private readonly LoadScreenUI loadScreenUI;
        private readonly PauseService pauseService;
        private readonly SdkService sdkService;


        public LoadScreenController(MainUIProvider uiProvider, PauseService pauseService, SdkService sdkService)
        {
            this.loadScreenUI = uiProvider.LoadScreenUI;
            this.pauseService = pauseService;
            this.sdkService = sdkService;
            loadScreenUI.Construct(CallSupport);
        }

        public void Initialize()
        {
            loadScreenUI.Initialize();
        }


        public void Open(LoadScreenUI.Reason reason, bool showSlider)
        {
            pauseService.Pause(this);
            loadScreenUI.ShowProgressSlider(showSlider);
            loadScreenUI.Open(reason);
        }


        public UniTask OpenAsync(LoadScreenUI.Reason reason, bool showSlider, CancellationToken cancellationToken)
        {
            if (loadScreenUI.IsOpened)
                return UniTask.CompletedTask;

            pauseService.Pause(this);
            loadScreenUI.ShowProgressSlider(showSlider);
            return loadScreenUI.OpenAsync(reason, cancellationToken);
        }


        public void Close(LoadScreenUI.Reason reason)
        {
            pauseService.Resume(this);
            loadScreenUI.Close(reason);
        }

        
        /// <summary>
        /// Temp implementation of fake load
        /// </summary>
        public async UniTask PlayProgressToStageAsync(float time, int stage, CancellationToken cancellationToken, bool fake = false)
        {
            loadScreenUI.ShowProgressSlider(true);
            loadScreenUI.SetProgressPercent(stage, fake, time);
            await UniTask.WaitForSeconds(time, true, cancellationToken: cancellationToken);
        }

        private void CallSupport()
        {
            sdkService.OpenSupportPage();
        }
    }
}