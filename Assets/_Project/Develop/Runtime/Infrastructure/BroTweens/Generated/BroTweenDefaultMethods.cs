using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.BroTweens
{
    //Generated
    public partial class BroTween
    {

#region Kill

        public static void Kill<T>(ref T t, bool withCallback = false) where T : BroTweenBase
        {
            t?.Kill_Internal(withCallback);
            t = null;
        }

#endregion

#region AnchoredPosition

        public static AnchoredPositionTween AnchoredPosition() => BroTweenService.I.Pools.AnchoredPositionTween.Get();

        public static AnchoredPositionTween AnchoredPosition(RectTransform target, in Vector2 to, in float duration)
        {
            var t = BroTweenService.I.Pools.AnchoredPositionTween.Get();
            t.SetParams(target, in to, in duration);
            return t;
        }

        public static AnchoredPositionTween AnchoredPosition(RectTransform target, in Vector2 from, in Vector2 to, in float duration)
        {
            var t = BroTweenService.I.Pools.AnchoredPositionTween.Get();
            t.SetParams(target, in from, in to, in duration);
            return t;
        }

#endregion

#region Sequence

        public static BroSequence Sequence() => BroTweenService.I.Pools.BroSequence.Get();

#endregion

#region SequenceLoop

        public static BroSequenceLoop SequenceLoop() => BroTweenService.I.Pools.BroSequenceLoop.Get();

        public static BroSequenceLoop SequenceLoop(in int repeatCount, in LoopType loopType = LoopType.Restart)
        {
            var t = BroTweenService.I.Pools.BroSequenceLoop.Get();
            t.SetParams(in repeatCount, in loopType);
            return t;
        }

        public static BroSequenceLoop SequenceLoop(in float betweenDelay, in int repeatCount, in LoopType loopType = LoopType.Restart)
        {
            var t = BroTweenService.I.Pools.BroSequenceLoop.Get();
            t.SetParams(in betweenDelay, in repeatCount, in loopType);
            return t;
        }

#endregion

#region ColorImage

        public static ColorImageTween ColorImage() => BroTweenService.I.Pools.ColorImageTween.Get();

        public static ColorImageTween ColorImage(Image target, in Color to, in float duration)
        {
            var t = BroTweenService.I.Pools.ColorImageTween.Get();
            t.SetParams(target, in to, in duration);
            return t;
        }

        public static ColorImageTween ColorImage(Image target, in Color from, in Color to, in float duration)
        {
            var t = BroTweenService.I.Pools.ColorImageTween.Get();
            t.SetParams(target, in from, in to, in duration);
            return t;
        }

#endregion

#region ColorText

        public static ColorTextTween ColorText() => BroTweenService.I.Pools.ColorTextTween.Get();

        public static ColorTextTween ColorText(TMP_Text target, in Color to, in float duration)
        {
            var t = BroTweenService.I.Pools.ColorTextTween.Get();
            t.SetParams(target, in to, in duration);
            return t;
        }

        public static ColorTextTween ColorText(TMP_Text target, in Color from, in Color to, in float duration)
        {
            var t = BroTweenService.I.Pools.ColorTextTween.Get();
            t.SetParams(target, in from, in to, in duration);
            return t;
        }

#endregion

#region FadeCanvasGroup

        public static FadeCanvasGroupTween FadeCanvasGroup() => BroTweenService.I.Pools.FadeCanvasGroupTween.Get();

        public static FadeCanvasGroupTween FadeCanvasGroup(CanvasGroup target, in float to, in float duration)
        {
            var t = BroTweenService.I.Pools.FadeCanvasGroupTween.Get();
            t.SetParams(target, in to, in duration);
            return t;
        }

        public static FadeCanvasGroupTween FadeCanvasGroup(CanvasGroup target, in float from, in float to, in float duration)
        {
            var t = BroTweenService.I.Pools.FadeCanvasGroupTween.Get();
            t.SetParams(target, in from, in to, in duration);
            return t;
        }

#endregion

#region FadeImage

        public static FadeImageTween FadeImage() => BroTweenService.I.Pools.FadeImageTween.Get();

        public static FadeImageTween FadeImage(Image target, in float to, in float duration)
        {
            var t = BroTweenService.I.Pools.FadeImageTween.Get();
            t.SetParams(target, in to, in duration);
            return t;
        }

        public static FadeImageTween FadeImage(Image target, in float from, in float to, in float duration)
        {
            var t = BroTweenService.I.Pools.FadeImageTween.Get();
            t.SetParams(target, in from, in to, in duration);
            return t;
        }

#endregion

#region FadeText

        public static FadeTextTween FadeText() => BroTweenService.I.Pools.FadeTextTween.Get();

        public static FadeTextTween FadeText(TMP_Text target, in float to, in float duration)
        {
            var t = BroTweenService.I.Pools.FadeTextTween.Get();
            t.SetParams(target, in to, in duration);
            return t;
        }

        public static FadeTextTween FadeText(TMP_Text target, in float from, in float to, in float duration)
        {
            var t = BroTweenService.I.Pools.FadeTextTween.Get();
            t.SetParams(target, in from, in to, in duration);
            return t;
        }

#endregion

#region Float

        public static FloatTween Float() => BroTweenService.I.Pools.FloatTween.Get();

        public static FloatTween Float(in Action<float> setter, in float duration)
        {
            var t = BroTweenService.I.Pools.FloatTween.Get();
            t.SetParams(in setter, in duration);
            return t;
        }

        public static FloatTween Float(in Action<float> setter, in float from, in float to, in float duration)
        {
            var t = BroTweenService.I.Pools.FloatTween.Get();
            t.SetParams(in setter, in from, in to, in duration);
            return t;
        }

        public static FloatTween Float<T>(T target, in Action<T, float> setter, in float duration) where T : class
        {
            var t = BroTweenService.I.Pools.FloatTween.Get();
            t.SetParams(target, in setter, in duration);
            return t;
        }

        public static FloatTween Float<T>(T target, in Action<T, float> setter, in float from, in float to, in float duration) where T : class
        {
            var t = BroTweenService.I.Pools.FloatTween.Get();
            t.SetParams(target, in setter, in from, in to, in duration);
            return t;
        }

        public static FloatTween Float(in float from, in float to, in float duration)
        {
            var t = BroTweenService.I.Pools.FloatTween.Get();
            t.SetParams(in from, in to, in duration);
            return t;
        }

#endregion

#region Int

        public static IntTween Int() => BroTweenService.I.Pools.IntTween.Get();

        public static IntTween Int(in int from, in int to, in float duration)
        {
            var t = BroTweenService.I.Pools.IntTween.Get();
            t.SetParams(in from, in to, in duration);
            return t;
        }

        public static IntTween Int(in Action<int> setter, in int from, in int to, in float duration)
        {
            var t = BroTweenService.I.Pools.IntTween.Get();
            t.SetParams(in setter, in from, in to, in duration);
            return t;
        }

        public static IntTween Int<T>(T target, in Action<T, int> setter, in int from, in int to, in float duration) where T : class
        {
            var t = BroTweenService.I.Pools.IntTween.Get();
            t.SetParams(target, in setter, in from, in to, in duration);
            return t;
        }

#endregion

#region PositionLocal

        public static PositionLocalTween PositionLocal() => BroTweenService.I.Pools.PositionLocalTween.Get();

        public static PositionLocalTween PositionLocal(Transform target, in Vector3 to, in float duration)
        {
            var t = BroTweenService.I.Pools.PositionLocalTween.Get();
            t.SetParams(target, in to, in duration);
            return t;
        }

        public static PositionLocalTween PositionLocal(Transform target, in Vector3 from, in Vector3 to, in float duration)
        {
            var t = BroTweenService.I.Pools.PositionLocalTween.Get();
            t.SetParams(target, in from, in to, in duration);
            return t;
        }

#endregion

#region Position

        public static PositionTween Position() => BroTweenService.I.Pools.PositionTween.Get();

        public static PositionTween Position(Transform target, in Vector3 to, in float duration)
        {
            var t = BroTweenService.I.Pools.PositionTween.Get();
            t.SetParams(target, in to, in duration);
            return t;
        }

        public static PositionTween Position(Transform target, in Vector3 from, in Vector3 to, in float duration)
        {
            var t = BroTweenService.I.Pools.PositionTween.Get();
            t.SetParams(target, in from, in to, in duration);
            return t;
        }

#endregion

#region PunchPosition

        public static PunchPositionTween PunchPosition() => BroTweenService.I.Pools.PunchPositionTween.Get();

        public static PunchPositionTween PunchPosition(Transform target, in Vector3 direction, in float duration, in bool isUiElement, in int countPerSec = 10, in float elasticity = 1f)
        {
            var t = BroTweenService.I.Pools.PunchPositionTween.Get();
            t.SetParams(target, in direction, in duration, in isUiElement, in countPerSec, in elasticity);
            return t;
        }

        public static PunchPositionTween PunchPosition(Transform target, in Vector3 defaultLocalPos, Vector3 direction, in float duration, in bool isUiElement, in int countPerSec = 10, in float elasticity = 1f)
        {
            var t = BroTweenService.I.Pools.PunchPositionTween.Get();
            t.SetParams(target, in defaultLocalPos, direction, in duration, in isUiElement, in countPerSec, in elasticity);
            return t;
        }

        public static PunchPositionTween PunchPosition(Transform target, List<VectorStep> steps)
        {
            var t = BroTweenService.I.Pools.PunchPositionTween.Get();
            t.SetParams(target, steps);
            return t;
        }

#endregion

#region PunchRotation

        public static PunchRotationTween PunchRotation() => BroTweenService.I.Pools.PunchRotationTween.Get();

        public static PunchRotationTween PunchRotation(Transform target, in Vector3 direction, in float duration, in int countPerSec = 10, in float elasticity = 1f)
        {
            var t = BroTweenService.I.Pools.PunchRotationTween.Get();
            t.SetParams(target, in direction, in duration, in countPerSec, in elasticity);
            return t;
        }

        public static PunchRotationTween PunchRotation(Transform target, in Vector3 defaultLocalRotation, in Vector3 direction, in float duration, in int countPerSec = 10, in float elasticity = 1f)
        {
            var t = BroTweenService.I.Pools.PunchRotationTween.Get();
            t.SetParams(target, in defaultLocalRotation, in direction, in duration, in countPerSec, in elasticity);
            return t;
        }

        public static PunchRotationTween PunchRotation(Transform target, List<VectorStep> steps)
        {
            var t = BroTweenService.I.Pools.PunchRotationTween.Get();
            t.SetParams(target, steps);
            return t;
        }

#endregion

#region PunchScale

        public static PunchScaleTween PunchScale() => BroTweenService.I.Pools.PunchScaleTween.Get();

        public static PunchScaleTween PunchScale(Transform target, in Vector3 direction, in float duration, in int countPerSec = 10, in float elasticity = 1f)
        {
            var t = BroTweenService.I.Pools.PunchScaleTween.Get();
            t.SetParams(target, in direction, in duration, in countPerSec, in elasticity);
            return t;
        }

        public static PunchScaleTween PunchScale(Transform target, in Vector3 defaultScale, in Vector3 direction, in float duration, in int countPerSec = 10, in float elasticity = 1f)
        {
            var t = BroTweenService.I.Pools.PunchScaleTween.Get();
            t.SetParams(target, in defaultScale, in direction, in duration, in countPerSec, in elasticity);
            return t;
        }

        public static PunchScaleTween PunchScale(Transform target, List<VectorStep> steps)
        {
            var t = BroTweenService.I.Pools.PunchScaleTween.Get();
            t.SetParams(target, steps);
            return t;
        }

#endregion

#region RectSizeDelta

        public static RectSizeDeltaTween RectSizeDelta() => BroTweenService.I.Pools.RectSizeDeltaTween.Get();

        public static RectSizeDeltaTween RectSizeDelta(RectTransform target, in Vector2 to, in float duration)
        {
            var t = BroTweenService.I.Pools.RectSizeDeltaTween.Get();
            t.SetParams(target, in to, in duration);
            return t;
        }

        public static RectSizeDeltaTween RectSizeDelta(RectTransform target, in Vector2 from, in Vector2 to, in float duration)
        {
            var t = BroTweenService.I.Pools.RectSizeDeltaTween.Get();
            t.SetParams(target, in from, in to, in duration);
            return t;
        }

#endregion

#region RotationLocal

        public static RotationLocalTween RotationLocal() => BroTweenService.I.Pools.RotationLocalTween.Get();

        public static RotationLocalTween RotationLocal(Transform target, in Vector3 to, in float duration, RotateMode mode = RotateMode.Shortest)
        {
            var t = BroTweenService.I.Pools.RotationLocalTween.Get();
            t.SetParams(target, in to, in duration, mode);
            return t;
        }

        public static RotationLocalTween RotationLocal(Transform target, in Vector3 from, in Vector3 to, in float duration, RotateMode mode = RotateMode.Shortest)
        {
            var t = BroTweenService.I.Pools.RotationLocalTween.Get();
            t.SetParams(target, in from, in to, in duration, mode);
            return t;
        }

        public static RotationLocalTween RotationLocal(Transform target, in Quaternion to, in float duration)
        {
            var t = BroTweenService.I.Pools.RotationLocalTween.Get();
            t.SetParams(target, in to, in duration);
            return t;
        }

        public static RotationLocalTween RotationLocal(Transform target, in Quaternion from, in Quaternion to, in float duration)
        {
            var t = BroTweenService.I.Pools.RotationLocalTween.Get();
            t.SetParams(target, in from, in to, in duration);
            return t;
        }

#endregion

#region Rotation

        public static RotationTween Rotation() => BroTweenService.I.Pools.RotationTween.Get();

        public static RotationTween Rotation(Transform target, in Vector3 to, in float duration, RotateMode mode = RotateMode.Shortest)
        {
            var t = BroTweenService.I.Pools.RotationTween.Get();
            t.SetParams(target, in to, in duration, mode);
            return t;
        }

        public static RotationTween Rotation(Transform target, in Vector3 from, in Vector3 to, in float duration, RotateMode mode = RotateMode.Shortest)
        {
            var t = BroTweenService.I.Pools.RotationTween.Get();
            t.SetParams(target, in from, in to, in duration, mode);
            return t;
        }

        public static RotationTween Rotation(Transform target, in Quaternion to, in float duration)
        {
            var t = BroTweenService.I.Pools.RotationTween.Get();
            t.SetParams(target, in to, in duration);
            return t;
        }

        public static RotationTween Rotation(Transform target, in Quaternion from, in Quaternion to, in float duration)
        {
            var t = BroTweenService.I.Pools.RotationTween.Get();
            t.SetParams(target, in from, in to, in duration);
            return t;
        }

#endregion

#region ScaleByCurve

        public static ScaleByCurveTween ScaleByCurve() => BroTweenService.I.Pools.ScaleByCurveTween.Get();

        public static ScaleByCurveTween ScaleByCurve(Transform target, in float duration, AnimationCurve curve)
        {
            var t = BroTweenService.I.Pools.ScaleByCurveTween.Get();
            t.SetParams(target, in duration, curve);
            return t;
        }

        public static ScaleByCurveTween ScaleByCurve(Transform target, in Vector3 defaultScale, in float duration, AnimationCurve curve)
        {
            var t = BroTweenService.I.Pools.ScaleByCurveTween.Get();
            t.SetParams(target, in defaultScale, in duration, curve);
            return t;
        }

#endregion

#region Scale

        public static ScaleTween Scale() => BroTweenService.I.Pools.ScaleTween.Get();

        public static ScaleTween Scale(Transform target, in Vector3 to, in float duration)
        {
            var t = BroTweenService.I.Pools.ScaleTween.Get();
            t.SetParams(target, in to, in duration);
            return t;
        }

        public static ScaleTween Scale(Transform target, in Vector3 from, in Vector3 to, in float duration)
        {
            var t = BroTweenService.I.Pools.ScaleTween.Get();
            t.SetParams(target, in from, in to, in duration);
            return t;
        }

#endregion

#region ShakePosition

        public static ShakePositionTween ShakePosition() => BroTweenService.I.Pools.ShakePositionTween.Get();

        public static ShakePositionTween ShakePosition(Transform target, in Vector3 strength, in float duration, in bool isUiElement, in int countPerSec = 10, in float angleRandomness = 90f, in bool fadeOut = true, in bool isFullRandomness = true)
        {
            var t = BroTweenService.I.Pools.ShakePositionTween.Get();
            t.SetParams(target, in strength, in duration, in isUiElement, in countPerSec, in angleRandomness, in fadeOut, in isFullRandomness);
            return t;
        }

        public static ShakePositionTween ShakePosition(Transform target, in Vector3 defaultLocalPos, Vector3 strength, in float duration, in bool isUiElement, in int countPerSec = 10, in float angleRandomness = 90f, in bool fadeOut = true, in bool isFullRandomness = true)
        {
            var t = BroTweenService.I.Pools.ShakePositionTween.Get();
            t.SetParams(target, in defaultLocalPos, strength, in duration, in isUiElement, in countPerSec, in angleRandomness, in fadeOut, in isFullRandomness);
            return t;
        }

        public static ShakePositionTween ShakePosition(Transform target, List<VectorStep> steps)
        {
            var t = BroTweenService.I.Pools.ShakePositionTween.Get();
            t.SetParams(target, steps);
            return t;
        }

#endregion

#region ShakeRotation

        public static ShakeRotationTween ShakeRotation() => BroTweenService.I.Pools.ShakeRotationTween.Get();

        public static ShakeRotationTween ShakeRotation(Transform target, in Vector3 strength, in float duration, in int countPerSec = 10, in float angleRandomness = 90f, in bool fadeOut = true, in bool isFullRandomness = true)
        {
            var t = BroTweenService.I.Pools.ShakeRotationTween.Get();
            t.SetParams(target, in strength, in duration, in countPerSec, in angleRandomness, in fadeOut, in isFullRandomness);
            return t;
        }

        public static ShakeRotationTween ShakeRotation(Transform target, in Vector3 defaultLocalRotation, in Vector3 strength, in float duration, in int countPerSec = 10, in float angleRandomness = 90f, in bool fadeOut = true, in bool isFullRandomness = true)
        {
            var t = BroTweenService.I.Pools.ShakeRotationTween.Get();
            t.SetParams(target, in defaultLocalRotation, in strength, in duration, in countPerSec, in angleRandomness, in fadeOut, in isFullRandomness);
            return t;
        }

        public static ShakeRotationTween ShakeRotation(Transform target, List<VectorStep> steps)
        {
            var t = BroTweenService.I.Pools.ShakeRotationTween.Get();
            t.SetParams(target, steps);
            return t;
        }

#endregion

#region SliderImageMove

        public static SliderImageMoveTween SliderImageMove() => BroTweenService.I.Pools.SliderImageMoveTween.Get();

        public static SliderImageMoveTween SliderImageMove(Image target, in float from, in float to, in float duration)
        {
            var t = BroTweenService.I.Pools.SliderImageMoveTween.Get();
            t.SetParams(target, in from, in to, in duration);
            return t;
        }

#endregion

#region SliderMove

        public static SliderMoveTween SliderMove() => BroTweenService.I.Pools.SliderMoveTween.Get();

        public static SliderMoveTween SliderMove(Slider target, in float from, in float to, in float duration)
        {
            var t = BroTweenService.I.Pools.SliderMoveTween.Get();
            t.SetParams(target, in from, in to, in duration);
            return t;
        }

#endregion
    }
}
