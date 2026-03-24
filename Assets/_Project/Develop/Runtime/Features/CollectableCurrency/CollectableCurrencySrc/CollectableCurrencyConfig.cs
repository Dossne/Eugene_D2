using Features.Collectables;
using Infrastructure.WalletSystem;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Features.CollectableCurrency
{
    [Serializable]
    public class CollectableCurrencyData
    {
        public CollectableType collectableType;
        public CurrencyType currencyType;
    }


    [CreateAssetMenu(fileName = "CollectableCurrencyConfig", menuName = "Config/Game/CollectableCurrencyConfig")]
    public class CollectableCurrencyConfig : ScriptableObject
    {        
        [SerializeField] List<CollectableCurrencyData> collectableCurrencyData;



        public List<CollectableCurrencyData> CollectableCurrencyData => collectableCurrencyData;



        public bool TryGet(CollectableType collectableType, out CollectableCurrencyData result)
        {
            result = null;
            for(int i = 0; i < collectableCurrencyData.Count; ++i)
            {
                if (collectableCurrencyData[i].collectableType == collectableType)
                {
                    result = collectableCurrencyData[i];
                    return true;
                }
            }

            return false;
        }


        public bool TryGet(CurrencyType currencyType, out CollectableCurrencyData result)
        {
            result = null;
            for (int i = 0; i < collectableCurrencyData.Count; ++i)
            {
                if (collectableCurrencyData[i].currencyType == currencyType)
                {
                    result = collectableCurrencyData[i];
                    return true;
                }
            }

            return false;
        }
    }
}


