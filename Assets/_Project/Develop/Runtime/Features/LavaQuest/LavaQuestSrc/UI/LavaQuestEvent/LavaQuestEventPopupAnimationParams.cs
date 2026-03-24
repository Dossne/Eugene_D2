using System;
using Infrastructure.HapticControl;
using TriInspector;
using UnityEngine;

namespace Features.LavaQuest
{
    [Serializable]
    public class LavaQuestEventPopupAnimationParams
    {
        [Title("Popup")]
        [Header("Top move")]
        public Vector3 topStartOffsetPos;
        public float topMoveDuration;
        public AnimationCurve topMoveCurve;

        [Header("Level txt animations")]
        public float levelTxtScaleDuration;
        public AnimationCurve levelTxScaleCurve;
        public float levelChangeTextDuration;

        [Header("Player txt animations")]
        public float playerScaleDuration;
        public AnimationCurve playerScaleCurve;
        public float playerChangeTextDuration;

        [Title("Icon")]
        [Header("Shared")]
        public float tapToContinueShowDelay;
        public float iconAnimationsDelay;

        [Header("Icon fade in")]
        public AnimationCurve iconFadeInCurve;
        public float iconFadeInDuration;

        [Header("Icon Move")]
        public float enemiesStartMoveDelay;
        public float enemiesBetweenDelayMin;
        public float enemiesBetweenDelayMax;

        [Header("Icon move")]
        public float iconMoveDuration;
        public AnimationCurve iconMoveCurve;

        [Header("Icon fall.Rotation")]
        public float iconRotationZMin;
        public float iconRotationZMax;
        public float iconRotationDuration;
        public AnimationCurve iconFallRotationCurve;

        [Header("Icon fall.Move")]
        public float iconJumpLavaDuration;
        public Vector3 iconFallTargetOffset;
        public float iconFallDuration;
        public AnimationCurve iconFallCurve;

        [Header("Haptics")]
        public HapticType platformLandHaptic = HapticType.Selection;
        public int platformLandHapticCount = 5;
        public HapticAction[] lavaFallHaptics;
    }
}