using System;
using System.Collections.Generic;
using Infrastructure.Configuration;
using UnityEngine;

namespace Infrastructure.Localization
{
    [Serializable]
    public class LocalizationData
    {
        public string key;
        [TextArea(1, 10)] public string en;
    }

    [Serializable]
    public class LocalizationDataConfig
    {
        public List<LocalizationData> datas;
    }

    [CreateAssetMenu(fileName = "LocalizationConfig", menuName = "Config/System/LocalizationConfig")]
    public class LocalizationConfig : JsonConvertableConfig
    {
        [SerializeField] private LocalizationDataConfig configData;
        
        private readonly Dictionary<string, LocalizationData> cached = new();

        public List<LocalizationData> Items => configData.datas;

        public override bool IsSaveRequired => false;
        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "LocalizationConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (LocalizationDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(LocalizationDataConfig);

        
        public void Initialize()
        {
            cached.Clear();
            for (int i = 0; i < Items.Count; i++)
            {
                if (cached.ContainsKey(Items[i].key))
                {
                    Debug.LogError($"Duplicate key {Items[i].key}!");
                    continue;
                }
                
                cached.Add(Items[i].key, Items[i]);
            }
        }


        public bool TryGet(string key, out LocalizationData result)
        {
            return cached.TryGetValue(key, out result);
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
            configData.datas = GoogleDocsUtils.SnatchDataRowsToConfig<LocalizationData>(tableData);
        }
#endif
    }
}