using System;
using Infrastructure.BroTweens;
using Infrastructure.Pool;
using UnityEngine;

namespace Features.TargetMarker
{
    public class PoolableTargetMarker : MonoBehaviour, IPoolableObject<PoolableTargetMarker>
    {
        public event Action<PoolableTargetMarker> RequestReleaseToPool;
        public event Action<PoolableTargetMarker> ObserveDestroy;

        [Header("Components")]
        [SerializeField] private RectTransform moveRoot;

        [Header("Scale")]
        [SerializeField] private ScaleFxData scaleFx;

        private BroTweenSafe scaleTween;
        private GameObject thisGameObject;

        [TriInspector.ShowInInspector] public bool IsInPool { get; set; }
        [TriInspector.ShowInInspector] public int PooledId { get; set; }


        private void OnDestroy()
        {
            ObserveDestroy?.Invoke(this);
            ObserveDestroy = null;
            RequestReleaseToPool = null;
            KillScaleAnimation();
        }


        public void SetPosition(Vector3 position)
        {
            moveRoot.position = position;
        }


        public void SetObjectActive(bool isActive)
        {
            if (thisGameObject.activeSelf != isActive)
                thisGameObject.SetActive(isActive);
        }


        public void ReleaseToPool()
        {
            KillScaleAnimation();
            RequestReleaseToPool?.Invoke(this);
        }


        public void PlayScaleAnimation()
        {
            moveRoot.localScale = Vector3.one * scaleFx.curve.Evaluate(0);
            if (!scaleTween.TryRewind())
                CreateScaleTween();

            scaleTween.Play();
        }


        void IPoolableObject<PoolableTargetMarker>.OnCreate()
        {
            thisGameObject = gameObject;
        }


        void IPoolableObject<PoolableTargetMarker>.OnPoolGet()
        {
        }


        void IPoolableObject<PoolableTargetMarker>.OnPoolRelease()
        {
            SetObjectActive(false);
        }


        void IPoolableObject<PoolableTargetMarker>.SetParent(Transform parent)
        {
            if (thisGameObject.transform.parent != parent)
                thisGameObject.transform.SetParent(parent);
        }


        void IPoolableObject<PoolableTargetMarker>.Destroy()
        {
            Destroy(gameObject);
        }


        private void KillScaleAnimation()
        {
            scaleTween.Kill();
        }


        private void CreateScaleTween()
        {
            scaleTween = BroTween.ScaleByCurve(moveRoot, Vector3.one, scaleFx.duration, scaleFx.curve).ToSafe();
        }
    }
}