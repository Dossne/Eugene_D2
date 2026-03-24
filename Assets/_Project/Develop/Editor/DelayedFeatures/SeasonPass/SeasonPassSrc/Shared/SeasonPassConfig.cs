using System;
using System.Collections.Generic;
using Infrastructure.Configuration;
using Newtonsoft.Json;
using UnityEngine;

namespace Features.SeasonPass
{
    [Serializable]
    public class SeasonPassFeatureData
    {
        [JsonProperty("en")] public bool isEnabled;
        [JsonProperty("swv")] public int showWidgetLevel;
        [JsonProperty("lv")] public int unlockLevel;
        [JsonProperty("count")] public int giveCountPerLevel;
    }

    [Serializable]
    public class SeasonPassRewardData
    {
        [JsonProperty("sn")] public int stepNumber;
        [JsonProperty("cc")] public int collectCount;
        [JsonProperty("rj"), TextArea(1, 15)] public string rewardJson;
        [JsonProperty("prj"), TextArea(1, 15)] public string premiumRewardJson;
    }

    [Serializable]
    public class SeasonPassFeatureDataConfig
    {
        public List<SeasonPassFeatureData> featureData;
        public List<SeasonPassRewardData> rewards;
    }

    [CreateAssetMenu(fileName = "SeasonPassConfig", menuName = "Config/Game/SeasonPassConfig")]
    public class SeasonPassConfig : JsonConvertableConfig
    {
        [SerializeField] private SeasonPassFeatureDataConfig configData;

        public SeasonPassFeatureData Feature => configData.featureData[0];
        public List<SeasonPassRewardData> Rewards => configData.rewards;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "SeasonPassConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (SeasonPassFeatureDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(SeasonPassFeatureDataConfig);

#if UNITY_EDITOR
        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.featureData, nameof(Feature), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.rewards, nameof(Rewards), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }

        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.featureData = GoogleDocsUtils.SnatchDataRowsToConfig<SeasonPassFeatureData>(tableData);
            configData.rewards     = GoogleDocsUtils.SnatchDataRowsToConfig<SeasonPassRewardData>(tableData);
        }
#endif
    }
}