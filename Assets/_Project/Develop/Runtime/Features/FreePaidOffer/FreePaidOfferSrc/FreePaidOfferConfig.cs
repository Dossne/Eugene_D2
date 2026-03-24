using Infrastructure.Configuration;
using Infrastructure.PurchaseSystem;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Features.FreePaidOffer
{
    [Serializable]
    public class FreePaidOfferFeatureData
    {
        public bool isFeatureEnabled;
        public int unlockLevel;
        public int durationSec;
    }


    [Serializable]
    public class FreePaidOfferPresetData
    {
        public int presetId;
        public float cumulativeSpentMin = 0.0f;
    }


    [Serializable]
    public class FreePaidOfferRewardData
    {
        public int presetId;
        [FixEnumNames] public OfferId InAppOfferId;
        public string rewardJson;
    }


    [Serializable]
    public class FreePaidOfferDataConfig
    {
        public List<FreePaidOfferFeatureData> feature;
        public List<FreePaidOfferPresetData> presets;
        public List<FreePaidOfferRewardData> rewards;
    }


    [CreateAssetMenu(fileName = "FreePaidOfferConfig", menuName = "Config/Game/FreePaidOfferConfig")]
    public class FreePaidOfferConfig : JsonConvertableConfig
    {
        [SerializeField] private FreePaidOfferDataConfig configData;
        private Dictionary<int, List<FreePaidOfferRewardData>> rewardsData = new Dictionary<int, List<FreePaidOfferRewardData>>();

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "FreePaidOfferConfig";


        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (FreePaidOfferDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(FreePaidOfferDataConfig);


        public FreePaidOfferFeatureData Feature => configData.feature[0];
        public List<FreePaidOfferPresetData> Presets => configData.presets;
        //public List<FreePaidOfferRewardData> Rewards => configData.rewards;



        public void Initialize()
        {
            rewardsData.Clear();
            for (int i = 0; i < configData.rewards.Count; ++i)
            {
                FreePaidOfferRewardData rewardData = configData.rewards[i];
                if(!rewardsData.ContainsKey(rewardData.presetId))
                {
                    rewardsData.Add(rewardData.presetId, new List<FreePaidOfferRewardData>());
                }

                rewardsData[rewardData.presetId].Add(rewardData); 
            }
        }


        public string GetRewardJson(OfferId InAppOfferId)
        {
            for(int i = 0; i < configData.rewards.Count; ++i)
            {
                if (configData.rewards[i].InAppOfferId == InAppOfferId)
                {
                    return configData.rewards[i].rewardJson;
                }
            }

            return string.Empty;
        }


        public List<FreePaidOfferRewardData> GetRewardsData(int presetId)
        {
            if (rewardsData.ContainsKey(presetId))
            {
                return rewardsData[presetId];
            }

            return new List<FreePaidOfferRewardData>();
        }


        public int GetOffersCount(int presetId)
        {
            if(rewardsData.ContainsKey(presetId))
            {
                return rewardsData[presetId].Count;
            }

            return 0;
        }


#if UNITY_EDITOR

        public override GoogleDocsUtils.MajorDimension SpreadSheetDimension => GoogleDocsUtils.MajorDimension.COLUMNS;

        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.feature, nameof(Feature), ref data);
            GoogleDocsUtils.AddConfigToDataColumns(configData.presets, nameof(Presets), ref data);
            GoogleDocsUtils.AddConfigToDataColumns(configData.rewards, nameof(configData.rewards), ref data);

            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.feature = GoogleDocsUtils.SnatchDataRowsToConfig<FreePaidOfferFeatureData>(tableData);
            configData.presets = GoogleDocsUtils.SnatchDataColumnsToConfig<FreePaidOfferPresetData>(tableData);
            configData.rewards = GoogleDocsUtils.SnatchDataColumnsToConfig<FreePaidOfferRewardData>(tableData);
        }
#endif
    }
}
