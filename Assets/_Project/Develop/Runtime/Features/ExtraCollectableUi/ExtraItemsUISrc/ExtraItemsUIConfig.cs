using System;
using System.Collections.Generic;
using Features.Collectables;
using Infrastructure.Configuration;
using UnityEngine;

namespace Features.ExtraCollectableUi
{
    [Serializable]
    public class ExtraItemsUIData
    {
        public CollectableType collectableType;
        public bool isEnabled;
        [Min(0)] public int slotIdx;
        public bool isActiveOnZero;
        public TextFormatType textFormatType;
    }

    [Serializable]
    public class ExtraCollectableUIDataConfig
    {
        public List<ExtraItemsUIData> datas;
    }

    [CreateAssetMenu(fileName = "ExtraItemsUIConfig", menuName = "Config/Game/ExtraItemsUIConfig")]
    public class ExtraItemsUIConfig : JsonConvertableConfig
    {
        [SerializeField] private ExtraCollectableUIDataConfig configData;

        public List<ExtraItemsUIData> Items => configData.datas;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "ExtraItemsUIConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (ExtraCollectableUIDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(ExtraCollectableUIDataConfig);


        public bool TryGetData(CollectableType collectableType, out ExtraItemsUIData data)
        {
            for (int i = 0; i < configData.datas.Count; i++)
            {
                var configItem = configData.datas[i];
                if (configItem.collectableType == collectableType)
                {
                    data = configItem;
                    return true;
                }
            }

            data = null;
            return false;
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
            configData.datas = GoogleDocsUtils.SnatchDataRowsToConfig<ExtraItemsUIData>(tableData);
        }
#endif
    }
}