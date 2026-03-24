using System.Collections.Generic;
using Features.Social;

namespace Features.Competition
{
    public struct LeaderBoardStateDto
    {
        public List<LeaderboardRecord> records;
        public int prevSavedIdx;
        public int actualIdx;
        public bool inProgressState;
        public bool inFinishedWithPrize;
    }
}