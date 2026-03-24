using Infrastructure.Popups;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Features.FreePaidOffer
{
    public class FreePaidOfferPopup : PopupBase
    {
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] RectTransform slotParent;
        [SerializeField] List<RectTransform> slotsAnchors;


        [Header("Skin images")]
        [SerializeField] private Image headerSkinImage;

        List<FreePaidOfferSlot> slots = new List<FreePaidOfferSlot>();



        public List<FreePaidOfferSlot> Slots => slots;
        public List<RectTransform> SlotsAnchors => slotsAnchors;
        public int DisplayedSlotsAmount => slotsAnchors.Count - 1;


        public void RemoveOffers()
        {
            slots.Clear();
            foreach (RectTransform child in slotParent)
            {
                Destroy(child.gameObject);
            }
        }


        public void RefreshSkin(Sprite headerSkinSprite, string headerText)
        {
            this.headerText.text = headerText;
            headerSkinImage.sprite = headerSkinSprite;
        }


        public void SetRestTime(string restTimeText)
        {
            timerText.text = restTimeText;
        }


        public void AddSlot(FreePaidOfferSlot slot, int slotIdx, int currentSlotIdx, int slotsAmount)
        {
            RectTransform slotRectTransform = slot.transform as RectTransform;
            slotRectTransform.SetParent(slotParent);
            slotRectTransform.localScale = Vector3.one;
            slotRectTransform.rotation = Quaternion.identity;
            slots.Add(slot);

            if(IsSlotDisplayed(slotIdx, currentSlotIdx, slotsAmount))
            {
                int firstDisplayedIdx = GetFirstDislpayedIdx(currentSlotIdx, slotsAmount);
                slotRectTransform.position = slotsAnchors[slotIdx - firstDisplayedIdx].position;
            }
        }


        public void RefreshSlots(int currentSlot)
        {
            for (int i = 0; i < slots.Count; ++i)
            {
                FreePaidOfferSlot slot = slots[i];
                slot.gameObject.SetActive(IsSlotDisplayed(i, currentSlot));
                slot.Refresh(isClaimed: i < currentSlot, isOpened: currentSlot == i);
            }
        }


        public bool GetSlot(int slotIdx, out FreePaidOfferSlot freePaidOfferSlot)
        {
            bool isSlotExist = slotIdx < slots.Count;
            freePaidOfferSlot = isSlotExist ? slots[slotIdx] : null;
            return isSlotExist;
        }


        public int GetFirstDislpayedIdx(int currentSlotIdx)
        {
            return Mathf.Min(currentSlotIdx, slots.Count - DisplayedSlotsAmount);
        }


        private int GetFirstDislpayedIdx(int currentSlotIdx, int slotCount)
        {
            return Mathf.Min(currentSlotIdx, slotCount - DisplayedSlotsAmount);
        }


        public bool IsSlotDisplayed(int slotIdx, int currentSlotIdx)
        {
            return IsSlotDisplayed(slotIdx, currentSlotIdx, slots.Count);
        }


        private bool IsSlotDisplayed(int slotIdx, int currentSlotIdx, int slotCount)
        {
            int firstDisplayedIdx = GetFirstDislpayedIdx(currentSlotIdx, slotCount);
            return slotIdx >= firstDisplayedIdx && slotIdx < currentSlotIdx + DisplayedSlotsAmount;
        }
    }
}

