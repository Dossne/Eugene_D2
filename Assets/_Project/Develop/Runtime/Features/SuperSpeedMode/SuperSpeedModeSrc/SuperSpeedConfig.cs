using System.Collections.Generic;

using UnityEngine;

using Infrastructure.Configuration;
using System;


namespace Features.SuperSpeedMode
{
    [Serializable]
    public class SuperSpeedData
    {
        public bool isFeatureEnabled;
        public int announceLevel;
        public int unlockLevel;
        public int maxWinCount;
    }

    [Serializable]
    public class SuperSpeedDataConfig
    {
        public List<SuperSpeedData> superSpeedData;
    }

    [CreateAssetMenu(fileName = "SuperSpeedConfig", menuName = "Config/Game/SuperSpeedConfig")]
    public class SuperSpeedConfig : JsonConvertableConfig
    {
        [Header("Google sheets")]
        [SerializeField] private SuperSpeedDataConfig configData;

        public SuperSpeedData SuperSpeedData => configData.superSpeedData[0];

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "SuperSpeedConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (SuperSpeedDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(SuperSpeedDataConfig);

        public bool IsFeatureEnabled() 
        {
            if (configData.superSpeedData.Count > 1)
                return false;

            return configData.superSpeedData[0].isFeatureEnabled;
        }

        public int AnnounceLevel()
        {
            if (configData.superSpeedData.Count > 1)
                return int.MaxValue;

            return configData.superSpeedData[0].announceLevel;
        }

        public int UnlockLevel()
        {
            if (configData.superSpeedData.Count > 1)
                return int.MaxValue;

            return configData.superSpeedData[0].unlockLevel;
        }

        public int MaxWinCount()
        {
            if (configData.superSpeedData.Count > 1)
                return -1;

            return configData.superSpeedData[0].maxWinCount;
        }

#if UNITY_EDITOR
        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.superSpeedData, nameof(SuperSpeedData), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.superSpeedData = GoogleDocsUtils.SnatchDataRowsToConfig<SuperSpeedData>(tableData);
        }
#endif
    }
}