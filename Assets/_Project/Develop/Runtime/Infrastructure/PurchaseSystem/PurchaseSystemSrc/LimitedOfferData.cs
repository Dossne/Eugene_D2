using System;

namespace Infrastructure.PurchaseSystem
{
    [Serializable]
    public class LimitedOfferData 
    {
        public OfferId offerId;
        public int count;
    }
}