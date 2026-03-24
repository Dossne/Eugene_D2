using System.Collections.Generic;
using Infrastructure.HapticControl;
using UnityEngine;


namespace Features.FreePaidOffer
{
    [CreateAssetMenu(fileName = "FreePaidOfferAnimationConfig", menuName = "Config/Game/FreePaidOfferAnimationConfig")]
    public class FreePaidOfferAnimationConfig : ScriptableObject
    {
        [Header("Reward")]
        public float rewardFirstFloatDelay = 0.3f;
        public float rewardNextFloatDelay = 0.3f;
        [FixEnumNames] public HapticType rewardFloatHaptic = HapticType.Selection;

        [Header("Current Slot")]
        public float currentSlotScaleDelayAfterAllRewards = 0.4f;
        public float currentSlotScaleDuration = 1.0f;
        public AnimationCurve currentSlotScaleCurve = AnimationCurve.Linear(0, 1, 1, 0);

        [Header("Other Slots")]
        public float otherSlotsMovementDuration = 0.5f;
        public List<float> otherSlotsMovementDelayAfterAllRewards;
        public AnimationCurve otherSlotsMovementEaseCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header("Next Slot")]
        public float nextSlotFadeBkgDelayAfterAllRewards = 0.5f;
        public float nextSlotFadeBkgDuration = 1.0f;
        public AnimationCurve nextSlotFadeBkgEaseCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header("Last Slot")]
        public float lastSlotScaleDuration = 1.0f;
        public AnimationCurve lastSlotScaleScaleCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header("Button")]
        public float buttonScaleDelayFromStart = 0.0f;
        public float buttonScaleDuration = 1f;
        public AnimationCurve buttonScaleCurve = AnimationCurve.Linear(0, 1, 1, 0);

        [Header("Tick")]
        public float tickScaleDelayFromStart = 0.0f;
        public float tickScaleDuration = 1f;
        public AnimationCurve tickScaleCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header("Lock")]
        public float lockDelayAfterAllRewards = 0.5f;
        public Vector3 lockFinishPosShiftedFromStart;
        [Space]
        public float lockRotationOnClickDuration = 0.15f;
        public float lockRotationOnClickMaxZAngle = 15.0f;
        public AnimationCurve lockRotationOnClickCurve = AnimationCurve.Linear(0, 0, 1, 1);
    }
}


