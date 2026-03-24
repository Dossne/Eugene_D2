using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Collectables;
using Infrastructure.BroTweens;
using UnityEngine;
using UnityEngine.UI;

namespace Features.ExtraCollectableUi
{
    public class ExtraCollectableHud : MonoBehaviour
    {
        [SerializeField] private Transform contentRoot;

        private Dictionary<CollectableType, ExtraCollectableSlotUi> slots = new();

        private bool isInit;
        public Transform ContentRoot => contentRoot;


        public void Initialize()
        {
            if (isInit)
                return;

            SetObjectActive(true);
            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            foreach (var pair in slots)
            {
                pair.Value.Deinitialize();
                Destroy(pair.Value.gameObject);
            }

            slots.Clear();
            SetObjectActive(false);

            isInit = false;
        }


        public UniTask PlayCountFxAsync(CollectableType itemType, CancellationToken cancellationToken)
        {
            if (slots.TryGetValue(itemType, out ExtraCollectableSlotUi slot))
            {
                return slot.PlayCountFx().ToUniTask(cancellationToken: cancellationToken);
            }

            return UniTask.CompletedTask;
        }


        public void SetCountText(CollectableType itemType, int current, int max)
        {
            if (!slots.TryGetValue(itemType, out ExtraCollectableSlotUi slot))
                return;

            string text = GetText(slot.TextFormatType, current, max);
            slot.RefreshText(text, current == max);
        }


        public void SetSlotActive(CollectableType collectableType)
        {
            if (slots.TryGetValue(collectableType, out ExtraCollectableSlotUi slot))
            {
                slot.SetObjectActive(true);
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot as RectTransform);
            }
        }


        public bool TryGetSlot(CollectableType itemType, out ExtraCollectableSlotUi extraCollectableSlotUi)
        {
            return slots.TryGetValue(itemType, out extraCollectableSlotUi);
        }


        public void Add(CollectableType itemType, ExtraCollectableSlotUi slot)
        {
            slots.TryAdd(itemType, slot);
        }


        private void SetObjectActive(bool value)
        {
            if (gameObject.activeSelf == value)
                return;

            gameObject.SetActive(value);
        }


        private string GetText(TextFormatType textFormatType, int current, int max)
        {
            return textFormatType switch
            {
                TextFormatType.Current      => current.ToString(),
                TextFormatType.CurrentToMax => $"{current.ToString()}/{max.ToString()}",
                _                           => string.Empty
            };

        }
    }
}