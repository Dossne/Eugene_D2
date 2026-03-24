using System;
using System.Collections.Generic;
using Infrastructure.Configuration;
using UnityEngine;

namespace Infrastructure.Reward.Container
{
    [Serializable]
    public class RewardContainerData
    {
        public string id;
        public string iconName;
        [TextArea(3, 10)] public string rewardJson;
    }

    [Serializable]
    public class RewardContainerDataConfig
    {
        public List<RewardContainerData> datas;
    }

    [CreateAssetMenu(fileName = "RewardContainersConfig", menuName = "Config/Game/RewardContainersConfig")]
    public class RewardContainersConfig : JsonConvertableConfig
    {
        [SerializeField] private RewardContainerDataConfig configData;

        public List<RewardContainerData> Items => configData.datas;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "RewardContainersConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (RewardContainerDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(RewardContainerDataConfig);


        public bool TryGetData(string id, out RewardContainerData result)
        {
            foreach (var data in Items)
            {
                if (data.id == id)
                {
                    result = data;
                    return true;
                }
            }

            result = null;
            return false;
        }
        
        
        
        public bool TryGetRewardContainerReward(string containerId, out (string iconName, ComplexReward complexReward) result)
        {
            result = default;

            if (string.IsNullOrEmpty(containerId))
            {
                return false;
            }

            if (!TryGetData(containerId, out RewardContainerData container))
                return false;

            ComplexReward reward = RewardUtils.ConvertFromJson<ComplexReward>(container.rewardJson);

            if (reward == null)
                return false;

            result = (container.iconName, reward);
            return true;

        }
        
        
#if UNITY_EDITOR
        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.datas, nameof(Items), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.datas = GoogleDocsUtils.SnatchDataRowsToConfig<RewardContainerData>(tableData);
        }
#endif
    }
}