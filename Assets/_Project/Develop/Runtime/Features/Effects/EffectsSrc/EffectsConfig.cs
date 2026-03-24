using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Features.CameraFollow;
using Infrastructure.HapticControl;
using UnityEngine;

namespace Features.Effects
{

    [Serializable]
    public class TimerSliderFxData
    {
        public Color defaultColor;
        public SerializedDictionary<int /*timeProgress*/, Color> colorsByTime;
        public AnimationCurve curve;
        public float duration;
    }

    [Serializable]
    public class TimerTextFxData
    {
        public int beginTime;
        public Color defaultColor;
        public Color lowColor;
        public AnimationCurve curve;
        public float duration;
    }

    [Serializable]
    public class TimerTextBoostFxData
    {
        [Header("Text")]
        public Color boostColor;
        public float textDurationSec;

        [Header("Scale hud")]
        public AnimationCurve scaleCurve;
        public float hudScale;
        public float scaleDurationSec;
    }

    [Serializable]
    public class TimerFx
    {
        public int value;
        public bool isContinuous;
    }

    [Serializable]
    public class VignetteAnimFxData
    {
        public float minAlpha;
        public float maxAlpha;
        public AnimationCurve curve;
        public float duration;
    }

    [CreateAssetMenu(fileName = "EffectsConfig", menuName = "Config/Game/EffectsConfig")]
    public class EffectsConfig : ScriptableObject
    {
        [TriInspector.Title("Hud timer")]
        public TimerSliderFxData sliderFx;
        public TimerTextFxData textFx;

        [TriInspector.Title("Low time")]
        public List<TimerFx> lowTimesFx;
        public VignetteAnimFxData lowTimeVignetteFx;

        [TriInspector.Title("Pre Booster Bonus Clock")]
        public TimerTextBoostFxData preBoosterBonusClockFx;

        [TriInspector.Title("Freeze booster")]
        public VignetteAnimFxData freezeTimeVignetteFx;
        public AnimationCurve hudFreezeCurve;
        public float hudFreezeDuration;
        public float hudShakeDuration;
        public float hudShakeStrength;
        public int hudShakeVibrato;
        public AnimationCurve sliderFreezeCurve;
        public float sliderFreezeDuration;
        public Color sliderFreezeColor;
        
        [TriInspector.Title("Death")]
        public HapticType deathHaptics = HapticType.HeavyImpact;
        public CameraShakeFxData deathCamShake;
        public VignetteAnimFxData deathVignetteFx;
    }
}