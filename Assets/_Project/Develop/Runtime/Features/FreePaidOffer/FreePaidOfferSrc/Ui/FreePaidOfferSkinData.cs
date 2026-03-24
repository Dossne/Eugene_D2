using System;
using UnityEngine.AddressableAssets;


namespace Features.FreePaidOffer
{
    [Serializable]
    public class FreePaidOfferSkinData
    {
        public AssetReferenceGameObject widgetRef;
        public string popupHeaderSpriteId = string.Empty;
        public string popupHeaderLocKey = string.Empty;
        public AssetReferenceGameObject offerSlotRef;
    }
}


