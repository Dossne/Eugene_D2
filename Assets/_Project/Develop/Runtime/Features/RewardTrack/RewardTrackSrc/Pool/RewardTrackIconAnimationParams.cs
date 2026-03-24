using System;
using Features.WinStreak;
using TriInspector;
using UnityEngine;

namespace Features.RewardTrack
{
    [Serializable]
    public class RewardTrackIconAnimationParams
    {
        [Title("Root show scale")]
        public float showScaleDuration = 0.25f;
        public AnimationCurve showScaleCurve = AnimationCurve.Linear(0, 1, 1, 1);
        
        [Title("Win Streak")]
        public WinStreakLabelAnimation  winStreakLabelAnimation;
        
        [Title("On Fly")]
        public float fadeTextDuration = 0.3f;
        public AnimationCurve fadeTextCurve = AnimationCurve.Linear(0, 1, 1, 0);
    }
}