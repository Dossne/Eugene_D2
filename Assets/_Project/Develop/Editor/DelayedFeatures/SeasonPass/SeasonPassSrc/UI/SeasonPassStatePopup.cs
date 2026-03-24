using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using Infrastructure.Popups;
using Infrastructure.TooltipControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.SeasonPass
{
    public class SeasonPassStatePopup : PopupBase
    {
        public event Action<SlotViewData> OnClicked;

        [SerializeField] private SeasonPassScrollLabel scrollLabelPf;
        [SerializeField] private SeasonPassSlotContainer slotContainerPf;

        [Header("Progress")]
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TextMeshProUGUI progressTxt;

        [Header("Scroll")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
        [SerializeField] private RectTransform contentRoot;
        //[SerializeField] private float customOffset = 100;

        [Header("Tooltip")]
        [SerializeField] private SerializedDictionary<SlotViewType, Tooltip> tooltipMap;

        private ScrollElementDataDto[] stateDatas;
        private List<SeasonPassSlotContainer> slotContainers = new();

        public void Construct(ScrollElementDataDto[] stateDatas)
        {
            this.stateDatas = stateDatas;
        }

        public bool TryGetTooltip(SlotViewType slotViewType, out Tooltip tooltip)
        {
            return tooltipMap.TryGetValue(slotViewType, out tooltip);
        }

        protected override void OnInitialize()
        {
            slotContainers.Clear();
            var scrollLabel = Instantiate(scrollLabelPf, contentRoot);
            scrollLabel.Initialize();

            for (var i = 0; i < stateDatas.Length; i++)
            {
                SeasonPassSlotContainer container = Instantiate(slotContainerPf, contentRoot);

                container.Construct(i, stateDatas[i].datas);
                container.Initialize();
                container.OnClicked += SeasonPassScrollSlot_OnClicked;
                slotContainers.Add(container);
            }

            foreach (var entryPair in tooltipMap)
            {
                entryPair.Value.Initialize();
            }

            DisableHorizontalLayoutGroupNextFrame().Forget();
            stateDatas = null;
        }

        protected override void OnDeinitialize()
        {
            for (var i = 0; i < slotContainers.Count; i++)
            {
                slotContainers[i].Deinitialize();
                slotContainers[i].Destroy();
            }

            foreach (var entryPair in tooltipMap)
            {
                entryPair.Value.Deinitialize();
            }

            OnClicked = null;
            slotContainers.Clear();
        }

        private async UniTask DisableHorizontalLayoutGroupNextFrame()
        {
            await UniTask.NextFrame(gameObject.GetCancellationTokenOnDestroy());

            foreach (var container in slotContainers)
            {
                container.DisableHorizontalLayoutGroup();
            }
        }

        private void SeasonPassScrollSlot_OnClicked(SlotViewData slotViewData)
        {
            OnClicked?.Invoke(slotViewData);
        }
    }
}