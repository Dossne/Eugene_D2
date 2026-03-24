using System;
using System.Collections.Generic;
using Infrastructure.Configuration;
using UnityEngine;

namespace Features.CameraFollow
{
    [Serializable]
    public class CameraData
    {
        public int level;
        public float posY;
        public float posZ;
        public float smoothTimeSec;
    }

    [Serializable]
    public class PrewiewData
    {
        public float posY;
        public float posZ;
        public float smoothTimeSec;
    }

    [Serializable]
    public class CameraDataConfig
    {
        public List<PrewiewData> preview;
        public List<CameraData> datas;
    }

    [CreateAssetMenu(fileName = "CameraConfig", menuName = "Config/Game/CameraConfig")]
    public class CameraConfig : JsonConvertableConfig
    {

        [SerializeField] private CameraDataConfig configData;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "CameraConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (CameraDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(CameraDataConfig);


        public PrewiewData PreviewData => configData.preview.Count > 0 ? configData.preview[0] : null;
        private List<CameraData> CameraData => configData.datas;


        public bool TryGetDataByLevel(int level, out CameraData result)
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

        public CameraData GetMaxLevelData()
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

            GoogleDocsUtils.AddConfigToDataRows(configData.preview, nameof(PreviewData), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.datas, nameof(CameraData), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.preview = GoogleDocsUtils.SnatchDataRowsToConfig<PrewiewData>(tableData);
            configData.datas = GoogleDocsUtils.SnatchDataRowsToConfig<CameraData>(tableData);
        }
#endif
    }
}