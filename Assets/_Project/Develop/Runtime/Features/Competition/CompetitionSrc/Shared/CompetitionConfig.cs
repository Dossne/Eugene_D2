using System;
using System.Collections.Generic;
using Infrastructure.Configuration;
using Newtonsoft.Json;
using UnityEngine;

namespace Features.Competition
{
    [Serializable]
    public class CompetitionFeatureData
    {
        [Header("Main")]
        [JsonProperty("en")] public bool isEnabled;
        [JsonProperty("swv")] public int showWidgetLevel;
        [JsonProperty("lv")] public int unlockLevel;
        [JsonProperty("count")] public int giveCountPerLevel;
        [JsonProperty("enMulti")] public bool isMultiplierEnabled;
        [JsonProperty("lname")] public string leaderboardName;
        [JsonProperty("lcount")] public int leaderboardCount;
        [JsonProperty("cDelay")] public float connectToNewLeaderboardDelaySec = 3f;
        [JsonProperty("debug")] public bool isDebugLog;
        
        [Header("Internet connect check")]
        [JsonProperty("icCheck")] public bool isCheckInternetConnect = true;
        [JsonProperty("icTimeout")] public int connectCheckTimeoutMs = 2000;
        [JsonProperty("icUrl")] public string connectCheckUrl = "https://clients3.google.com/generate_204";
    }

    [Serializable]
    public class CompetitionMultiplierData
    {
        [JsonProperty("val")] public int value;
    }

    [Serializable]
    public class CompetitionRewardTrackData
    {
        [JsonProperty("sn")] public int stepNumber;
        [JsonProperty("cc")] public int collectCount;
        [JsonProperty("rj"), TextArea(1, 15)] public string rewardJson;
    }

    [Serializable]
    public class CompetitionServerRewardData
    {
        [JsonProperty("rj"), TextArea(1, 15)] public string rewardJson;
    }

    [Serializable]
    public class CompetitionDataConfig
    {
        public List<CompetitionFeatureData> featureData;
        public List<CompetitionMultiplierData> multiplierData;
        public List<CompetitionRewardTrackData> rewardTracks;
        public List<CompetitionServerRewardData> serverRewards;
    }

    [CreateAssetMenu(fileName = "CompetitionConfig", menuName = "Config/Game/CompetitionConfig")]
    public class CompetitionConfig : JsonConvertableConfig
    {
        [SerializeField] private CompetitionDataConfig configData;

        public CompetitionFeatureData Feature => configData.featureData[0];
        public List<CompetitionRewardTrackData> RewardTracks => configData.rewardTracks;
        public List<CompetitionServerRewardData> ServerRewards => configData.serverRewards;
        public List<CompetitionMultiplierData> Multipliers => configData.multiplierData;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "CompetitionConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (CompetitionDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(CompetitionDataConfig);

#if UNITY_EDITOR
        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.featureData, nameof(Feature), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.multiplierData, nameof(Multipliers), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.rewardTracks, nameof(RewardTracks), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.serverRewards, nameof(ServerRewards), ref data);

            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }

        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.featureData = GoogleDocsUtils.SnatchDataRowsToConfig<CompetitionFeatureData>(tableData);
            configData.multiplierData = GoogleDocsUtils.SnatchDataRowsToConfig<CompetitionMultiplierData>(tableData);
            configData.rewardTracks = GoogleDocsUtils.SnatchDataRowsToConfig<CompetitionRewardTrackData>(tableData);
            configData.serverRewards = GoogleDocsUtils.SnatchDataRowsToConfig<CompetitionServerRewardData>(tableData);
        }
#endif
    }
}