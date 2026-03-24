using System;
using System.Collections.Generic;
using Infrastructure.Configuration;
using UnityEngine;

namespace Features.Widgets
{
    [Serializable]
    public class WidgetData
    {
        public WidgetId id;
        public RootSide root;
        public int priority;
    }

    [Serializable]
    public class WidgetDataConfig
    {
        public List<WidgetData> datas;
    }

    [CreateAssetMenu(fileName = "WidgetsConfig", menuName = "Config/WidgetsConfig")]
    public class WidgetsConfig : JsonConvertableConfig
    {
        [SerializeField] private WidgetDataConfig configData;

        public List<WidgetData> Items => configData.datas;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "WidgetsConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (WidgetDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(WidgetDataConfig);

#if UNITY_EDITOR
        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.datas, nameof(Items), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }

        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.datas = GoogleDocsUtils.SnatchDataRowsToConfig<WidgetData>(tableData);
        }
#endif
    }
}