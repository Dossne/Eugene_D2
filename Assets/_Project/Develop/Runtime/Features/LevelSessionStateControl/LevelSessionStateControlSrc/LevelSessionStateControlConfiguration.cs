using Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Features.LevelSessionStateControl
{
    [CreateAssetMenu(fileName = "LevelSessionStateControlConfiguration", menuName = "Config/Game/LevelSessionStateControlConfiguration")]
    public class LevelSessionStateControlConfiguration : JsonConvertableConfig
    {
        [Serializable]
        public class FeatureData
        {
            public bool isUnlocked;
        }

        [Serializable]
        public class FeatureDataList
        {
            public List<FeatureData> data;
        }

        [SerializeField] private FeatureDataList configData;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "LevelSessionStateControlConfiguration";

        protected override Type ConfigurationDataType => typeof(FeatureDataList);

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (FeatureDataList)value;
        }

        public bool IsFeatureUnlocked()
        {
            if (configData.data.Count < 1)
                return false;
            return configData.data[0].isUnlocked;
        }

#if UNITY_EDITOR
        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.data, nameof(FeatureData), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.data = GoogleDocsUtils.SnatchDataRowsToConfig<FeatureData>(tableData);
        }
#endif
    }
}