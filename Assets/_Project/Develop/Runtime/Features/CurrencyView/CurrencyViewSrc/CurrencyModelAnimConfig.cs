using System;
using System.Collections.Generic;
using Infrastructure.HapticControl;
using UnityEngine;


namespace Features.CurrencyView
{
    public enum FxType
    {
        FlyToUi,
    }


    [Serializable]
    public class CurrencyModelFxData
    {
        public FxType fxType;
        public int itemsCount;
        public float delay;
        public float zOffset;
        public HapticType hapticOnFinish = HapticType.Selection;
        public CurrencyModelAnimationParams animParams;
    }


    [CreateAssetMenu(fileName = "CurrencyModelAnimConfig", menuName = "Config/Game/Wallet/CurrencyModelAnimConfig")]
    public class CurrencyModelAnimConfig : ScriptableObject
    {
        [SerializeField] private List<CurrencyModelFxData> fxDatas;
        private Dictionary<FxType, CurrencyModelFxData> cached = new();


        public List<CurrencyModelFxData> FxDatas => fxDatas;



        public void Initialize()
        {
            cached.Clear();

            foreach (var data in fxDatas)
            {
                if (cached.TryAdd(data.fxType, data))
                    continue;

                Debug.LogError($"[GAME DESIGN] Fx duplicate {data.fxType}. Check config {nameof(CurrencyModelAnimConfig)}", this);
            }
        }


        public bool TryGet(FxType type, out CurrencyModelFxData result)
        {
            if (cached.TryGetValue(type, out result))
                return true;

            Debug.LogError($"[GAME DESIGN] Data not found for fx {type}. Check config {nameof(CurrencyModelAnimConfig)}", this);
            return false;
        }
    }
}

