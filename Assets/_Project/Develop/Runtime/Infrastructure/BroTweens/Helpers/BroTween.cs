using System;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.BroTweens
{
    /// <summary>
    /// Complex, not default methods
    /// </summary>
    public partial class BroTween
    {
        public const string Tag = "[BroTween]";
        
        public static BroTweenBase Bounce(Transform target)
        {
            ButtonBounceParams bounceConfig = BroTweenService.I.Config.buttonBounce;
            return ScaleByCurve(target, Vector3.one, bounceConfig.duration, bounceConfig.curve);
        }

        
        public static BroTweenSafe DelayedCall(in float delay, in Action callback)
        {
            BroTweenSafe call = Sequence().InsertCallback(in delay, in callback).SetUpdate(true).ToSafe();
            call.Play();
            return call;
        }
        

        /// <summary>
        /// WARNING: Do not forget to call Play.
        /// </summary>
        public static BroSequence ClickBounceWithCallBack(Button button, Transform bounceTarget, in Action callback, in bool callbackOnEnd = false)
        {
            ButtonBounceParams bounceConfig = BroTweenService.I.Config.buttonBounce;
            float calcDelay = !callbackOnEnd ? bounceConfig.callbackDelay : bounceConfig.duration;

            return ClickBounceWithCallBack(button, bounceTarget, in callback, bounceConfig.duration, calcDelay, bounceConfig.curve);
        }


        /// <summary>
        /// Zero alloc when create new delegate callback.
        /// WARNING: Do not forget to call Play.
        /// </summary>
        public static BroSequence ClickBounceWithCallBack<T>(Button button, Transform bounceTarget, T callbackOwner, in Action<T> callback, in bool callbackOnEnd = false)
            where T : class
        {
            ButtonBounceParams bounceConfig = BroTweenService.I.Config.buttonBounce;
            float calcDelay = !callbackOnEnd ? bounceConfig.callbackDelay : bounceConfig.duration;
            return ClickBounceWithCallBack(button, bounceTarget, callbackOwner, in callback, bounceConfig.duration, calcDelay, bounceConfig.curve);
        }


        /// <summary>
        /// WARNING: Do not forget to call Play.
        /// </summary>
        public static BroSequence ClickBounceWithCallBack(Button button,
                                                             Transform bounceTarget,
                                                             in Action callback,
                                                             in float duration,
                                                             in float callbackDelay,
                                                             AnimationCurve curve)
        {
            BroSequence sequence = Sequence().SetUpdate(true);

            ScaleByCurveTween bounce = ScaleByCurve(bounceTarget, Vector3.one, duration, curve);
            sequence.InsertCallback(0, button, targetBtn => targetBtn.enabled = false);
            sequence.Append(bounce);

            sequence.InsertCallback(callbackDelay, in callback);
            sequence.AppendCallback(button, targetBtn => targetBtn.enabled = true);
            sequence.OnComplete(bounceTarget, targetBtn => targetBtn.localScale = Vector3.one);
            return sequence;
        }


        /// <summary>
        /// Zero alloc when create new delegate callback.
        /// WARNING: Do not forget to call Play.
        /// </summary>
        public static BroSequence ClickBounceWithCallBack<T>(Button button, Transform bounceTarget, T callbackOwner, in Action<T> callback,
                                                                in float duration, in float callbackDelay, AnimationCurve curve) where T : class
        {
            BroSequence sequence = Sequence().SetUpdate(true);

            ScaleByCurveTween bounce = ScaleByCurve(bounceTarget, Vector3.one, duration, curve);
            sequence.InsertCallback(0, button, targetBtn => targetBtn.enabled = false);
            sequence.Append(bounce);

            sequence.InsertCallback(callbackDelay, target: callbackOwner, in callback);
            sequence.AppendCallback(button, targetBtn => targetBtn.enabled = true);
            sequence.OnComplete(bounceTarget, targetBtn => targetBtn.localScale = Vector3.one);
            //Debug.Log("GetClickBounceWithCallBack");
            return sequence;
        }
    }
}