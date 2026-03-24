using System;
using System.Collections.Generic;
using Infrastructure.Configuration;
using Newtonsoft.Json;
using UnityEngine;

namespace Features.LavaQuest
{
    [Serializable]
    public class LavaQuestFeatureData
    {
        [JsonProperty("en")] public bool isFeatureEnabled;
        [JsonProperty("swv")] public int showWidgetLevel;
        [JsonProperty("lv")] public int unlockLevel;
        [JsonProperty("du")] public int durationSec;
        [JsonProperty("lc")] public int looseCdSec;
    }

    [Serializable]
    public class LavaQuestChainData
    {
        [JsonProperty("id")] public int id;
    }

    [Serializable]
    public class LavaQuestRewardData
    {
        [JsonProperty("id")] public int id;
        [JsonProperty("rj")] public string rewardJson;
        [JsonProperty("ri")] public string billboardRewardIcon;
    }

    [Serializable]
    public class LavaQuestStepData
    {
        [JsonProperty("id")] public int id;
        [JsonProperty("si")] public int stepIdx;
        [JsonProperty("mi")] public int minPlayers;
        [JsonProperty("ma")] public int maxPlayers;
        [JsonProperty("it")] public int iconCountTotal;
        [JsonProperty("if")] public int iconCountFall;
    }

    [Serializable]
    public class LavaQuestIconData
    {
        [JsonProperty("en")] public bool enemy;
        [JsonProperty("na")] public string name;
        [JsonProperty("ba")] public string back;
    }

    [Serializable]
    public class LavaQuestDataConfig
    {
        [JsonProperty("fe")] public List<LavaQuestFeatureData> feature;
        [JsonProperty("ic")] public List<LavaQuestIconData> icons;
        [JsonProperty("ch")] public List<LavaQuestChainData> chain;
        [JsonProperty("re")] public List<LavaQuestRewardData> rewards;
        [JsonProperty("st")] public List<LavaQuestStepData> steps;
    }

    [CreateAssetMenu(fileName = "LavaQuestConfig", menuName = "Config/Game/LavaQuestConfig")]
    public class LavaQuestConfig : JsonConvertableConfig
    {
        [SerializeField] private LavaQuestDataConfig configData;

        public LavaQuestFeatureData Feature => configData.feature[0];
        public List<LavaQuestIconData> Icons => configData.icons;
        public List<LavaQuestChainData> Chain => configData.chain;
        public List<LavaQuestRewardData> Rewards => configData.rewards;
        public List<LavaQuestStepData> Steps => configData.steps;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "LavaQuestConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (LavaQuestDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(LavaQuestDataConfig);

#if UNITY_EDITOR
        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.feature, nameof(LavaQuestFeatureData), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.icons, nameof(Icons), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.chain, nameof(Chain), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.rewards, nameof(Rewards), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.steps, nameof(Steps), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.feature = GoogleDocsUtils.SnatchDataRowsToConfig<LavaQuestFeatureData>(tableData);
            configData.icons = GoogleDocsUtils.SnatchDataRowsToConfig<LavaQuestIconData>(tableData);
            configData.chain = GoogleDocsUtils.SnatchDataRowsToConfig<LavaQuestChainData>(tableData);
            configData.rewards = GoogleDocsUtils.SnatchDataRowsToConfig<LavaQuestRewardData>(tableData);
            configData.steps = GoogleDocsUtils.SnatchDataRowsToConfig<LavaQuestStepData>(tableData);
        }
#endif
    }
}