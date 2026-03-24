using Infrastructure.Popups;
using Infrastructure.SpriteAtlasControl;
using Infrastructure.WalletSystem;
using System.Collections.Generic;
using UnityEngine;
using VContainer;


namespace Features.CurrencyRewardDisplay
{
    public class CurrencyRewardDisplayPopup : PopupBase
    {
        [SerializeField] Transform currencyRewardDisplaySlotRoot;
        [SerializeField] CurrencyRewardDisplaySlot currencyRewardDisplaySlotPf;
        private SpriteAtlasService spriteAtlasService;
        private CurrencyConfig currencyConfig;
        List<(CurrencyType currency, int amount, int startValue)> currencyRewards;


        [Inject]
        public void Construct(SpriteAtlasService spriteAtlasService)
        {
            this.spriteAtlasService = spriteAtlasService;
        }


        public void Construct(List<(CurrencyType currency, int amount, int startValue)> currencyRewards, CurrencyConfig currencyConfig)
        {
            this.currencyRewards = currencyRewards;
            this.currencyConfig = currencyConfig;
        }


        protected override void OnInitialize()
        {
            for(int i = 0; i < currencyRewards.Count; ++i)
            {
                var item = currencyRewards[i];
                currencyConfig.TryGet(item.currency, out CurrencyData currencyData);
                CurrencyRewardDisplaySlot slot = Instantiate(currencyRewardDisplaySlotPf, currencyRewardDisplaySlotRoot);
                slot.Construct(spriteAtlasService.GetFromMain(currencyData.packIconName), item.amount);
            }
        }


        protected override void OnDeinitialize()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}



