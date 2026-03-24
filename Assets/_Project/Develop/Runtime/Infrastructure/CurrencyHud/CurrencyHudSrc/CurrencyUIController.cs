using Infrastructure.SpriteAtlasControl;
using Infrastructure.Utilities;
using Infrastructure.WalletSystem;
using R3;
using UnityEngine;
using VContainer;

namespace Infrastructure.CurrencyHud
{
    /// <summary>
    /// Use for local CurrencyUI items in popups
    /// </summary>
    public class CurrencyUIController : MonoBehaviour
    {
        [SerializeField] private CurrencyType currencyType;
        [SerializeField] private CurrencyUI currencyUI;
        
        private Wallet wallet;
        private SpriteAtlasService spriteAtlasService;
        private CompositeDisposable disposable;


        [Inject]
        public void Construct(Wallet wallet, SpriteAtlasService spriteAtlasService)
        {
            this.wallet = wallet;
            this.spriteAtlasService = spriteAtlasService;
        }


        public void Initialize()
        {
            Sprite icon = spriteAtlasService.GetCurrencyIcon(currencyType);
            currencyUI.Construct(icon, wallet.GetCount(currencyType), "", null);
            currencyUI.Initialize();
        }


        public void Deinitialize()
        {
            currencyUI.Deinitialize();
            disposable?.Dispose();
        }


        public void OnActivate()
        {
            disposable = new CompositeDisposable();
            wallet.OnChange.Subscribe(HandleTransaction).AddTo(disposable);

            Refresh(wallet.GetCount(currencyType), false);
        }


        public void OnDeactivate()
        {
            disposable?.Dispose();
        }


        private void Refresh(int value, bool isAnimated)
        {
            currencyUI.RefreshValue(value, isAnimated, ActionType.Set);
        }


        private void HandleTransaction(Transaction tr)
        {
            if (tr.currency == currencyType)
                Refresh(tr.total, true);
        }
    }
}