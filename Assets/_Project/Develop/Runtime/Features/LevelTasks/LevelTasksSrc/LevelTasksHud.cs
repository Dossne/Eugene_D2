using System.Collections.Generic;
using Features.Collectables;
using Infrastructure.AssetManagement;
using Infrastructure.SpriteAtlasControl;
using UnityEngine;
using UnityEngine.UI;

namespace Features.LevelTasks
{
    public class LevelTasksHud : MonoBehaviour
    {
        [SerializeField] private Transform contentRoot;
        [SerializeField] private ContentSizeFitter contentSizeFitter;
        [SerializeField] private TasksSlotUI slotPf;

        private Dictionary<CollectableType, TasksSlotUI> slots = new();

        private Instantiator instantiator;
        private SpriteAtlasService spriteAtlasService;
        private CollectablesConfig collectablesConfig;
        private Dictionary<CollectableType, int> tasks;

        private bool isInit;

        private bool shrinkRoutineRunning = false;


        public void Construct(Instantiator instantiator, SpriteAtlasService spriteAtlasService, CollectablesConfig collectablesConfig,
                              Dictionary<CollectableType, int> tasks)
        {
            this.instantiator = instantiator;
            this.spriteAtlasService = spriteAtlasService;
            this.collectablesConfig = collectablesConfig;
            this.tasks = tasks;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            InitUISlots();
            SetObjectActive(true);
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            SetObjectActive(false);
            foreach (var pair in slots)
            {
                Destroy(pair.Value.gameObject);
            }

            slots.Clear();
            isInit = false;
        }


        public RectTransform GetIconRect(CollectableType itemType)
        {
            return slots[itemType].IconRect;
        }


        public void RefreshState(CollectableType itemType, int currentCount, bool withFx)
        {
            if (!slots.TryGetValue(itemType, out TasksSlotUI slot)) 
                return;
            
            slot.Refresh(currentCount);
            
            if(withFx)
                slot.PlayCountFx();
        }


        private void InitUISlots()
        {
            foreach (var pair in tasks)
            {
                if (!slots.TryGetValue(pair.Key, out TasksSlotUI slot))
                {
                    slot = instantiator.Instantiate(slotPf, parent: contentRoot, worldSpace: false);
                    slot.OnShrinkStep += Slot_OnShrinkStep;
                    slots.Add(pair.Key, slot);
                    slot.SetObjectActive(true);
                }

                string iconName = collectablesConfig.Get(pair.Key).iconName;
                Sprite icon = spriteAtlasService.GetFromMain(iconName);
                slot.Construct(icon, pair.Value);
                slot.ResetState();
            }
        }


        private void Slot_OnShrinkStep()
        {
            RefreshContentSize();
        }


        private void RefreshContentSize()
        {
            if (!shrinkRoutineRunning)
                StartCoroutine(Routine());
        }


        System.Collections.IEnumerator Routine()
        {
            shrinkRoutineRunning = true;
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            yield return null;
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            shrinkRoutineRunning = false;
        }


        private void SetObjectActive(bool value)
        {
            if (gameObject.activeSelf == value)
                return;

            gameObject.SetActive(value);
        }
    }
}