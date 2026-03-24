using System;
using Infrastructure.Animations;
using TMPro;
using UnityEngine;

namespace Infrastructure.Pool.FloatingText
{
    public class PoolableFloatingText : MonoBehaviour, IPoolableObject<PoolableFloatingText>
    {
        public event Action<PoolableFloatingText> RequestReleaseToPool;
        public event Action<PoolableFloatingText> ObserveDestroy;

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject mainRoot;
        [SerializeField] private RectTransform mainRootRect;
        [SerializeField] private TextMeshProUGUI countTxt;
        [SerializeField] private FloatingTextAnimatorBase textAnimator;

        [TriInspector.ShowInInspector] public bool IsInPool { get; set; }
        [TriInspector.ShowInInspector] public int PooledId { get; set; }


        private void OnDestroy()
        {
            ObserveDestroy?.Invoke(this);
            ObserveDestroy = null;
            RequestReleaseToPool = null;
        }


        public void Show(string text, Vector2 position, Vector3 scale, bool isAnchorPos)
        {
            countTxt.text = text;
            
            if(isAnchorPos)
                mainRootRect.anchoredPosition = position;
            else
                mainRootRect.position = position;

            mainRootRect.localScale = scale;
            textAnimator.StartPlay();
            canvasGroup.alpha = 1;
        }


        public void ForceStop()
        {
            textAnimator.ForceStop();
        }


        void IPoolableObject<PoolableFloatingText>.OnCreate()
        {
            textAnimator.OnStopPlay += FloatingTextAnimatorBase_OnStopPlay;
            textAnimator.SetUpdateEnabled(false);
        }


        void IPoolableObject<PoolableFloatingText>.OnPoolGet()
        {
            //transform.SetAsLastSibling();
        }


        void IPoolableObject<PoolableFloatingText>.OnPoolRelease()
        {
            canvasGroup.alpha = 0;
        }


        void IPoolableObject<PoolableFloatingText>.SetParent(Transform parent)
        {
            if (mainRoot.transform.parent != parent)
                mainRoot.transform.SetParent(parent);
        }


        void IPoolableObject<PoolableFloatingText>.Destroy()
        {
            textAnimator.OnStopPlay -= FloatingTextAnimatorBase_OnStopPlay;
            Destroy(gameObject);
        }


        private void ReleaseToPool()
        {
            RequestReleaseToPool?.Invoke(this);
        }


        private void SetObjectActive(bool isActive)
        {
            if (mainRoot.activeSelf != isActive)
                mainRoot.SetActive(isActive);
        }


        private void FloatingTextAnimatorBase_OnStopPlay()
        {
            ReleaseToPool();
        }


#if UNITY_EDITOR


        [TriInspector.Button, TriInspector.ShowInPlayMode]
        public void ReleaseTest()
        {
            ReleaseToPool();
        }


        [TriInspector.Button, TriInspector.ShowInPlayMode]
        public void DestroyTest()
        {
            Destroy(gameObject);
        }
#endif
    }
}