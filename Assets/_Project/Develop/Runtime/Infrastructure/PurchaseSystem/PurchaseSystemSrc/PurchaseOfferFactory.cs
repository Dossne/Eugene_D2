using Features.Boosters;
using Features.Life;
using Infrastructure.AssetManagement;
using Infrastructure.WalletSystem;
using VContainer;

namespace Infrastructure.PurchaseSystem
{
    public class PurchaseOfferFactory
    {
        private readonly Instantiator instantiator;

        [Inject]
        public PurchaseOfferFactory(Instantiator instantiator) 
        {
            this.instantiator = instantiator;
        }

        public PurchaseOfferBase CreatePurchaseOffer(InAppData inAppData)
        {
            return instantiator.InstantiateClass<PurchaseOfferBase>(Lifetime.Scoped, inAppData);
        }
    }
}