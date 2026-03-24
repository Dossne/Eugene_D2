using System.Collections.Generic;

namespace Features.Competition
{
    public sealed class RewardTrackDataAscByStepNumComparer : IComparer<CompetitionRewardTrackData>
    {
        public static readonly RewardTrackDataAscByStepNumComparer Instance = new();

        public int Compare(CompetitionRewardTrackData a, CompetitionRewardTrackData b)
        {
            return a.stepNumber.CompareTo(b.stepNumber);
        }
    }
}