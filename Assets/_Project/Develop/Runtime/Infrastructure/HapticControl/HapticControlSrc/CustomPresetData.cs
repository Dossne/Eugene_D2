using System;
using Lofelt.NiceVibrations;
using UnityEngine;

namespace Infrastructure.HapticControl
{
    [Serializable]
    public class CustomPresetData
    {
        public HapticType hapticType;
        public bool isClip;
        [TriInspector.HideIf(nameof(isClip))] [Range(0f, 1f)] public float amplitude;
        [HideInInspector /*remove if not Android*/] public float frequency = 1f;
        [TriInspector.HideIf(nameof(isClip))] public float duration;
        [TriInspector.ShowIf(nameof(isClip))] public HapticClip clip;
    }
}