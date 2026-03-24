using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.BroTweens;
using Infrastructure.Pool;
using UnityEngine;


namespace Features.WinStreak
{
    public class PoolableWinStreakObject : MonoBehaviour, IPoolableObject<PoolableWinStreakObject>
    {
        public event Action<PoolableWinStreakObject> RequestReleaseToPool;
        public event Action<PoolableWinStreakObject> ObserveDestroy;
        [Header("Components")]
        [SerializeField] private Transform boostersFlyPoint;

        [Header("Animation")]
        [SerializeField] private WinStreakObjectMoveParams animParams;
        [SerializeField] private float stayBeforeMoveDuration;
        [SerializeField] private float boostersBeginSpawnTime;

        private GameObject thisGameObject;
        private BroTweenSafe moveTween;

        public Vector3 BoostersFlyPoint => boostersFlyPoint.position;
        [TriInspector.ShowInInspector] public bool IsInPool { get; set; }
        [TriInspector.ShowInInspector] public int PooledId { get; set; }


        private void OnDisable()
        {
            moveTween.Kill();
            ReleaseToPool();
        }


        private void OnDestroy()
        {
            ObserveDestroy?.Invoke(this);
            ObserveDestroy = null;
            RequestReleaseToPool = null;
        }


        public async UniTask MoveAsync(Vector3 left, Vector3 center, Vector3 right, CancellationToken token)
        {
            moveTween.Kill();
            MoveToCenter(left, center);
            await UniTask.WaitForSeconds(animParams.moveFromLeftDuration + stayBeforeMoveDuration, cancellationToken: token);
            MoveToRightOffScreen(right);
            await UniTask.WaitForSeconds(animParams.moveToRightDuration, cancellationToken: token);
            ReleaseToPool();
        }


        public float GetBoostersBeginSpawnTime()
        {
            return boostersBeginSpawnTime;
        }


        public float GetTotalTime()
        {
            return animParams.moveFromLeftDuration + animParams.moveToRightDuration + stayBeforeMoveDuration;
        }


        void IPoolableObject<PoolableWinStreakObject>.OnCreate()
        {
            thisGameObject = gameObject;
        }


        void IPoolableObject<PoolableWinStreakObject>.OnPoolGet()
        {
            SetObjectActive(true);
        }


        void IPoolableObject<PoolableWinStreakObject>.OnPoolRelease()
        {
            SetObjectActive(false);
        }


        void IPoolableObject<PoolableWinStreakObject>.SetParent(Transform parent)
        {
            if (thisGameObject.transform.parent != parent)
                thisGameObject.transform.SetParent(parent);
        }


        void IPoolableObject<PoolableWinStreakObject>.Destroy()
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


        private void MoveToRightOffScreen(Vector3 right)
        {
            moveTween = BroTween.Position(thisGameObject.transform, right, animParams.moveToRightDuration).SetEase(animParams.moveToRightCurve).ToSafe();
            moveTween.Play();
        }


        private void MoveToCenter(Vector3 left, Vector3 center)
        {
            thisGameObject.transform.position = left;
            moveTween = BroTween.Position(thisGameObject.transform, center, animParams.moveFromLeftDuration).SetEase(animParams.moveFromLeftCurve).ToSafe();
            moveTween.Play();
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