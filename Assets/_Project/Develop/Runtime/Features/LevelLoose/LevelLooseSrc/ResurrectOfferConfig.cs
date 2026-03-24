using System;
using System.Collections.Generic;

using UnityEngine;

using Infrastructure.Configuration;
using Infrastructure.PurchaseSystem;
using System.Linq;


namespace Features.LevelLoose
{
    [Serializable]
    public class ResurrectOfferDataConfig
    {
        public List<ResurrectOfferData> datas;
    }

    [CreateAssetMenu(fileName = "ResurrectOfferConfig", menuName = "Config/Game/ResurrectOfferConfig")]
    public class ResurrectOfferConfig : JsonConvertableConfig
    {
        [SerializeField] private ResurrectOfferDataConfig configData;

        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "ResurrectOfferConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (ResurrectOfferDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(ResurrectOfferDataConfig);

        public List<ResurrectOfferData> Items => configData.datas;

        public List<OfferId> GetOrderedOffers()
        {
            List<OfferId> result = configData.datas.OrderBy(x => x.order)
                                                   .Select(x => x.offerId)
                                                   .ToList();
            return result;
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
            configData.datas = GoogleDocsUtils.SnatchDataRowsToConfig<ResurrectOfferData>(tableData);
        }
#endif
    }
}