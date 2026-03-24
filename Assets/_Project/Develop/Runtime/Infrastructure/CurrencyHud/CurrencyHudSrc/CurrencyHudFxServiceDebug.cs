/*
using Infrastructure.WalletSystem;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Utilities;
using UnityEngine;
using VContainer;


namespace Infrastructure.CurrencyHud
{
    public class CurrencyHudFxServiceDebug : MonoBehaviour, IDisposable
    {
        [TriInspector.Title("Debug test")]
        [SerializeField] private CurrencyType debugCurrency = CurrencyType.Coins;
        [SerializeField] private int debugAmount = 120;
        [SerializeField] private int debugWalletState = 2000;

        private CurrencyHudService currencyHudService;
        private CurrencyHudFxService currencyHudFxService;
        private CancellationTokenSource debugCts = new CancellationTokenSource();

        [Inject]
        public void Construct(
            CurrencyHudService currencyHudService,
            CurrencyHudFxService currencyHudFxService
            )
        {
            this.currencyHudService = currencyHudService;
            this.currencyHudFxService = currencyHudFxService;
        }


        void IDisposable.Dispose()
        {
            debugCts.Cancel();
            debugCts.Dispose();
        }


#if UNITY_EDITOR
        [TriInspector.Button]
        private void TestFly()
        {
            currencyHudService.Refresh(debugCurrency, debugWalletState, false, ActionType.Set);
            currencyHudFxService.Schedule(debugCurrency, debugAmount, debugWalletState);
            currencyHudFxService.ExecuteScheduledAsync(debugCts.Token).Forget();
        }
#endif
    }
}
*/


