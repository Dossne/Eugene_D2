using System.Runtime.CompilerServices;
using Lofelt.NiceVibrations;

namespace Infrastructure.HapticControl
{
    public static class HapticExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HapticPatterns.PresetType ToPreset(this HapticType type)
        {
            return type switch
            {
                HapticType.Selection    => HapticPatterns.PresetType.Selection,
                HapticType.Success      => HapticPatterns.PresetType.Success,
                HapticType.Warning      => HapticPatterns.PresetType.Warning,
                HapticType.Failure      => HapticPatterns.PresetType.Failure,
                HapticType.LightImpact  => HapticPatterns.PresetType.LightImpact,
                HapticType.MediumImpact => HapticPatterns.PresetType.MediumImpact,
                HapticType.HeavyImpact  => HapticPatterns.PresetType.HeavyImpact,
                HapticType.RigidImpact  => HapticPatterns.PresetType.RigidImpact,
                HapticType.SoftImpact   => HapticPatterns.PresetType.SoftImpact,
                _                       => HapticPatterns.PresetType.None
            };
        }
    }
}