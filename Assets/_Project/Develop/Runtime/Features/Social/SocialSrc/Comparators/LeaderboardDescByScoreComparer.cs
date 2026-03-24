using System.Collections.Generic;
using Features.Social;

namespace Features.Competition
{
    //Order by descending non alloc way
    public sealed class LeaderboardDescByScoreComparer : IComparer<LeaderboardRecord>
    {
        public static readonly LeaderboardDescByScoreComparer Instance = new();

        public int Compare(LeaderboardRecord a, LeaderboardRecord b)
        {
            int result = b.score.CompareTo(a.score);
            if (result == 0)
            {
                result = b.subScore.CompareTo(a.subScore);
            }

            return result;
        }
    }
}