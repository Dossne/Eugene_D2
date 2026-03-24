using System;
using System.Collections.Generic;
using Infrastructure.Configuration;
using UnityEngine;

namespace Infrastructure.Ads
{
    [Serializable]
    public class AdsData
    {
        public int  interMinLvlNumber;
        public int  rateUsMinLvlNumber;
        public int  rewardedResurrectPerDay;
        public int  rewardedLifePerDay;
        public bool isBannerEnabled;
    }

    [Serializable]
    public class AdsDataConfig
    {
        public List<AdsData> datas;
    }

    [CreateAssetMenu(fileName = "AdsConfig", menuName = "Config/AdsConfig")]
    public class AdsConfig : JsonConvertableConfig
    {

        [SerializeField] private AdsDataConfig configData;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "AdsConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (AdsDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(AdsDataConfig);

        public List<AdsData> AdsList => configData.datas;
        public AdsData AdsData => configData.datas[0];

#if UNITY_EDITOR


        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();

            GoogleDocsUtils.AddConfigToDataRows(configData.datas, nameof(AdsList), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }


        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.datas = GoogleDocsUtils.SnatchDataRowsToConfig<AdsData>(tableData);
        }
#endif
    }
}