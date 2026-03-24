using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Infrastructure.PurchaseSystem
{
    [Serializable]
    public class PurchaseState
    {
        public float cumulativeSpendedUsd = 0;
        public List<InAppInProgress> inAppsInProgress = new();        
        public List<PurchasedInAppData> purchasedInAppDataList = new();
        [JsonProperty("lod")] public List<LimitedOfferData> limitedOffers = new();
    }
}