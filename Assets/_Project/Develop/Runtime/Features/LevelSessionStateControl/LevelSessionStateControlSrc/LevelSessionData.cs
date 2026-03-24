using System;
using System.Collections.Generic;
using Features.Boosters;
using Features.Collectables;
using Features.LevelComplete;
using Features.LevelConfiguration;
using Features.RewardTrack;
using Infrastructure.Utilities;
using Newtonsoft.Json;

namespace Features.LevelSessionStateControl
{
    [Serializable]
    public class LevelSessionData
    {
        //LevelCreateManager
        [JsonProperty("level")] public LevelConfiguration.Level level;

        //LevelCreateManager
        [JsonProperty("levelSkin")] public LevelTableSkin skin;

        //ExtraItemsSpawner
        [JsonProperty("extraItems")] public List<CollectableData> extraItems;

        //CollectableCurrencyService
        [JsonProperty("collectedCurrency")] public Dictionary<CollectableType, (int collected, int target)> collectedCurrency;

        //LevelTaskManager
        [JsonProperty("tasks")] public Dictionary<CollectableType, int> tasks;

        //RewardTrackCollectController
        [JsonProperty("rewardTrack")] public RewardTrackLevelSessionState rewardTrack;

        //WinStreakBonusApplier
        [JsonProperty("winStreak")] public List<CollectableData> winStreakObjects;

        //LevelUpManager
        [JsonProperty("exp")] public int experience;
        [JsonProperty("reported")] public int lastReportedLevel;

        //CharacterManager
        [JsonProperty("pos")] public SerializableVector3 pos;
        [JsonProperty("progress")] public float progress01;

        //TimeManager
        [JsonProperty("currentTime")] public float currentTime;
        [JsonProperty("totalTime")] public float totalTime;

        //LevelLooseManager
        [JsonProperty("loseReason")] public LoseReason loseReason;
        [JsonProperty("loseCounter")] public int loseCounter;

        //BoosterManager
        [JsonProperty("activeBoosters")] public Dictionary<BoosterType, int /*secondsLeft*/> activeBoosters;

        //GameBaseAnalytics
        [JsonProperty("preBoosters")] public List<BoosterType> preBoosters;
        [JsonProperty("inGameBoosters")] public Dictionary<BoosterType, int /*usedCount*/> inGameBoosters;
        [JsonProperty("winStreakBoosters")] public Dictionary<BoosterType, int /*usedCount*/> winStreakBoosters;
    }
}