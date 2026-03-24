using System;
using System.Collections.Generic;
using Features.Boosters.Model;
using Features.Character;
using Features.Collectables;
using Features.Events;
using Features.LevelTasks;
using Features.TargetMarker;
using Infrastructure.AudioControl;
using Infrastructure.Collections;
using Infrastructure.Configs;
using Infrastructure.SpriteAtlasControl;
using R3;
using UnityEngine;

namespace Features.Boosters.Behaviour.InGameBooster
{
    public sealed class ObjectFinderBehaviour : InGameBoosterBehaviour
    {
        private const float Second = 1;
        private readonly LevelTaskManager taskManager;
        private readonly ItemCollectManager itemCollectManager;
        private readonly SpriteAtlasService spriteAtlasService;
        private readonly CharacterManager characterManager;
        private readonly TargetMarkerService targetMarkerService;
        private readonly CollectablesConfig collectablesConfig;
        private readonly HashSet<CollectableItem> activeTargets;
        private readonly CollectItemEvent collectItemEvent;
        private readonly List<(CollectableItem, float)> tasksByDistance;

        private CompositeDisposable disposable;
        private float second;


        public ObjectFinderBehaviour(InGameBoosterModelBase model,
            InGameBoosterSlotView view,
            LevelTaskManager taskManager,
            ItemCollectManager itemCollectManager,
            SpriteAtlasService spriteAtlasService,
            CharacterManager characterManager,
            ConfigProvider configProvider,
            CollectItemEvent collectItemEvent,
            TargetMarkerService targetMarkerService) : base(model, view)
        {
            this.taskManager = taskManager;
            this.itemCollectManager = itemCollectManager;
            this.spriteAtlasService = spriteAtlasService;
            this.collectItemEvent = collectItemEvent;
            this.characterManager = characterManager;
            this.targetMarkerService = targetMarkerService;
            this.collectablesConfig = configProvider.CollectablesConfig;
            this.activeTargets = new HashSet<CollectableItem>();
            this.tasksByDistance = new List<(CollectableItem, float)>();
        }


        public override void Deinitialize()
        {
            ActionsOnUnapply();
        }


        protected override void ActionsOnApply()
        {
            disposable = new CompositeDisposable();
            collectItemEvent.Subscribe(CollectItemEvent).AddTo(disposable);
            second = Second;
            InitializeTaskItems();
        }


        protected override void ActionsOnUnapply()
        {
            disposable.Dispose();

            foreach (var collectableItem in activeTargets)
            {
                targetMarkerService.MarkForRemove(collectableItem.Center);
            }

            activeTargets.Clear();
        }


        protected override void ActionsOnTick(float dt)
        {
            second -= dt;
            if (second <= 0)
            {
                AudioService.I.PlaySfx(SfxType.ObjectFinderBooster);
                second = Second;
            }
        }


        private void InitializeTaskItems()
        {
            InitTaskCollectables();

            int count = Math.Min(tasksByDistance.Count, (int)model.BoostedValue);

            for (int i = 0; i < count; i++)
            {
                AddCollectableToMarkers(tasksByDistance[i].Item1);
            }
        }


        private void InitTaskCollectables()
        {
            tasksByDistance.Clear();
            List<CollectableType> tasks = taskManager.GetCurrentTasks();
            var groupedItems = itemCollectManager.GetItems();
            Vector3 characterPos = characterManager.GetPosition();

            foreach (CollectableType task in tasks)
            {
                if (groupedItems.TryGetValue(task, out IntHashMap<CollectableItem> map))
                {
                    foreach (var idx in map)
                    {
                        var item = map.GetValueByIndex(idx);
                        var distanceSqr = (item.Center.position - characterPos).sqrMagnitude;
                        tasksByDistance.Add((item, distanceSqr));
                    }
                }
            }

            tasksByDistance.Sort((l, r) => l.Item2.CompareTo(r.Item2));
        }


        private void AddCollectableToMarkers(CollectableItem collectable)
        {
            CollectablesData configData = collectablesConfig.Get(collectable.CollectableType);
            Sprite icon = spriteAtlasService.GetFromMain(configData.iconName);
            targetMarkerService.AddTarget(collectable.Center, icon);
            activeTargets.Add(collectable);
        }


        private void CollectItemEvent(CollectableItem collectable)
        {
            if (!activeTargets.Remove(collectable))
                return;

            targetMarkerService.MarkForRemove(collectable.Center);

            InitTaskCollectables();

            foreach (var entry in tasksByDistance)
            {
                if (!activeTargets.Contains(entry.Item1))
                {
                    AddCollectableToMarkers(entry.Item1);
                    break;
                }
            }
        }
    }
}