using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Features.LevelLoose
{
    [CreateAssetMenu(fileName = "LostItemSettings", menuName = "Config/Game/LostItemSettings")]
    public class LostItemSettings : ScriptableObject
    {
        [SerializeField] private SerializedDictionary<LostItemType, LostItemSettingData> settings = new();

        public LostItemSettingData GetLostItemSettingData(LostItemType lostItemType)
        {
            if (settings.ContainsKey(lostItemType))
                return settings[lostItemType];
            return null;
        }
    }
}