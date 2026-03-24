using Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Features.BottomPanel
{
    [Serializable]
    public class BottomPanelFeatureData
    {
        public bool isEnabled;
    }

    [Serializable]
    public class BottomPanelButtonData
    {
        public string id;
        public string iconName;
        public int unlockLevel;
        public float layoutMinWidth;
        public float layoutPrefferedWidth;
        public float toggleIconScale;
        public float untoggleIconScale;
        public string textKey;
    }

    [Serializable]
    public class BottomPanelConfigurationData
    {
        public List<BottomPanelFeatureData> featureData;
        public List<BottomPanelButtonData> buttonData;
    }

    [CreateAssetMenu(fileName = "BottomPanelConfiguration", menuName = "Config/BottomPanelConfiguration")]
    public class BottomPanelConfiguration : JsonConvertableConfig
    {
        [SerializeField] private BottomPanelConfigurationData configData;

        public BottomPanelFeatureData Feature => configData.featureData[0];
        public List<BottomPanelButtonData> Buttons => configData.buttonData;

        public BottomPanelButtonData GetButtonData(string id)
        {
            return configData.buttonData.Find(x => x.id == id);
        }

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "BottomPanelConfiguration";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (BottomPanelConfigurationData)value;
        }

        protected override Type ConfigurationDataType => typeof(BottomPanelConfigurationData);

#if UNITY_EDITOR
        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.featureData, nameof(Feature), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.buttonData, nameof(Buttons), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }

        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.featureData = GoogleDocsUtils.SnatchDataRowsToConfig<BottomPanelFeatureData>(tableData);
            configData.buttonData = GoogleDocsUtils.SnatchDataRowsToConfig<BottomPanelButtonData>(tableData);
        }
#endif
    }
}