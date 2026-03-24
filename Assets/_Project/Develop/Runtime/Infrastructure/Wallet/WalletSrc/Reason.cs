namespace Infrastructure.WalletSystem
{
    public static class Reason
    {
        public static class In //income
        {
            public const string LevelComplete = "level_complete";
            public const string LevelCompleteMulti = "level_complete_multiply";
            public const string Cheat = "cheat";
            public const string LifeInByTime = "health_income_by_time";
            public const string LifeInByCoin = "health_income_by_coin";
            public const string LifeInByRewardAd = "health_income_by_reward_ad";
        }
        
        public static class Out //outcome
        {
            public const string Resurrect = "resurrect";
            public const string BoosterBuy = "booster_buy";
            public const string Cheat = "cheat";
            public const string LifeOut = "health_outcome";
            public const string LifeBuy = "health_buy";
        }
    }
}