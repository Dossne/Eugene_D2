using System;
using System.Collections.Generic;
using Infrastructure.Configuration;
using UnityEngine;

namespace Features.LevelUp
{
    [Serializable]
    public class LevelUpData
    {
        public int level;
        public int points;
    }

    [Serializable]
    public class LevelUpDataConfig
    {
        public List<LevelUpData> config;
    }

    [CreateAssetMenu(fileName = "LevelUpConfig", menuName = "Config/Game/LevelUpConfig")]
    public class LevelUpConfig : JsonConvertableConfig
    {
        [SerializeField] protected LevelUpDataConfig configData = new();

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "LevelUpConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (LevelUpDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(LevelUpDataConfig);

        public List<LevelUpData> LevelUpData => configData.config;


        public List<int> GetPoints()
        {
            var result = new List<int>();

            for (int i = 0; i < LevelUpData.Count; i++)
            {
                result.Add(LevelUpData[i].points);
            }

            return result;
        }


#if UNITY_EDITOR

        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.config, nameof(LevelUpData), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.config = GoogleDocsUtils.SnatchDataRowsToConfig<LevelUpData>(tableData);
        }


#endif
    }
}