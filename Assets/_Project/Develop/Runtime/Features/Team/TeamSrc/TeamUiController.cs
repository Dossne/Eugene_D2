using Cysharp.Threading.Tasks;
using Infrastructure.Popups;
using System.Threading;
using VContainer;


namespace Features.Team
{
    public class TeamUiController
    {
        private PopupService popupService;
        private TeamScreen teamScreen;

        [Inject]
        public TeamUiController(PopupService popupService) 
        {
            this.popupService = popupService;   
        }

        public bool IsInitialized { get; private set; }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized)
                return;

            await InitializeShopScreen(cancellationToken);

            IsInitialized = true;
        }

        public void Deinitialize()
        {
            if (!IsInitialized)
                return;

            DeinitializeShopScreen();

            IsInitialized = false;
        }

        public void ShowTeamPopup()
        {
            if (teamScreen != null)
                teamScreen.Open();
        }

        public void CloseTeamPopup()
        {         
            if (teamScreen != null)
                teamScreen.Close();
        }

        private async UniTask InitializeShopScreen(CancellationToken cancellationToken)
        {
            teamScreen = await popupService.GetAsync<TeamScreen>(cancellationToken, true, popupMode: PopupService.PopupMode.Screen);
            teamScreen.Initialize();
        }

        private void DeinitializeShopScreen()
        {
            teamScreen.Deinitialize();
        }
    }
}