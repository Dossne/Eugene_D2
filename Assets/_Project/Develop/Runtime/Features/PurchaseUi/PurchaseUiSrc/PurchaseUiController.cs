using Infrastructure.InputControl;
using Infrastructure.PurchaseSystem;
using VContainer;

namespace Features.PurchaseUi
{
    public class PurchaseUiController
    {
        private readonly IPurchaseManager purchaseManager;
        private InputUiService inputUiService;

        [Inject]
        public PurchaseUiController(IPurchaseManager purchaseManager,
                                    InputUiService   inputUiService)
        {
            this.purchaseManager = purchaseManager;
            this.inputUiService  = inputUiService;
        }

        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (IsInitialized)
                return;

            purchaseManager.OnPurchaseLoadingEvent += PurchaseManager_OnPurchaseLoadingEvent;
            purchaseManager.OnPurchaseFailed       += PurchaseManager_OnPurchaseFailed;
            IsInitialized = true;
        }

        public void Deinitialize()
        {
            if (!IsInitialized)
                return;

            purchaseManager.OnPurchaseLoadingEvent -= PurchaseManager_OnPurchaseLoadingEvent;
            purchaseManager.OnPurchaseFailed       -= PurchaseManager_OnPurchaseFailed;

            IsInitialized = false;
        }

        private void PurchaseManager_OnPurchaseLoadingEvent((bool isLoading, string message, float maxTime) loadingData)
        {
#if UNITY_IPHONE || UNITY_IOS
            if(loadingData.isLoading)
            {
                inputUiService.DisableInput();
            }
            else
            {
                inputUiService.EnableInput();
            }
#endif
        }

        private void PurchaseManager_OnPurchaseFailed()
        {
#if UNITY_IPHONE || UNITY_IOS
            inputUiService.EnableInput();
#endif
        }
        
    }
}