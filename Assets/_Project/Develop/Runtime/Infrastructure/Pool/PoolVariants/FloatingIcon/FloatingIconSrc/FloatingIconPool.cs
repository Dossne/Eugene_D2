
namespace Infrastructure.Pool.FloatingIcon
{
    public class FloatingIconPool : ComponentPool<FloatingIconType, PoolableFloatingIcon>
    {
        protected override void OnBeforeInitialize()
        {
            SetItemsRoot(FloatingIconType.Task, mainUIProvider.CountersRoot);
            SetItemsRoot(FloatingIconType.Currency, mainUIProvider.FlyingCurrencyRoot);
            SetItemsRoot(FloatingIconType.BonusClockBooster, mainUIProvider.FlyingCurrencyRoot);
            SetItemsRoot(FloatingIconType.CollectableCurrency, mainUIProvider.CountersRoot);
            SetItemsRoot(FloatingIconType.FreePaidOfferSlotLock, mainUIProvider.OverPopupRoot);
        }
    }
}