using Cysharp.Threading.Tasks;
using Infrastructure.Configs;
using Infrastructure.CurrencyHud;
using Infrastructure.Popups;
using Infrastructure.Utilities;
using Infrastructure.WalletSystem;
using System.Collections.Generic;
using System.Threading;

namespace Features.CurrencyRewardDisplay
{
    public class CurrencyRewardDisplayService
    {
        private readonly PopupService popupService;
        private readonly CurrencyHudFxService currencyHudFxService;
        private readonly CurrencyConfig currencyConfig;
        private readonly List<(CurrencyType currency, int amount, int startValue)> scheduled = new();
        private readonly CancellationTokenSource cts;
        private CurrencyRewardDisplayPopup currencyRewardDisplayPopup;
        private bool isInit;



        public CurrencyRewardDisplayService(
            PopupService popupService,
            CurrencyHudFxService currencyHudFxService,
            ConfigProvider configProvider)
        {
            this.popupService = popupService;
            this.currencyHudFxService = currencyHudFxService;
            this.currencyConfig = configProvider.CurrencyConfig;

            this.cts = new CancellationTokenSource();
        }


        public void Initialize()
        {
            if (isInit)
                return;

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            scheduled.Clear();
            cts.Cancel();
            cts.Dispose();

            isInit = false;
        }


        public void Schedule(CurrencyType currency, int amount, int startValue)
        {
            scheduled.Add((currency, amount, startValue));
        }


        public async UniTaskVoid ExecuteScheduledAsync(CancellationToken cancellationToken)
        {
            if(scheduled.IsNullOrEmpty())
            {
                return;
            }

            var scheduledCopy = new List<(CurrencyType currency, int amount, int startValue)>(scheduled);
            scheduled.Clear();

            await OpenCurrencyRewardDisplayPopupAsync(scheduledCopy);
            await UniTask.WaitUntil(() => currencyRewardDisplayPopup == null || !currencyRewardDisplayPopup.IsOpened);
            FlyCurrency(scheduledCopy);
        }


        private async UniTask<bool> OpenCurrencyRewardDisplayPopupAsync(List<(CurrencyType currency, int amount, int startValue)> currencyRewards)
        {
            currencyRewardDisplayPopup = await popupService.GetAsync<CurrencyRewardDisplayPopup>(cts.Token, true);
            currencyRewardDisplayPopup.Construct(currencyRewards, currencyConfig);
            currencyRewardDisplayPopup.Initialize();
            currencyRewardDisplayPopup.Open();
            return true;
        }


        private void FlyCurrency(List<(CurrencyType currency, int amount, int startValue)> currencyRewards)
        {
            foreach(var item in currencyRewards)
            {
                currencyHudFxService.Schedule(item.currency, item.amount, item.startValue);
            }

            currencyHudFxService.ExecuteScheduledAsync(cts.Token).Forget();
        }
    }
}

