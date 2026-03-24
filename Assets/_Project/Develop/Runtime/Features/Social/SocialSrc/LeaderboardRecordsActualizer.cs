using System.Collections.Generic;
using Features.Competition;
using Features.PlayerProfile;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Features.Social
{
    public class RecordsGeneratorContext
    {
        //statistics
        private int firstWinCountMin = 31;
        private int firstWinCountMax = 300;
        private int longestWinStreakMin = 30;
        private int longestWinStreakMax = 100;
        private int totalWinCountMin = 70;
        private int totalWinCountMax = 550;

        //scoring
        private int randomScoreMin = 5;
        private int randomScoreMax = 1000;
        private float randomScorePercMin = 0.5f;
        private float randomScorePercMax = 0.95f;

        public (int firstWinCount, int maxWinStreak, int totalWinCount) GetStatistics()
        {
            var totalWinCount = Random.Range(totalWinCountMin, totalWinCountMax + 1);

            var firstWinCount = Random.Range(firstWinCountMin, firstWinCountMax);
            firstWinCount = Mathf.Clamp(firstWinCount, firstWinCount, totalWinCount - 1);

            var maxWinStreak = Random.Range(longestWinStreakMin, longestWinStreakMax);
            maxWinStreak = Mathf.Clamp(maxWinStreak, maxWinStreak, firstWinCount - 1);

            return (firstWinCount, maxWinStreak, totalWinCount);
        }

        public long GetRandomScoreAdditive(long baseVal)
        {
            return baseVal + Random.Range(randomScoreMin, randomScoreMax + 1);
        }

        public long GetRandomScorePerc(long baseVal)
        {
            float randomMultiplier = Random.Range(randomScorePercMin, randomScorePercMax);
            return (long)(baseVal * randomMultiplier);
        }
    }

    /// <summary>
    /// Actualize cached records to actual state of server prize (finish leaderboard logic)
    /// </summary>
    public class LeaderboardRecordsActualizer
    {
        private readonly PlayerProfileConfiguration playerProfileConfig;
        private readonly RecordsGeneratorContext generator = new();

        public LeaderboardRecordsActualizer(PlayerProfileConfiguration playerProfileConfig)
        {
            this.playerProfileConfig = playerProfileConfig;
        }

        public (int newPlayerPosIdx, List<LeaderboardRecord> records) GetActualizedRecords(List<LeaderboardRecord> records,
                                                                                           int maxLeaderboardCount,
                                                                                           int serverRewardMaxCount,
                                                                                           int serverRewardIdx = -1)
        {
            var playerPosIdx = GetPlayerPosIdx(records);
            var serverRewardMaxIdx = serverRewardMaxCount > 0 ? serverRewardMaxCount - 1 : 0;

            //Records are in actual state, do nothing
            if (playerPosIdx == serverRewardIdx || serverRewardIdx < 0 && playerPosIdx > serverRewardMaxIdx)
                return (playerPosIdx, records);

            int newPlayerPosIdx = serverRewardIdx >= 0
                ? PrepareDataWithServerPrize(records, serverRewardIdx)
                : PrepareDataWithoutServerPrize(records, maxLeaderboardCount, serverRewardMaxIdx);

            Swap(records, playerPosIdx, newPlayerPosIdx);

            CalculateFakeScore(records, newPlayerPosIdx);

            records.Sort(LeaderboardDescByScoreComparer.Instance);

            return (newPlayerPosIdx, records);
        }

        public static void Swap(List<LeaderboardRecord> records, int removeIdx, int insertIntoIdx)
        {
            if (records.Count <= 1 || records.Count <= insertIntoIdx)
                return;

            var item = records[removeIdx];
            records.RemoveAt(removeIdx);
            records.Insert(insertIntoIdx, item);
        }

        //The position is known from the server, but it does not match the records.
        private int PrepareDataWithServerPrize(List<LeaderboardRecord> records, int serverRewardIdx)
        {
            var maxRecordIdx = records.Count - 1;

            if (maxRecordIdx < serverRewardIdx)
            {
                AddFakeRecords(records, serverRewardIdx - maxRecordIdx);
            }

            return serverRewardIdx;
        }

        //The reward was not received from the server (the player did not take a prize place), but the player's position in records is less/equals than serverRewardMaxIdx.
        private int PrepareDataWithoutServerPrize(List<LeaderboardRecord> records, int maxLeaderboardCount, int serverRewardMaxIdx)
        {
            var maxRecordIdx = records.Count - 1;
            var targetPlayerPosIdx = Random.Range(serverRewardMaxIdx + 1, maxLeaderboardCount); //non prize place

            if (maxRecordIdx < targetPlayerPosIdx)
            {
                AddFakeRecords(records, targetPlayerPosIdx - maxRecordIdx);
            }

            return targetPlayerPosIdx;
        }

        private void CalculateFakeScore(List<LeaderboardRecord> records, int playerIdx)
        {
            long playerScore = records[playerIdx].score;
            for (var i = 0; i < records.Count; i++)
            {
                var record = records[i];

                if (i < playerIdx)
                {
                    record.score = generator.GetRandomScoreAdditive(playerScore);
                }
                else if (i > playerIdx)
                {
                    record.score = generator.GetRandomScorePerc(playerScore);
                }
            }
        }

        private void AddFakeRecords(List<LeaderboardRecord> records, int createCount)
        {
            for (var i = 0; i < createCount; i++)
            {
                var item = CreateFakeLeaderboardRecord();
                records.Add(item);
            }
        }

        private LeaderboardRecord CreateFakeLeaderboardRecord()
        {
            var avatarIdx = Random.Range(0, playerProfileConfig.Avatars.Count);

            (int firstWinCount, int maxWinStreak, int totalWinCount) fakeStats = generator.GetStatistics();

            var metaData = new PlayerMetaData
            {
                playerAvatar = new PlayerProfileAvatarDto
                {
                    avatarId = playerProfileConfig.Avatars[avatarIdx].avatarId,
                },

                playerStatistics = new PlayerStatistics
                {
                    firstWinCount = fakeStats.firstWinCount,
                    maxWinStreak = fakeStats.maxWinStreak,
                    totalWinCount = fakeStats.totalWinCount,
                }
            };

            var item = new LeaderboardRecord
            {
                displayName = GenerateDefaultName(),
                metaData = metaData,
                score = 0,
                subScore = 0,
                isPlayer = 0
            };
            return item;
        }

        private string GenerateDefaultName()
        {
            return $"{playerProfileConfig.Feature.defaultName}{Random.Range(0, 1000000)}";
        }

        private int GetPlayerPosIdx(List<LeaderboardRecord> records)
        {
            int playerPosIdx = 0;
            for (var i = 0; i < records.Count; i++)
            {
                if (records[i].IsPlayer())
                {
                    playerPosIdx = i;
                    break;
                }
            }

            return playerPosIdx;
        }
    }
}