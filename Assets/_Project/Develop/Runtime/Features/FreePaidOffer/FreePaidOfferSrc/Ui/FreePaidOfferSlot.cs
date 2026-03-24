using Features.Boosters;
using Features.FreePaidOffer;
using Features.PurchaseUi;
using Infrastructure.Reward;
using Infrastructure.SpriteAtlasControl;
using System;
using System.Collections.Generic;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class FreePaidOfferSlot : MonoBehaviour
{
    [SerializeField] PurchaseRewardGridView purchaseRewardGridView;
    [SerializeField] Button buyButton;
    [SerializeField] TextMeshProUGUI buttonTMP;
    [SerializeField] GameObject closedBackground;
    [SerializeField] CanvasGroup openedBackground;
    [SerializeField] Transform openedButtonBkgTransform;
    [SerializeField] Transform closedButtonBkgTransform;
    [SerializeField] GameObject lockImage;
    [SerializeField] GameObject tickGO;
    [SerializeField] private GameObject notifier;

    


    public int Idx { get; private set; } = 0;
    public Transform ButtonTransform => buyButton.transform;
    public Transform OpenedBkgButtonTransform => openedButtonBkgTransform;
    public Transform ClosedButtonBkgTransform => closedButtonBkgTransform;
    public PurchaseRewardGridView PurchaseRewardGridView => purchaseRewardGridView;
    public GameObject TickGO => tickGO;
    public GameObject LockImage => lockImage;
    public CanvasGroup OpenedBackground => openedBackground;



    public void Initialize(ComplexReward complexReward, 
        Action onBuyButtonClick, 
        SpriteAtlasService spriteAtlasService, 
        List<BoosterData> boosterConfig,
        int idx, 
        FreePaidOfferSkinData skinData,
        string buttonText)
    {
        Idx = idx;

        buyButton.onClick.AddListener(() =>
        {
            SetNotifierActive(false);
            onBuyButtonClick?.Invoke();
        });
        SetPriceText(buttonText);
        purchaseRewardGridView.Construct(complexReward, spriteAtlasService, boosterConfig);
        SetNotifierActive(false);
    }


    public void Deinitialize()
    {
        buyButton.onClick.RemoveAllListeners();
    }


    public void Refresh(bool isClaimed, bool isOpened)
    {
        SetActiveLockImage(!isClaimed && !isOpened);
        SetSkin(isClaimed || isOpened);
        buyButton.gameObject.SetActive(!isClaimed);
        openedButtonBkgTransform.gameObject.SetActive(!isClaimed);
        tickGO.SetActive(isClaimed);
    }


    public void SetPriceText(string priceText)
    {
        buttonTMP.text = priceText;
    }

    public void SetNotifierActive(bool value)
    {
        if (notifier == null)
        {
            Debug.Log("Notifier is not set");
            return;
        }

        notifier.SetObjectActive(value);
    }

    public void SetSkin(bool isOpened)
    {
        openedBackground.alpha = isOpened ? 1.0f : 0.0f;
        openedBackground.gameObject.SetActive(isOpened);
        closedBackground.SetActive(!isOpened);
    }


    public void SetActiveLockImage(bool isActive)
    {
        lockImage.SetActive(isActive);
    }


    public void SetButtonInteractable(bool IsInteractable)
    {
        buyButton.interactable = IsInteractable;
    }
}
