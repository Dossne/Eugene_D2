using Cysharp.Threading.Tasks;
using Infrastructure.Pool;
using System;
using System.Threading;
using Infrastructure.BroTweens;
using UnityEngine;


namespace Features.CurrencyView
{
    public class PoolableCurrencyModel : MonoBehaviour, IPoolableObject<PoolableCurrencyModel>
    {
        public event Action<PoolableCurrencyModel> RequestReleaseToPool;
        public event Action<PoolableCurrencyModel> ObserveDestroy;

        private CurrencyModelFxData fxData;

        private GameObject thisGameObject;
        private BroTweenSafe flyTween;
        private Vector3 startPosition;
        private Vector3 endPosition;
        private Action callback;

        [TriInspector.ShowInInspector] public bool IsInPool { get; set; }
        [TriInspector.ShowInInspector] public int PooledId { get; set; }


        private void OnDisable()
        {
            ReleaseToPool();
        }


        private void OnDestroy()
        {
            ObserveDestroy?.Invoke(this);
            ObserveDestroy = null;
            RequestReleaseToPool = null;
            flyTween.Kill();
        }


        public UniTask FlyAsync(CurrencyModelFxData fxData, Vector3 startPosition, Vector3 endPosition, Action callback, CancellationToken cancellationToken)
        {
            this.fxData = fxData;
            this.startPosition = startPosition;
            this.endPosition = endPosition;
            this.callback = callback;

            return CreateFlySequenceAsync(cancellationToken);
        }


        void IPoolableObject<PoolableCurrencyModel>.OnCreate()
        {
            thisGameObject = gameObject;
        }


        void IPoolableObject<PoolableCurrencyModel>.OnPoolGet()
        {
            SetObjectActive(true);
        }


        void IPoolableObject<PoolableCurrencyModel>.OnPoolRelease()
        {
            SetObjectActive(false);
            flyTween.Kill();
        }


        void IPoolableObject<PoolableCurrencyModel>.SetParent(Transform parent)
        {
            if (thisGameObject.transform.parent != parent)
                thisGameObject.transform.SetParent(parent);
        }


        void IPoolableObject<PoolableCurrencyModel>.Destroy()
        {
            Destroy(gameObject);
        }


        private void ReleaseToPool()
        {
            RequestReleaseToPool?.Invoke(this);
        }


        private void SetObjectActive(bool isActive)
        {
            if (thisGameObject.activeSelf != isActive)
                thisGameObject.SetActive(isActive);
        }


        private UniTask CreateFlySequenceAsync(CancellationToken cancellationToken)
        {
            transform.position = startPosition;
            transform.localScale = Vector3.one * fxData.animParams.scaleCurve.Evaluate(0);
            flyTween.Kill();

            BroSequence seq = BroTween.Sequence()
                                      .SetUpdate(true)
                                      .Insert(0, GetMoveTween())
                                      .Insert(0, GetScaleTween())
                                      .Insert(0, GetRotateTween())
                                      .AppendCallback(OnEndFly)
                                      .OnComplete(ReleaseToPool);

            flyTween = seq.ToSafe();
            flyTween.Play();
            
            return flyTween.ToUniTask(BroTweenCancelBehaviour.KillWithCallback, cancellationToken: cancellationToken);
        }


        private void OnEndFly()
        {
            callback?.Invoke();
        }


        private BroTweenBase GetScaleTween()
        {
            return BroTween.ScaleByCurve(transform, Vector3.one, fxData.animParams.scaleDuration, fxData.animParams.scaleCurve);
        }


        private BroTweenBase GetMoveTween()
        {
            return BroTween.Float(MoveByCurveXY, fxData.animParams.moveDuration)
                           .SetEase(fxData.animParams.movementEaseCurve);
        }


        private BroTweenBase GetRotateTween()
        {
            return BroTween.RotationLocal(transform, new Vector3(0.0f, fxData.animParams.rotateSpeedY, 0.0f), fxData.animParams.rotateDuration, RotateMode.Full)
                           .SetEase(fxData.animParams.rotateEaseCurve);
        }


        private void MoveByCurveXY(float t)
        {
            float basePosX = Mathf.LerpUnclamped(startPosition.x, endPosition.x, t);
            float basePosY = Mathf.LerpUnclamped(startPosition.y, endPosition.y, fxData.animParams.curveY.Evaluate(t));
            transform.position = new Vector3(basePosX, basePosY, transform.position.z);
        }
    }
}