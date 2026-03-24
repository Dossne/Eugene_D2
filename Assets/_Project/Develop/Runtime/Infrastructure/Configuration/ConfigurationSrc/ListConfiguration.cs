using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.Configuration
{
    public abstract class ListConfiguration<K, V> : JsonConvertableConfig, IListConfiguration where K : IComparable where V : ListConfigurationData<K> 
    {
        [SerializeField] protected List<V> configurationData = new();

        protected override object ConfigurationDataObject
        {
            get => configurationData;
            set => configurationData = (List<V>)value;
        }

        protected override Type ConfigurationDataType => typeof(List<V>);

        public V GetConfigurationData(K key)
        {
            return configurationData.Find(x => x.id.Equals(key));
        }

        public List<V> GetConfigurationDataSet(K key)
        {
            return configurationData.FindAll(x => x.id.Equals(key));
        }

        public List<V> GetConfigurationData()
        {
            return configurationData;
        }
#if UNITY_EDITOR

        public void SortAndRename_Editor()
        {
            Sort_Editor();
            GenerateName_Editor();
        }

        private void Sort_Editor()
        {
            configurationData.Sort((x, y) =>
            {
                int result = x.sort.CompareTo(y.sort);
                if (result != 0)
                {
                    return result;
                }

                return x.id.CompareTo(y.id);
            });
        }

        protected virtual void GenerateName_Editor()
        {
            configurationData.ForEach(data => { data.SetName_Editor($"{data.sort} {data.id}"); });
        }
        
        public override void FromGoogleSpreadSheet_Editor()
        {
            base.FromGoogleSpreadSheet_Editor();
            SortAndRename_Editor();
        }

        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configurationData, GoogleTableName, ref data);

            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }
#endif

    }
}