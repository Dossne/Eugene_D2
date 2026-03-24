using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Infrastructure.Configuration
{
    [Obsolete("Не десериализуется из ремот конфигов Say в рантайме")]
    public abstract class SerializableDictionaryConfiguration<K, V> : JsonConvertableConfig where V : SerializableDictionaryConfigurationData<K>
    {
        [SerializeField] protected SerializedDictionary<K, V> configurationData = new();

        protected override object ConfigurationDataObject
        {
            get => configurationData;
            set => configurationData = (SerializedDictionary<K, V>)value;
        }

        protected override Type ConfigurationDataType => typeof(List<V>);

        public bool TryGetConfigurationData(K key, out V result)
        {
            return configurationData.TryGetValue(key, out result);
        }

        public List<K> GetKeys() => configurationData.Keys.ToList();

        public List<V> GetDataset() => configurationData.Values.ToList();

#if UNITY_EDITOR

        private void OnValidate()
        {
            ValidateImpl_Editor();
        }

        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            var keys = configurationData.Keys;
            
            foreach (var key in keys)
                configurationData[key].keyId = key;

            GoogleDocsUtils.AddConfigToDataRows(configurationData.Values.ToList(), "Config data", ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }

        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            var listData = GetValueList_Editor(tableData);
            configurationData.Clear();
            for (int i = 0; i < listData.Count; i++)
                configurationData.Add(listData[i].keyId, listData[i]);
        }

        protected abstract List<V> GetValueList_Editor(IList<IList<object>> tableData);

        protected virtual void ValidateImpl_Editor()
        {
            foreach (var item in configurationData)
            {
                if (item.Value == null)
                    continue;

                if (item.Value.keyId == null)
                    item.Value.keyId = item.Key;

                if (!item.Value.keyId.Equals(item.Key))
                    item.Value.keyId = item.Key;
            }
        }
#endif

    }
}