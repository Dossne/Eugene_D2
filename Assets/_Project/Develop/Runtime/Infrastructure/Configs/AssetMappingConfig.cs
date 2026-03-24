using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Infrastructure.Configs
{

    [CreateAssetMenu(fileName = "AssetMappingConfig", menuName = "Config/System/AssetMappingConfig")]
    public class AssetMappingConfig : ScriptableObject
    {
        
        [SerializeField] private AssetReferenceGameObject characterViewRef;
        [field: SerializeField] public AssetReferenceGameObject CurrencyUI { get; private set; }
        [field: SerializeField] public AssetReferenceGameObject LifeUI { get; private set; }
        [field: SerializeField] public AssetReferenceGameObject ExtraCollectableSlotUIRef { get; private set; }
    

        public AssetReferenceGameObject GetCharacterView()
        {
            return characterViewRef;
        }
    }
}