using System;

using Infrastructure.PurchaseSystem;

namespace Features.LevelLoose
{
    [Serializable]
    public class ResurrectOfferData
    {
        public OfferId offerId;
        public int order;
    }
}