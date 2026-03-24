using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Widgets;
using Features.WinStreak;
using Infrastructure.BroTweens;
using Infrastructure.Collections;
using Infrastructure.Configs;
using Infrastructure.Pool;
using UnityEngine;

namespace Features.ScoringVisualize
{
    public class ScoringVisualizeService
    {
        public event Action<ScoringFeature> OnComplete;
        private readonly PoolService poolService;
        private readonly WinStreakStateController winStreakStateController;
        private readonly WidgetManager widgetManager;
        private readonly ScoringVisualizeConfig config;
        private readonly List<ScoringScheduleTask> scheduledTasks = new();
        private ScoringIconPool iconPool;
        private ScoringTextPool textPool;
        private ParticlesPool particlePool;

        private bool isInit;

        public ScoringVisualizeService(ConfigProvider configProvider, PoolService poolService, WinStreakStateController winStreakStateController, WidgetManager widgetManager)
        {
            this.config = configProvider.ScoringVisualizeConfig;
            this.poolService = poolService;
            this.winStreakStateController = winStreakStateController;
            this.widgetManager = widgetManager;
        }

        public void Initialize()
        {
            if (isInit)
                return;

            iconPool = poolService.Get<ScoringIconPool>();
            textPool = poolService.Get<ScoringTextPool>();
            particlePool = poolService.Get<ParticlesPool>();

            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            scheduledTasks.Clear();
            isInit = false;
        }

        public async UniTask ExecuteScheduledAsync(CancellationToken cancellationToken)
        {
            if (scheduledTasks.Count == 0)
                return;

            await UniTask.WaitForSeconds(config.animationsBeginDelay, true, cancellationToken: cancellationToken);

            using var uniTasks = PooledList<UniTask>.Get();

            foreach (var task in scheduledTasks)
            {
                var seq = CreateSequence(task.feature, task.addScore);
                seq.Play();
                uniTasks.Add(seq.ToUniTask(cancellationToken: cancellationToken));
            }

            scheduledTasks.Clear();

            await UniTask.WhenAll(uniTasks);
        }

        public void Schedule(ScoringFeature feature, int count)
        {
            scheduledTasks.Add(new ScoringScheduleTask(feature, count));
        }

        public bool IsScheduled(ScoringFeature feature)
        {
            foreach (var task in scheduledTasks)
            {
                if (task.feature == feature)
                    return true;
            }

            return false;
        }

        private BroTweenBase CreateSequence(ScoringFeature feature, int count)
        {
            var sequence = BroTween.Sequence().SetUpdate(true);

            if (!config.scoringData.TryGetValue(feature, out var configData))
            {
                Debug.LogWarning($"Couldn't find config data for feature: {feature}");
                return sequence;
            }

            if (!widgetManager.TryGetWidget(configData.widgetId, out IWidgetReadable widget))
            {
                Debug.LogWarning($"Couldn't find widget for feature: {feature}");
                return sequence;
            }

            if (!textPool.TryGetItem(out var textView))
            {
                Debug.LogWarning($"Couldn't find textView for feature: {feature}");
                return sequence;
            }

            var startDelay = configData.startDelay;

            var viewCount = Math.Min(count, config.maxIconCount);

            Vector2 scoringTextOffset = default;
            Vector2 scoringViewPos = default;

            for (int i = 0; i < viewCount; i++)
            {
                if (!iconPool.TryGetItem(feature, out var iconView))
                {
                    Debug.LogError($"Couldn't find iconView for feature {feature}");
                    return sequence;
                }

                var viewPos = GetViewMovePositions(widget);
                var iconStartTime = i * UnityEngine.Random.Range(config.betweenDelayMin, config.betweenDelayMax);
                iconStartTime += startDelay;

                Action completeCallback = null;
                var isLastItem = i == viewCount - 1;

                if (isLastItem)
                    completeCallback = () => OnComplete?.Invoke(feature);

                var iconTween = iconView.GetTween(config.iconAnimParams, viewPos.from, viewPos.to, completeCallback);
                var iconEndTime = iconStartTime + iconTween.Duration;

                sequence.Insert(iconStartTime, iconTween);
                sequence.Insert(iconEndTime, GetWidgetScaleTween(widget));

                if (i == 0)
                {
                    scoringTextOffset = iconView.ScoringTextTargetOffset;
                    scoringViewPos = viewPos.from;
                }

                if (isLastItem)
                {
                    sequence.InsertCallback(iconEndTime, () => PlaySplashFx(widget.IconPosition));
                }
            }

            var isWinStreak = winStreakStateController.IsMaxLevel;
            var baseCount = isWinStreak ? count / winStreakStateController.RewardMultiplier : count;
            var baseCountTxt = $"+{baseCount.ToString()}";
            var wsCount = $"+{count.ToString()}";

            var textPos = GetTextPositions(widget, scoringViewPos, scoringTextOffset);

            textView.SetPosition(textPos.pos);
            sequence.Insert(startDelay,
                            textView.GetTween(particlePool, config.textAnimParams, config.winStreakAnimParams, baseCountTxt, isWinStreak, wsCount, textPos.isLabelFromLeft));

            return sequence;
        }

        private BroTweenBase GetWidgetScaleTween(IWidgetReadable widget)
        {
            return BroTween.ScaleByCurve(widget.RectTransform, config.widgetAnimParams.scaleDuration, config.widgetAnimParams.scaleCurve);
        }

        private void PlaySplashFx(Vector3 pos)
        {
            if (particlePool.TryGetItem(PoolableParticleType.CurrencyCountSparks, out var splashFx))
            {
                splashFx.transform.position = pos;
            }
        }

        private (Vector2 from, Vector2 to) GetViewMovePositions(IWidgetReadable widget)
        {
            if (!widgetManager.TryGetWidgetRootSide(widget.Id, out var rootSide))
            {
                Debug.LogWarning($"Couldn't find widget rootSide: {widget.Id}");
                return (Vector2.zero, Vector2.zero);
            }

            var viewOffset = widget.ScoringTargetOffset;
            if (rootSide == RootSide.Right)
            {
                viewOffset.x = -viewOffset.x;
            }

            var rX = UnityEngine.Random.Range(config.minSpawnPosition.x, config.maxSpawnPosition.x);
            var rY = UnityEngine.Random.Range(config.minSpawnPosition.y, config.maxSpawnPosition.y);
            var from = new Vector2(widget.IconPosition.x + viewOffset.x + rX, widget.IconPosition.y + viewOffset.y + rY);

            var to = widget.IconPosition;

            return (from, to);
        }

        private (Vector2 pos, bool isLabelFromLeft) GetTextPositions(IWidgetReadable widget, Vector2 itemViewPos, Vector2 offsetFromItemView)
        {
            if (!widgetManager.TryGetWidgetRootSide(widget.Id, out var rootSide))
            {
                Debug.LogWarning($"Couldn't find widget rootSide: {widget.Id}");
                return (Vector2.zero, false);
            }

            if (rootSide == RootSide.Right)
            {
                offsetFromItemView.x = -offsetFromItemView.x;
            }

            var from = new Vector2(itemViewPos.x + offsetFromItemView.x, itemViewPos.y + offsetFromItemView.y);

            return (from, rootSide != RootSide.Left);
        }
    }
}