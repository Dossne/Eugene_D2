using System;
using TriInspector;
using UnityEngine;

namespace Features.RewardTrack
{

    [Serializable]
    public class RewardTrackProgressHudAnimationParams
    {
        [Title("Slider")]
        public ProgressSliderAnimationParams sliderPartialMove;
        [Space]
        public ProgressSliderAnimationParams sliderFullMove;

        [Title("Reward icon hide")]
        public AnimationCurve rewardIconScaleDownCurve;
        public float rewardIconScaleDownDuration = 0.3f;
        [Space]
        public AnimationCurve rewardIconScaleUpCurve;
        public float rewardIconScaleUpDuration = 0.3f;
        [Space]
        public float rewardIconDurationTotal = 1f;

    }
}