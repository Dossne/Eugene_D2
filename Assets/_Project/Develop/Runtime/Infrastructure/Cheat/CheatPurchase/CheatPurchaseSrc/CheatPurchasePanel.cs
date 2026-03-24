#if PR_CHEAT

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Features.LevelConfiguration;
using Infrastructure.Ads;
using Infrastructure.PersistentProgress;
using Infrastructure.Popups;
using Infrastructure.PurchaseSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Infrastructure.Cheat.CheatPurchase
{
    public class CheatPurchasePanel : MonoBehaviour
    {

        [SerializeField] private Button cheatBtn;
        [SerializeField] private TextMeshProUGUI cheatBtnTxt;
        [SerializeField] private TextMeshProUGUI spentUsdTxt;
        [SerializeField] private List<Button> closeBtns;

        private IPurchaseManager purchaseManager;
        private PurchaseOfferContainer purchaseOfferContainer;
        private bool isInit;
        public bool IsInit => isInit;


        [Inject]
        public void Construct(IPurchaseManager purchaseManager, PurchaseOfferContainer purchaseOfferContainer)
        {
            this.purchaseManager = purchaseManager;      
            this.purchaseOfferContainer = purchaseOfferContainer;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            foreach (var btn in closeBtns)
            {
                btn.onClick.AddListener(Close);
            }

            cheatBtn.onClick.AddListener(SwitchCheatPurchase);

            cheatBtnTxt.text = purchaseManager.CheatPurchaseEnabled ? "Purchase is ON" : "Purchase is OFF";
            spentUsdTxt.text = $"${purchaseOfferContainer.CumulativeSpentUsd} spent";
            purchaseOfferContainer.OnPurchaseCompleted += PurchaseOfferContainer_OnPurchaseCompleted;
            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            foreach (var btn in closeBtns)
            {
                btn.onClick.RemoveListener(Close);
            }
            cheatBtn.onClick.RemoveListener(SwitchCheatPurchase);
            purchaseOfferContainer.OnPurchaseCompleted -= PurchaseOfferContainer_OnPurchaseCompleted;
            isInit = true;
        }


        public void SetObjectActive(bool value)
        {
            if (gameObject.activeSelf == value)
                return;

            gameObject.SetActive(value);
        }


        private void Close()
        {
            gameObject.SetActive(false);
        }

        private void SwitchCheatPurchase()
        {
            purchaseManager.SetCheatPurchaseEnabled(!purchaseManager.CheatPurchaseEnabled);
            cheatBtnTxt.text = purchaseManager.CheatPurchaseEnabled ? "Purchase is ON" : "Purchase is OFF";
        }

        private void PurchaseOfferContainer_OnPurchaseCompleted(OfferId id, bool arg2)
        {
            spentUsdTxt.text = $"${purchaseOfferContainer.CumulativeSpentUsd} spent";
        }
    }
}
#endif