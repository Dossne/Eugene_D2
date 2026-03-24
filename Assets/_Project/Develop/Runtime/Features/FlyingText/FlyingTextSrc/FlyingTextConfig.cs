using System;
using System.Collections.Generic;
using Infrastructure.Configuration;
using UnityEngine;

namespace Features.FlyingText
{
    [Serializable]
    public class CounterTextData
    {
        public int points;
        public float textScale;
    }

    [Serializable]
    public class FlyingTextTargetData
    {
        [Header("Counter text")]
        public float counterTextHeight;
        public float counterRandPosX;
        public float counterRandPosY;
        public float inputMultiplierX;
        
        [Header("LevelUp text")]
        public float levelUpTextHeight;
    }

    [Serializable]
    public class FlyingTextDataConfig
    {
        public List<CounterTextData> textConfig;
        public List<FlyingTextTargetData> textTargetConfig;
    }

    [CreateAssetMenu(fileName = "FlyingTextConfig", menuName = "Config/Game/FlyingTextConfig")]
    public class FlyingTextConfig : JsonConvertableConfig
    {
        [SerializeField] private FlyingTextDataConfig configData;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "FlyingTextConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (FlyingTextDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(FlyingTextDataConfig);

        public List<CounterTextData> FlyingTextData => configData.textConfig;
        public List<FlyingTextTargetData> TextTargetConfig => configData.textTargetConfig;

        
#if UNITY_EDITOR
        
        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.textConfig, nameof(FlyingTextData), ref data);
            GoogleDocsUtils.AddConfigToDataRows(configData.textTargetConfig, nameof(TextTargetConfig), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.textConfig = GoogleDocsUtils.SnatchDataRowsToConfig<CounterTextData>(tableData);
            configData.textTargetConfig = GoogleDocsUtils.SnatchDataRowsToConfig<FlyingTextTargetData>(tableData);
        }
        
#endif
    }
}