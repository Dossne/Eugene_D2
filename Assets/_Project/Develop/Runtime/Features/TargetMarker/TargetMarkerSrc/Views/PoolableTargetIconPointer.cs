using System;
using Infrastructure.BroTweens;
using Infrastructure.Pool;
using UnityEngine;
using UnityEngine.UI;

namespace Features.TargetMarker
{
    public class PoolableTargetIconPointer : MonoBehaviour, IPoolableObject<PoolableTargetIconPointer>
    {
        public event Action<PoolableTargetIconPointer> RequestReleaseToPool;
        public event Action<PoolableTargetIconPointer> ObserveDestroy;

        [Header("Components")]
        [SerializeField] private RectTransform moveRoot;
        [SerializeField] private RectTransform rotatingRoot;
        [SerializeField] private Image targetIcon;

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


        public void Construct(Sprite icon)
        {
            targetIcon.sprite = icon;
        }


        public void SetLocalPosition(Vector2 position)
        {
            moveRoot.localPosition = position;
        }


        public void SetRotation(Quaternion rot)
        {
            rotatingRoot.rotation = rot;
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

            if(!scaleTween.TryRewind())
                CreateScaleTween();
            
            scaleTween.Play();
        }


        void IPoolableObject<PoolableTargetIconPointer>.OnCreate()
        {
            thisGameObject = gameObject;
        }


        void IPoolableObject<PoolableTargetIconPointer>.OnPoolGet()
        {
        }


        void IPoolableObject<PoolableTargetIconPointer>.OnPoolRelease()
        {
            SetObjectActive(false);
        }


        void IPoolableObject<PoolableTargetIconPointer>.SetParent(Transform parent)
        {
            if (thisGameObject.transform.parent != parent)
                thisGameObject.transform.SetParent(parent);
        }


        void IPoolableObject<PoolableTargetIconPointer>.Destroy()
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