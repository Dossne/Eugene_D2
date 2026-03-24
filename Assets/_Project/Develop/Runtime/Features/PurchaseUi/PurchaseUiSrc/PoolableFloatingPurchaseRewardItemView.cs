using Features.PurchaseUi;
using Features.RewardTrack;
using Infrastructure.Animations;
using Infrastructure.Pool;
using System;
using System.Collections;
using System.Collections.Generic;
using Infrastructure.Reward;
using UnityEngine;


namespace Features.PurchaseUi
{
    public class PoolableFloatingPurchaseRewardItemView : MonoBehaviour, IPoolableObject<PoolableFloatingPurchaseRewardItemView>
    {
        [SerializeField] private GameObject mainRoot;
        [SerializeField] private RectTransform mainRootRect;
        [SerializeField] protected FloatingTextAnimatorBase anim;
        [SerializeField] private PurchaseRewardItemView rewardItemView;


        public event Action<PoolableFloatingPurchaseRewardItemView> RequestReleaseToPool;
        public event Action<PoolableFloatingPurchaseRewardItemView> ObserveDestroy;

        [TriInspector.ShowInInspector] public bool IsInPool { get; set; }
        [TriInspector.ShowInInspector] public int PooledId { get; set; }

        public void Destroy()
        {
            ObserveDestroy?.Invoke(this);
            ObserveDestroy = null;
            RequestReleaseToPool = null;
        }


        public void Show(Sprite icon, string labelText, bool isDisplayRibbon, bool isDisplayInfinityIcon, Vector2 position, Vector3 scale, bool isAnchorPos)
        {
            rewardItemView.Construct(icon, labelText, null, isDisplayRibbon, isDisplayInfinityIcon);

            if (isAnchorPos)
                mainRootRect.anchoredPosition = position;
            else
                mainRootRect.position = position;

            mainRootRect.localScale = scale;
            anim.StartPlay();
        }


        public void ForceStop()
        {
            anim.ForceStop();
        }


        void IPoolableObject<PoolableFloatingPurchaseRewardItemView>.OnCreate()
        {
            anim.OnStopPlay += FloatingTextAnimatorBase_OnStopPlay;
        }

        void IPoolableObject<PoolableFloatingPurchaseRewardItemView>.OnPoolGet()
        {
            transform.SetAsLastSibling();
            SetObjectActive(true);
        }

        void IPoolableObject<PoolableFloatingPurchaseRewardItemView>.OnPoolRelease()
        {
            SetObjectActive(false);
        }

        void IPoolableObject<PoolableFloatingPurchaseRewardItemView>.SetParent(Transform parent)
        {
            if (mainRoot.transform.parent != parent)
                mainRoot.transform.SetParent(parent);
        }


        private void SetObjectActive(bool isActive)
        {
            if (mainRoot.activeSelf != isActive)
                mainRoot.SetActive(isActive);
        }


        private void ReleaseToPool()
        {
            RequestReleaseToPool?.Invoke(this);
        }


        private void FloatingTextAnimatorBase_OnStopPlay()
        {
            ReleaseToPool();
        }
    }
}

