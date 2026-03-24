using Newtonsoft.Json;
using System;


namespace Features.FreePaidOffer
{
    [Serializable]
    public class FreePaidOfferState
    {
        [JsonProperty("fpc")] public int offerCounter = 1;
        [JsonProperty("fpp")] public int presetId = 0;
        [JsonProperty("fpo")] public int offerIdx = 0;
        [JsonProperty("fps")] public FreePaidOfferSkinType freePaidOfferSkinType = FreePaidOfferSkinType.SkinType_1;
    }
}