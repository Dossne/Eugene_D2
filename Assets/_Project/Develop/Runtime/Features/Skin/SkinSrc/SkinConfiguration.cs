using AYellowpaper.SerializedCollections;
using Features.FreePaidOffer;
using System.Linq;
using UnityEngine;

namespace Features.Skin
{
    [CreateAssetMenu(fileName = "SkinConfiguration", menuName = "Config/Game/SkinConfiguration")]
    public class SkinConfiguration : ScriptableObject
    {
        [SerializeField] private SerializedDictionary<SuperDiscountSkinType, SuperDiscountSkinData> superDiscountSkinData;
        //Skins data for other features can be added here

        public SuperDiscountSkinData GetSuperDiscountSkin(SuperDiscountSkinType superDiscountSkinType) 
        {
            if (superDiscountSkinData.TryGetValue(superDiscountSkinType, out var data))
                return data;

            Debug.Log($"{superDiscountSkinType} skin not found!");
            return superDiscountSkinData.First().Value;
        }
    }
}