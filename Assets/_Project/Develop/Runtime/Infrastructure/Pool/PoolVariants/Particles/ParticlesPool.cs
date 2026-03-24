namespace Infrastructure.Pool
{
    public sealed class ParticlesPool : ComponentPool<PoolableParticleType, PoolableParticleSystem>
    {
        protected override void OnBeforeInitialize()
        {
            SetItemsRoot(PoolableParticleType.CurrencyCountSparks, mainUIProvider.FlyingCurrencyRoot);
            SetItemsRoot(PoolableParticleType.WinStreakSplash, mainUIProvider.OverPopupRoot);
            SetItemsRoot(PoolableParticleType.FreePaidOfferSparks, mainUIProvider.OverPopupRoot);
            SetItemsRoot(PoolableParticleType.RewardTrackHudProgress, mainUIProvider.OverPopupRoot);
        }
    }
}