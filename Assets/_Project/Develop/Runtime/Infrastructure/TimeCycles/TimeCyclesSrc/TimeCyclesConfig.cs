using System;
using System.Collections.Generic;
using Infrastructure.Configuration;
using UnityEngine;

namespace Infrastructure.TimeCycles
{
    [Serializable]
    public class TimeCycleData
    {
        public TimeCycleType cycleType;
        public int targetHour;
        public int targetMinute;
        public DayOfWeek targetDayOfWeek;
    }

    [Serializable]
    public class TimeCycleDataConfig
    {
        public List<TimeCycleData> datas;
    }

    [CreateAssetMenu(fileName = "TimeCyclesConfig", menuName = "Config/System/TimeCyclesConfig")]
    public class TimeCyclesConfig : JsonConvertableConfig
    {
        [SerializeField] private TimeCycleDataConfig configData;

        public List<TimeCycleData> Items => configData.datas;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "TimeCyclesConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (TimeCycleDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(TimeCycleDataConfig);

#if UNITY_EDITOR
        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.datas, nameof(Items), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.datas = GoogleDocsUtils.SnatchDataRowsToConfig<TimeCycleData>(tableData);
        }
#endif
    }
}