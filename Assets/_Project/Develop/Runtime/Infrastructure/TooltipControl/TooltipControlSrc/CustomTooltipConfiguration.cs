using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Infrastructure.TooltipControl
{
    [CreateAssetMenu(fileName = "CustomTooltipConfiguration", menuName = "Config/TooltipControl/CustomTooltipConfiguration")]
    public class CustomTooltipConfiguration : ScriptableObject
    {
        [SerializeField] private SerializedDictionary<string, CustomTooltipData> settings = new();

        public CustomTooltipData GetData(string assetId)
        {
            if (settings.ContainsKey(assetId))
                return settings[assetId];
            return new();
        }
    }
}