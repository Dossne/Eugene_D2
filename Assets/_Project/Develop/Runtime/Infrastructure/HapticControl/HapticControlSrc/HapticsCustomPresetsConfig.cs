using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.HapticControl
{
    [CreateAssetMenu(fileName = "HapticsCustomPresetsConfig", menuName = "Config/System/HapticsCustomPresetsConfig")]
    public class HapticsCustomPresetsConfig : ScriptableObject
    {
#if UNITY_EDITOR
        [TriInspector.ValidateInput(nameof(ValidateData)), TriInspector.ListDrawerSettings(ShowElementLabels = true)]
#endif
        [SerializeField] private List<CustomPresetData> customPresets = new();


        public bool TryGet(HapticType type, out CustomPresetData result)
        {
            foreach (var data in customPresets)
            {
                if (data.hapticType == type)
                {
                    result = data;
                    return true;
                }
            }

            result = null;
            return false;
        }


#if UNITY_EDITOR
        public TriInspector.TriValidationResult ValidateData()
        {
            bool hasDuplicate = false;
            HapticType duplicateKey = HapticType.None;

            for (var i = 0; i < customPresets.Count; i++)
            {
                CustomPresetData preset = customPresets[i];

                if (!preset.isClip && preset.clip != null)
                {
                    preset.clip = null;
                }

                if (HapticService.IsDefault(preset.hapticType))
                    return TriInspector.TriValidationResult.Error("[Haptics] ERROR. Do not set default NiceVibrations library type. Choose with id >=1000 for custom presets");


                if (customPresets.FindAll(x => x.hapticType == customPresets[i].hapticType).Count > 1)
                {
                    duplicateKey = customPresets[i].hapticType;
                    hasDuplicate = true;
                    break;
                }
            }

            return hasDuplicate
                ? TriInspector.TriValidationResult.Error($"[Haptics] ERROR. Duplicate key: {duplicateKey}!")
                : TriInspector.TriValidationResult.Valid;
        }
#endif
    }
}