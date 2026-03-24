using System;
using System.Collections.Generic;
using Features.Collectables;
using Infrastructure.Configuration;
using Newtonsoft.Json;
using UnityEngine;

namespace Features.RewardTrack
{
    [Serializable]
    public class RewardTrackFeatureData
    {
        [Header("Common")]
        public bool isEnabled;
        public int unlockLevel;
        
        [Header("Item spawn")]
        public int spawnCount;
        public int spawnYPos;
        public float minScale;
        public float maxScale;
    }

    [Serializable]
    public class RewardTrackData
    {
        public int id;
        public DayOfWeek startDay;
        public DayOfWeek endDay;
    }

    [Serializable]
    public class RewardTargetData
    {
        public CollectableType item;
        public string themeImageName;
    }

    [Serializable]
    public class RewardTrackRewardData
    {
        [JsonProperty("id")] public int id;
        [JsonProperty("sn")] public int stepNumber;
        [JsonProperty("cc")] public int collectCount;
        [JsonProperty("rj"), TextArea(1, 15)] public string rewardJson;
    }

    [Serializable]
    public class RewardTrackDataConfig
    {
        public List<RewardTrackFeatureData> feature;
        public List<RewardTrackData> trackData;
        public List<RewardTargetData> targets;
        public List<RewardTrackRewardData> rewards;
    }

    [CreateAssetMenu(fileName = "RewardTrackConfig", menuName = "Config/Game/RewardTrackConfig")]
    public class RewardTrackConfig : JsonConvertableConfig
    {
        [SerializeField] private RewardTrackDataConfig configData;

        public RewardTrackFeatureData Feature => configData.feature[0];
        public List<RewardTrackData> TrackDatas => configData.trackData;
        public List<RewardTargetData> Targets => configData.targets;
        public List<RewardTrackRewardData> Rewards => configData.rewards;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "RewardTrackConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (RewardTrackDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(RewardTrackDataConfig);

#if UNITY_EDITOR
        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.feature, nameof(Feature), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.trackData, nameof(TrackDatas), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.targets, nameof(Targets), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.rewards, nameof(Rewards), ref data);

            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.feature = GoogleDocsUtils.SnatchDataRowsToConfig<RewardTrackFeatureData>(tableData);
            configData.trackData = GoogleDocsUtils.SnatchDataRowsToConfig<RewardTrackData>(tableData);
            configData.targets = GoogleDocsUtils.SnatchDataRowsToConfig<RewardTargetData>(tableData);
            configData.rewards = GoogleDocsUtils.SnatchDataRowsToConfig<RewardTrackRewardData>(tableData);
        }
#endif
    }
}