using System;
using System.Collections.Generic;
using Features.Boosters;
using Features.Competition;
using Features.LavaQuest;
using Features.Level;
using Features.RewardTrack;
using Features.WinStreak;
using Infrastructure.WalletSystem;
using Newtonsoft.Json;

namespace Infrastructure.PersistentProgress
{
    [Serializable]
    public class GameState
    {
        public int levelIdx;
        [JsonProperty("lcl")] public bool loseOnCurrentLevel;
        [JsonProperty("sladd")] public int scheduledLevelAdd;
        public List<CurrencyAmountSave> currencies;
        public BoostersState booster = new();
        [JsonProperty("ws")] public WinStreakState winSteak = new();
        [JsonProperty("rt")] public RewardTrackState rewardTrack = new();
        [JsonProperty("lq")] public LavaQuestState lavaQuest = new();
        [JsonProperty("ls")] public LevelStatistics levelStatistics = new();
        [JsonProperty("competition")] public CompetitionState competitionState = new();
    }

}