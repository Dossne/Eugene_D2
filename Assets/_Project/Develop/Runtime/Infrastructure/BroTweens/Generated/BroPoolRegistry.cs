using System;
using Infrastructure.Collections;

namespace Infrastructure.BroTweens
{
    //Generated
    [Serializable]
    public class BroPoolRegistry
    {
        [TriInspector.ShowInInspector] internal BroPool<AnchoredPositionTween> AnchoredPositionTween;
        [TriInspector.ShowInInspector] internal BroPool<BroCallback> BroCallback;
        [TriInspector.ShowInInspector] internal BroPool<BroSequence> BroSequence;
        [TriInspector.ShowInInspector] internal BroPool<BroSequenceItem> BroSequenceItem;
        [TriInspector.ShowInInspector] internal BroPool<BroSequenceLoop> BroSequenceLoop;
        [TriInspector.ShowInInspector] internal BroPool<ColorImageTween> ColorImageTween;
        [TriInspector.ShowInInspector] internal BroPool<ColorTextTween> ColorTextTween;
        [TriInspector.ShowInInspector] internal BroPool<FadeCanvasGroupTween> FadeCanvasGroupTween;
        [TriInspector.ShowInInspector] internal BroPool<FadeImageTween> FadeImageTween;
        [TriInspector.ShowInInspector] internal BroPool<FadeTextTween> FadeTextTween;
        [TriInspector.ShowInInspector] internal BroPool<FloatTween> FloatTween;
        [TriInspector.ShowInInspector] internal BroPool<IntTween> IntTween;
        [TriInspector.ShowInInspector] internal BroPool<PositionLocalTween> PositionLocalTween;
        [TriInspector.ShowInInspector] internal BroPool<PositionTween> PositionTween;
        [TriInspector.ShowInInspector] internal BroPool<PunchPositionTween> PunchPositionTween;
        [TriInspector.ShowInInspector] internal BroPool<PunchRotationTween> PunchRotationTween;
        [TriInspector.ShowInInspector] internal BroPool<PunchScaleTween> PunchScaleTween;
        [TriInspector.ShowInInspector] internal BroPool<RectSizeDeltaTween> RectSizeDeltaTween;
        [TriInspector.ShowInInspector] internal BroPool<RotationLocalTween> RotationLocalTween;
        [TriInspector.ShowInInspector] internal BroPool<RotationTween> RotationTween;
        [TriInspector.ShowInInspector] internal BroPool<ScaleByCurveTween> ScaleByCurveTween;
        [TriInspector.ShowInInspector] internal BroPool<ScaleTween> ScaleTween;
        [TriInspector.ShowInInspector] internal BroPool<ShakePositionTween> ShakePositionTween;
        [TriInspector.ShowInInspector] internal BroPool<ShakeRotationTween> ShakeRotationTween;
        [TriInspector.ShowInInspector] internal BroPool<SliderImageMoveTween> SliderImageMoveTween;
        [TriInspector.ShowInInspector] internal BroPool<SliderMoveTween> SliderMoveTween;

        private readonly FastList<IBroPool> pools = new();
        private readonly BroTweenConfig config;

        public BroPoolRegistry(BroTweenConfig config)
        {
            this.config = config;
        }

        public void Initialize()
        {
            AnchoredPositionTween = CreatePool<AnchoredPositionTween>();
            BroCallback = CreatePool<BroCallback>();
            BroSequence = CreatePool<BroSequence>();
            BroSequenceItem = CreatePool<BroSequenceItem>();
            BroSequenceLoop = CreatePool<BroSequenceLoop>();
            ColorImageTween = CreatePool<ColorImageTween>();
            ColorTextTween = CreatePool<ColorTextTween>();
            FadeCanvasGroupTween = CreatePool<FadeCanvasGroupTween>();
            FadeImageTween = CreatePool<FadeImageTween>();
            FadeTextTween = CreatePool<FadeTextTween>();
            FloatTween = CreatePool<FloatTween>();
            IntTween = CreatePool<IntTween>();
            PositionLocalTween = CreatePool<PositionLocalTween>();
            PositionTween = CreatePool<PositionTween>();
            PunchPositionTween = CreatePool<PunchPositionTween>();
            PunchRotationTween = CreatePool<PunchRotationTween>();
            PunchScaleTween = CreatePool<PunchScaleTween>();
            RectSizeDeltaTween = CreatePool<RectSizeDeltaTween>();
            RotationLocalTween = CreatePool<RotationLocalTween>();
            RotationTween = CreatePool<RotationTween>();
            ScaleByCurveTween = CreatePool<ScaleByCurveTween>();
            ScaleTween = CreatePool<ScaleTween>();
            ShakePositionTween = CreatePool<ShakePositionTween>();
            ShakeRotationTween = CreatePool<ShakeRotationTween>();
            SliderImageMoveTween = CreatePool<SliderImageMoveTween>();
            SliderMoveTween = CreatePool<SliderMoveTween>();
        }

        public void Deinitialize()
        {
            for (int i = 0; i < pools.length; i++)
            {
                pools[i].Deinitialize();
            }
            pools.Clear();
        }

        private BroPool<T> CreatePool<T>() where T : IBroPoolable, new()
        {
            var cfg = config.GetPoolParams(typeof(T));
            var p = new BroPool<T>(cfg.startCapacity);
            p.Prewarm(cfg.preWarmCount);
            pools.Add(p);
            return p;
        }
    }
}
