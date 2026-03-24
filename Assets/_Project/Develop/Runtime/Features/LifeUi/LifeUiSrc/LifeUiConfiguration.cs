using Infrastructure.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Features.LifeUi
{
    [Serializable]
    public class LifeUiData
    {
        public bool rewardedLifeEnabled;
    }

    [Serializable]
    public class LifeUiDataConfig
    {
        public List<LifeUiData> datas = new();
    }

    [CreateAssetMenu(fileName = "LifeUiConfiguration", menuName = "Config/Game/LifeUiConfiguration")]
    public class LifeUiConfiguration : JsonConvertableConfig
    {
        [SerializeField] private LifeUiDataConfig configData = new();
        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "LifeUiConfiguration";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (LifeUiDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(LifeUiDataConfig);

        public LifeUiData LifeUiData => configData.datas[0];

#if UNITY_EDITOR
        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.datas, nameof(LifeUiData), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.datas = GoogleDocsUtils.SnatchDataRowsToConfig<LifeUiData>(tableData);
        }
#endif
    }
}