namespace Infrastructure.Pool.FloatingText
{
    public class FloatingTextPool : ComponentPool<FloatingTextType, PoolableFloatingText>
    {
        protected override void OnBeforeInitialize()
        {
            SetItemsRoot(FloatingTextType.PointsCounter, mainUIProvider.CountersRoot);
            SetItemsRoot(FloatingTextType.LevelUpSize, mainUIProvider.LevelUpRoot);
            SetItemsRoot(FloatingTextType.RewAdsText, mainUIProvider.PopupRoot);
            SetItemsRoot(FloatingTextType.NotEnoughCurrency, mainUIProvider.OverPopupRoot);
            SetItemsRoot(FloatingTextType.BonusClockBooster, mainUIProvider.FlyingCurrencyRoot);
            SetItemsRoot(FloatingTextType.FlyToUiFx, mainUIProvider.FxFlyToUiStartArea);
            SetItemsRoot(FloatingTextType.InfinitePreBooster, mainUIProvider.PopupRoot);
            SetItemsRoot(FloatingTextType.FreePaidOfferSlot, mainUIProvider.PopupRoot);
            SetItemsRoot(FloatingTextType.RedAlert, mainUIProvider.OverPopupRoot);
            SetItemsRoot(FloatingTextType.WhiteAlert, mainUIProvider.OverPopupRoot);
        }
    }
}