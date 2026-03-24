using System.Collections.Generic;
using Infrastructure.SystemsLifeCycle;
using Lofelt.NiceVibrations;
using UnityEngine;

namespace Infrastructure.HapticControl
{
    public class HapticService : Singleton<HapticService>, ISystemTickable
    {
        private HapticsCustomPresetsConfig customPresetsConfig;
        private readonly Dictionary<HapticType, BakedHapticData> cachedCustom = new();
        private bool isHapticOn = true;
        private float cdCurrent;
        private float cd = 0.07f;


        public void Construct(HapticsCustomPresetsConfig customPresetsConfig)
        {
            this.customPresetsConfig = customPresetsConfig;
        }


        public void Initialize()
        {
            if (!isHapticOn)
                return;

            HapticController.Init();
            HapticPatternsCustom.Init();
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.None); //cctor call for prepare default presets
        }


        public void Deinitialize()
        {
            cachedCustom.Clear();
        }


        void ISystemTickable.Tick()
        {
            if (cdCurrent <= 0)
                return;

            cdCurrent -= Time.unscaledDeltaTime;
        }


        public void SetHapticEnabled(bool value)
        {
            isHapticOn = value;
        }


        public void HapticSelection()
        {
            Haptic(HapticType.Selection);
        }


        public void HapticLight()
        {
            Haptic(HapticType.LightImpact);
        }


        public void Haptic(HapticType type)
        {
            if (!isHapticOn || type == HapticType.None)
                return;

            if (cdCurrent > 0)
                return;

            cdCurrent = cd;

            if (IsDefault(in type))
            {
                HapticPatterns.PlayPreset(type.ToPreset());
            }
            else
            {
                PlayCustom(type);
            }
        }


        public static bool IsDefault(in HapticType type)
        {
            return (int)type <= 999;
        }


        private void PlayCustom(HapticType customType)
        {
            if (cachedCustom.TryGetValue(customType, out BakedHapticData data))
            {
                PlayBakedData(data);
                return;
            }
            
            if (!customPresetsConfig.TryGet(customType, out CustomPresetData configData))
                return;

            data = ConvertConfigToBaked(configData);
            cachedCustom.Add(customType, data);
            PlayBakedData(data);
        }


        private BakedHapticData ConvertConfigToBaked(CustomPresetData configData)
        {
            var result = new BakedHapticData();

            if (configData.isClip)
            {
                result.isClip = true;
                result.bytes = configData.clip.json;
                result.gamepadRumble = configData.clip.gamepadRumble;
                return result;
            }

            var con = HapticPatternsCustom.PrepareConstant(configData.amplitude, configData.frequency, configData.duration);

            result.bytes = con.bytes;
            result.gamepadRumble = con.gamepadRumble;
            result.amplitude = con.clampedAmplitude;
            result.frequency = con.clampedFrequency;
            return result;
        }

        
        private void PlayBakedData(BakedHapticData data)
        {
            HapticController.Load(data.bytes, data.gamepadRumble);
            
            if (!data.isClip)
            {
                HapticController.Loop(false);
                HapticController.clipLevel = data.amplitude;
                HapticController.clipFrequencyShift = data.frequency;
            }

            HapticController.Play();
        }


#if PR_CHEAT || UNITY_EDITOR
        //GC.Alloc each call by prepare data (bytes, rumble). Use it only for testing
        public void CheatHapticNoBake(float amplitude, float duration)
        {
            if (!isHapticOn)
                return;

            if (cdCurrent > 0)
                return;

            cdCurrent = cd;

            HapticPatterns.PlayConstant(amplitude, 1, duration);
        }
#endif

    }
}