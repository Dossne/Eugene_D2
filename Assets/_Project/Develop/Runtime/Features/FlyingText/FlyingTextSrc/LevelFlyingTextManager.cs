using System.Collections.Generic;
using Features.Collectables;
using Features.Events;
using Features.LevelUp;
using Infrastructure.Pool.FloatingText;
using Infrastructure.Configs;
using Infrastructure.InputControl;
using Infrastructure.Localization;
using Infrastructure.Pool;
using Infrastructure.Utilities;
using R3;
using UnityEngine;

namespace Features.FlyingText
{
    public class LevelFlyingTextManager
    {
        private readonly PoolService poolService;
        private readonly FlyingTextConfig flyingTextConfig;
        private readonly InputService inputService;
        private readonly Dictionary<int /*points*/, CounterTextData> counterMapped;
        private readonly CollectItemEvent collectItemEvent;
        private readonly LevelProgressChangeEvent levelProgressChangeEvent;
        private readonly CollectablesConfig collectablesConfig;

        private FloatingTextPool pool;
        private FlyingTextTargetData textTargetData;
        private CounterTextData defaultData;
        private CompositeDisposable disposables;
        private bool isInit;


        public LevelFlyingTextManager(
            PoolService poolService,
            ConfigProvider configProvider,
            InputService inputService,
            CollectItemEvent collectItemEvent,
            LevelProgressChangeEvent levelProgressChangeEvent)
        {
            this.poolService = poolService;
            this.flyingTextConfig = configProvider.FlyingTextConfig;
            this.collectablesConfig = configProvider.CollectablesConfig;
            this.inputService = inputService;
            this.collectItemEvent = collectItemEvent;
            this.levelProgressChangeEvent = levelProgressChangeEvent;
            this.counterMapped = new Dictionary<int, CounterTextData>();
        }



        public void Initialize()
        {
            if (isInit)
                return;

            pool = poolService.Get<FloatingTextPool>();

            disposables = new();
            collectItemEvent.Subscribe(CollectItemEvent).AddTo(disposables);
            levelProgressChangeEvent.Subscribe(LevelChangeEvent).AddTo(disposables);
            textTargetData = flyingTextConfig.TextTargetConfig[0];

            InitializeConfigData();
            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            disposables.Dispose();
            counterMapped.Clear();
            pool = null;

            isInit = false;
        }

        
        private Vector2 GetCounterTextPos()
        {
            var result = Random.insideUnitCircle * new Vector2(textTargetData.counterRandPosX, textTargetData.counterRandPosY);
            result.y += textTargetData.counterTextHeight;
            result.x -= inputService.X * textTargetData.inputMultiplierX;
            return result;
        }


        private void InitializeConfigData()
        {
            foreach (var item in flyingTextConfig.FlyingTextData)
            {
                counterMapped.Add(item.points, item);
            }

            foreach (var item in flyingTextConfig.FlyingTextData)
            {
                defaultData = item;
                break;
            }
        }


        private void CollectItemEvent(CollectableItem item)
        {
            if (!collectablesConfig.TryGet(item.CollectableType, out CollectablesData data) || data.cost <= 0)
                return;

            if (!pool.TryGetItem(FloatingTextType.PointsCounter, out PoolableFloatingText text))
                return;
            
            CounterTextData counterTextData = counterMapped.GetValueOrDefault(data.cost, defaultData);
            float scale = counterTextData.textScale;
            Vector2 randomPos = GetCounterTextPos();
            text.Show($"+{data.cost.ToString()}", randomPos, Vector3.one * scale, true);
        }


        private void LevelChangeEvent(LevelProgressArgs args)
        {
            if (args.type != ActionType.Add)
                return;
            
            string text;

            switch (args.source)
            {
                case Source.ExpLevelUp:
                    text = LocalizationService.I.Get(LocKeys.Character.LevelUp, args.levelByCurrentXp.ToString());
                    break;
                case Source.SizeBooster:
                    text = LocalizationService.I.Get(LocKeys.Character.SizeUp);
                    break;

                default:
                    return;
            }

            if (!pool.TryGetItem(FloatingTextType.LevelUpSize, out PoolableFloatingText fText))
                return;
            
            fText.Show(text, new Vector2(0f, textTargetData.levelUpTextHeight), Vector3.one, true);
        }
    }
}