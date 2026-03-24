using Infrastructure.Pool;


namespace Features.PurchaseUi
{
    public class FloatingPurchaseRewardItemViewPool : ComponentPool<FloatingPurchaseRewardItemViewType, PoolableFloatingPurchaseRewardItemView>
    {
        protected override void OnBeforeInitialize()
        {
            SetItemsRoot(FloatingPurchaseRewardItemViewType.FreePaidOfferSlot, mainUIProvider.OverPopupRoot);
        }
    }
}

