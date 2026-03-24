using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Features.FreePaidOffer
{
    [CreateAssetMenu(fileName = "FreePaidOfferSkinConfiguration", menuName = "Config/Game/FreePaidOfferSkinConfiguration")]
    public class FreePaidOfferSkinConfiguration : ScriptableObject
    {
        [SerializeField] private SerializedDictionary<FreePaidOfferSkinType, FreePaidOfferSkinData> freePaidOfferSkinData;
        //Skins data for other features can be added here

        public FreePaidOfferSkinData GetFreePaidOfferSkin(FreePaidOfferSkinType freePaidOfferSkinType)
        {
            if (freePaidOfferSkinData.TryGetValue(freePaidOfferSkinType, out var data))
                return data;

            Debug.Log($"{freePaidOfferSkinType} skin not found!");
            foreach(var item in freePaidOfferSkinData)
            {
                return item.Value;
            }
            return null;           
        }
    }
}
