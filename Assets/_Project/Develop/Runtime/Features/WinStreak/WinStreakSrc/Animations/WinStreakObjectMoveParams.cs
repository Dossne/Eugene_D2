using System;
using UnityEngine;

namespace Features.WinStreak
{
    [Serializable]
    public class WinStreakObjectMoveParams
    {
        [Header("From left")]
        public AnimationCurve moveFromLeftCurve;
        public float moveFromLeftDuration;

        [Header("To right")]
        public AnimationCurve moveToRightCurve;
        public float moveToRightDuration;

    }
}