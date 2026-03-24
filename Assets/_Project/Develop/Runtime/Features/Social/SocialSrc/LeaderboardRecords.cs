using Newtonsoft.Json;
using System;
using System.Collections.Generic;



namespace Features.Social
{
    public class LeaderboardRecords
    {
        [JsonProperty("r")] public List<LeaderboardRecord> records = null;
        [JsonProperty("lg")] public long leaderboardGeneration = -1;
        
        private LeaderboardRecord playerRecord = null;



        public bool IsScoreSent { get; private set; } = false;
        public DateTime NextUpdateDate { get; private set; } = DateTime.MinValue;



        public LeaderboardRecords()
        {
            records = new List<LeaderboardRecord>();
            leaderboardGeneration = -1;
            playerRecord = null;

            IsScoreSent = false;
            NextUpdateDate = DateTime.MinValue;
        }


        public LeaderboardRecords(List<LeaderboardRecord> leaderboardRecords, long recordsGeneration, long unsentScore, string playerId, DateTime nextUpdateDate)
        {
            this.records = leaderboardRecords;
            this.leaderboardGeneration = recordsGeneration;

            IsScoreSent = false;
            NextUpdateDate = nextUpdateDate;
            AddScore(unsentScore);
        }


        public override string ToString()
        {
            string result = string.Empty;
            foreach (LeaderboardRecord record in records)
            {
                result += record.ToString() + "\n";
            }

            result += $"generation: {leaderboardGeneration}";

            return result;
        }


        public void ScoreSent()
        {
            IsScoreSent = true;
        }


        public void AddScore(long unsynchronizedScore)
        {
            CachePlayerRecord();
            playerRecord.score += unsynchronizedScore;
        }
        

        public void UpdatePlayerData(string displayName, PlayerMetaData playerMetaData)
        {
            CachePlayerRecord();
            playerRecord.displayName = displayName;
            playerRecord.metaData = playerMetaData;
        }


        private void CachePlayerRecord()
        {
            if (playerRecord == null)
            {
                for (int i = 0; i < records.Count; ++i)
                {
                    LeaderboardRecord leaderboardRecord = records[i];
                    if (leaderboardRecord.IsPlayer())
                    {
                        playerRecord = leaderboardRecord;
                        break;
                    }
                }
            }
        }
    }
}


