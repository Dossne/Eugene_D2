using System;
using System.Collections.Generic;
using Infrastructure.Configuration;
using UnityEngine;

namespace Features.Character
{
    [Serializable]
    public class CharacterData
    {
        public int level;
        public float scale;
        public float moveSpeed;
        public float superSpeed;
    }

    [Serializable]
    public class CharacterDataConfig
    {
        public List<CharacterData> datas;
    }

    [CreateAssetMenu(fileName = "CharacterConfig", menuName = "Config/Game/CharacterConfig")]
    public class CharacterConfig : JsonConvertableConfig
    {

        [SerializeField] private CharacterDataConfig configData;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "CharacterConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (CharacterDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(CharacterDataConfig);

        private List<CharacterData> CharacterData => configData.datas;

        
        public bool TryGetDataByLevel(int level, out CharacterData result)
        {
            result = null;
            foreach (var dataItem in configData.datas)
            {
                if (dataItem.level == level)
                {
                    result = dataItem;
                    return true;
                }
            }

            return false;
        }
        
        public CharacterData GetMaxLevelData()
        {
            int maxLevelIdx = 0;
            int maxLevel = 0;
            for (int i = 0; i < configData.datas.Count; i++)
            {
                if (configData.datas[i].level > maxLevel)
                {
                    maxLevel = configData.datas[i].level;
                    maxLevelIdx = i;
                }
            }
            return configData.datas[maxLevelIdx];
        }
        
        
#if UNITY_EDITOR


        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.datas, nameof(CharacterData), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.datas = GoogleDocsUtils.SnatchDataRowsToConfig<CharacterData>(tableData);
        }
#endif
    }
}