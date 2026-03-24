using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.SeasonPass
{
    public class SeasonPassSlotContainer : MonoBehaviour
    {
        public event Action<SlotViewData> OnClicked;

        [SerializeField] private TextMeshProUGUI slotLabelText;
        [SerializeField] private SerializedDictionary<SlotViewType, SeasonPassSlotView> slotViews;
        [SerializeField] private HorizontalLayoutGroup horizontalLayoutGroup;

        private bool isInit;

        public void Construct(int stepIdx, List<SlotViewData> slotViewData)
        {
            if (slotViewData.Count == 0)
            {
                Debug.LogError($"[SeasonPass] Slot view data is empty for stepIdx = {stepIdx}");
                return;
            }

            foreach (var slotData in slotViewData)
            {
                if (!slotViews.TryGetValue(slotData.viewType, out SeasonPassSlotView slotView))
                {
                    Debug.LogError($"Slot view {slotData.viewType} not found");
                    continue;
                }

                slotView.Construct(slotData);
            }

            slotLabelText.text = (stepIdx + 1).ToString();
        }

        public void Initialize()
        {
            if (isInit)
                return;

            SetObjectActive(true);
            horizontalLayoutGroup.enabled = true;

            foreach (var slotView in slotViews.Values)
            {
                slotView.Initialize();
                slotView.OnClicked += SeasonPassUISlot_OnClicked;
            }

            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            foreach (var slotView in slotViews.Values)
            {
                slotView.Deinitialize();
            }

            isInit = false;
        }

        public void Destroy()
        {
            foreach (var slotView in slotViews.Values)
            {
                slotView.Destroy();
            }

            Destroy(gameObject);
        }

        public void DisableHorizontalLayoutGroup()
        {
            horizontalLayoutGroup.enabled = false;
        }

        private void SetObjectActive(bool value)
        {
            gameObject.SetObjectActive(value);
        }

        private void SeasonPassUISlot_OnClicked(SlotViewData slotViewData)
        {
            OnClicked?.Invoke(slotViewData);
        }
    }
}