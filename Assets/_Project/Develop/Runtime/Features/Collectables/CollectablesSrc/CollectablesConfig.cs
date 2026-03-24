using System;
using System.Collections.Generic;
using Infrastructure.Configuration;
using UnityEngine;

namespace Features.Collectables
{

    [Serializable]
    public class CollectablesData
    {
        public CollectableType type;
        public string iconName;
        public string iconResurrectName;
        public int cost;
        public int size;
        public float defaultScaleOverride = 1;
    }

    [Serializable]
    public class CollectablesDataConfig
    {
        public List<CollectablesData> config;
    }

    [CreateAssetMenu(fileName = "CollectablesConfig", menuName = "Config/Game/CollectablesConfig")]
    public class CollectablesConfig : JsonConvertableConfig
    {
        [SerializeField] protected CollectablesDataConfig configData = new();

        private Dictionary<CollectableType, CollectablesData> grouped = new();

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "CollectablesConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (CollectablesDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(CollectablesDataConfig);

        private List<CollectablesData> CollectablesData => configData.config;


        public void Initialize()
        {
            grouped.Clear();

            foreach (var item in CollectablesData)
            {
                grouped.Add(item.type, item);
            }
        }


        public CollectablesData Get(CollectableType type)
        {
            return grouped[type];
        }


        public bool TryGet(CollectableType type, out CollectablesData data)
        {
            return grouped.TryGetValue(type, out data);
        }
        

        public void Add(CollectablesData data)
        {
            configData.config.Add(data);
            grouped.Add(data.type, data);
        }
        
#if UNITY_EDITOR


        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.config, nameof(CollectablesData), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.config = GoogleDocsUtils.SnatchDataRowsToConfig<CollectablesData>(tableData);
        }


#endif
    }
}