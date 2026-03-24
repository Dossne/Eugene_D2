using System;
using System.Collections.Generic;
using Infrastructure.Configuration;
using UnityEngine;

namespace Features.MainMenuUnlock
{
    [Serializable]
    public class MainMenuUnlockData
    {
        public int unlockLevelNumber;
    }

    [Serializable]
    public class MainMenuUnlockDataConfig
    {
        public List<MainMenuUnlockData> datas;
    }

    [CreateAssetMenu(fileName = "MainMenuUnlockConfig", menuName = "Config/Game/MainMenuUnlockConfig")]
    public class MainMenuUnlockConfig : JsonConvertableConfig
    {
        [SerializeField] private MainMenuUnlockDataConfig configData;

        public MainMenuUnlockData Item => configData.datas[0];

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "MainMenuUnlockConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (MainMenuUnlockDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(MainMenuUnlockDataConfig);

#if UNITY_EDITOR
        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.datas, nameof(Item), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.datas = GoogleDocsUtils.SnatchDataRowsToConfig<MainMenuUnlockData>(tableData);
        }
#endif
    }
}