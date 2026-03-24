using Infrastructure.WalletSystem;
using Infrastructure.Configs;
using Infrastructure.PersistentProgress;

namespace Infrastructure.WelcomeBonus
{
    public class FirstLaunchHandler : ISavable
    {
        private readonly Wallet wallet;
        private readonly ConfigProvider configProvider;
        private int launchCount = 0;

        public FirstLaunchHandler(Wallet wallet, ConfigProvider configProvider)
        {
            this.wallet = wallet;
            this.configProvider = configProvider;
        }

        public void Initialize() 
        {
            if (launchCount == 0)
            {
                var coinData = configProvider.CurrencyConfig.Items.Find(x => x.currencyType == CurrencyType.Coins);
                wallet.Increase(CurrencyType.Coins, coinData.welcomeBonus, "welcome bonus");
            }
        }


        void ISavable.Load(Progress progress)
        {
            launchCount = progress.appState.launchCount;
        }

        void ISavable.Save(Progress progress)
        {
        }
    }
}