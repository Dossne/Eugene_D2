using System;
using System.Collections.Generic;
using Infrastructure.Configuration;
using UnityEngine;


namespace Infrastructure.PurchaseSystem
{
    [Serializable]
    public class InAppData
    {
        [Header("InApp Info")]
        public int defaultOrder;
        [FixEnumNames] public OfferId id;
        public OfferType type = OfferType.InApp;
        public OfferGroup group;
        public PurchaseOfferUiType purchaseOfferUiType;
        public PurchaseProductType productType;
        public float firstRewardMultiplierPercent = 100;
        [TextArea(4,50)]public string rewardJson;
        public bool isBestPrice;
        public bool isPopular;
        public float price;
        public int unlockLevel;
        public OfferId unlockOfferId = OfferId.none;
        public bool oneTimeOffer = false;
        public float cumulativeSpentMin = float.MinValue;
        public float cumulativeSpentMax = float.MaxValue;
        public int  purchasePerCycleLimit = int.MaxValue;
        public bool limitedPurchasePerCycle = false;
        public bool hideOnPurchaseLimitReached = false;
        public bool isDecorative = false;
        public bool isEnabled = true;
    }

    [Serializable]
    public class InAppDataConfig
    {
        public List<InAppData> datas;
    }

    [CreateAssetMenu(fileName = "InAppConfig", menuName = "Config/InAppConfig")]
    public class InAppConfig : JsonConvertableConfig
    {
        [SerializeField] private InAppDataConfig configData;
        public override string GoogleTableName { get; set; }
        public override string GoogleSheetName { get; set; } = "InAppConfig";

        protected override object ConfigurationDataObject
        {
            get => configData;
            set => configData = (InAppDataConfig)value;
        }

        protected override Type ConfigurationDataType => typeof(InAppDataConfig);
        public List<InAppData> Items => configData.datas;

        public InAppData GetOfferData(OfferId inAppOfferId) 
            => configData.datas.Find(x  => x.id == inAppOfferId);

#if UNITY_EDITOR
        public override void ToGoogleSpreadSheet_Editor()
        {
            List<IList<object>> data = new List<IList<object>>();
            GoogleDocsUtils.AddConfigToDataRows(configData.datas, nameof(Items), ref data);
            GoogleDocsUtils.WriteAsync(data, fileName, GoogleSheetName, SpreadSheetDimension);
        }

        public override void TableDataToConfigData_Editor(IList<IList<object>> tableData)
        {
            configData.datas = GoogleDocsUtils.SnatchDataRowsToConfig<InAppData>(tableData);
        }
#endif
    }
}