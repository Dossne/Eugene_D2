using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Collectables;
using Features.LevelConfiguration;
using Infrastructure.BroTweens;
using Infrastructure.Pool;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Features.WinStreak
{
    public class PoolableWinStreakBooster : MonoBehaviour, IPoolableObject<PoolableWinStreakBooster>
    {
        public event Action<PoolableWinStreakBooster> RequestReleaseToPool;
        public event Action<PoolableWinStreakBooster> ObserveDestroy;

        [Header("Components")]
        [SerializeField] private CollectableItem collectableItem;

        [Header("Animation")]
        [SerializeField] private WinStreakBoosterFlyParams animParams;

        private GameObject thisGameObject;
        private BroTweenSafe moveTween;
        private BroTweenSafe scaleTween;
        private BroTweenSafe rotTween;

        [TriInspector.ShowInInspector] public bool IsInPool { get; set; }
        [TriInspector.ShowInInspector] public int PooledId { get; set; }


        private void OnDisable()
        {
            ReleaseToPool();
            KillTweens();
        }


        private void OnDestroy()
        {
            ObserveDestroy?.Invoke(this);
            ObserveDestroy = null;
            RequestReleaseToPool = null;
        }


        public void Construct(CollectableType collectableType, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            collectableItem.Construct(collectableType, position, rotation, scale);
        }


        public async UniTask MoveAsync(Vector3 from, Vector3 to, CancellationToken token)
        {
            PlayMoveTween(from, to);
            PlayScaleTween();
            PLayRotateTween();

            await UniTask.WaitForSeconds(animParams.moveDuration, cancellationToken: token);

            KillTweens();
            PhysicsWakeUp();
        }


        public bool IsTargetItem(CollectableItem item)
        {
            return item == collectableItem;
        }


        public CollectableData GetDataExtended()
        {
            return collectableItem.GetDataExtended();
        }


        public void PhysicsWakeUp()
        {
            collectableItem.SetCollidersActive(true);
            collectableItem.SetCollidersLayerToDefault();
            collectableItem.PhysicsWakeUp();
        }


        private void KillTweens()
        {
            moveTween.Kill();
            scaleTween.Kill();
            rotTween.Kill();
        }


        private void PhysicsSetSleep()
        {
            collectableItem.SetCollidersActive(false);
            collectableItem.PhysicsSetSleep();
        }


        void IPoolableObject<PoolableWinStreakBooster>.OnCreate()
        {
            thisGameObject = gameObject;
        }


        void IPoolableObject<PoolableWinStreakBooster>.OnPoolGet()
        {
            PhysicsSetSleep();
            SetObjectActive(true);
        }


        void IPoolableObject<PoolableWinStreakBooster>.OnPoolRelease()
        {
            SetObjectActive(false);
        }


        void IPoolableObject<PoolableWinStreakBooster>.SetParent(Transform parent)
        {
            if (thisGameObject.transform.parent != parent)
                thisGameObject.transform.SetParent(parent);
        }


        void IPoolableObject<PoolableWinStreakBooster>.Destroy()
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


        private void PlayMoveTween(Vector3 from, Vector3 to)
        {
            moveTween.Kill();
            collectableItem.SetPositionAnRotation(from, Quaternion.identity);

            moveTween = BroTween.Float(t => Move(from, to, t), animParams.moveDuration)
                                .SetEase(animParams.moveEaseCurve)
                                .ToSafe();
            moveTween.Play();
        }


        private void Move(Vector3 from, Vector3 to, float t)
        {
            float posX = Mathf.Lerp(from.x, to.x, t);
            float posY = Mathf.Lerp(from.y, to.y, t) + animParams.curveY.Evaluate(t);
            float posZ = Mathf.Lerp(from.z, to.z, t);
            thisGameObject.transform.position = new Vector3(posX, posY, posZ);
        }


        private void PlayScaleTween()
        {
            scaleTween.Kill();
            thisGameObject.transform.localScale = animParams.startScale;
            scaleTween = BroTween.ScaleByCurve(thisGameObject.transform, animParams.scaleDuration, animParams.scaleCurve).ToSafe();
            scaleTween.Play();
        }


        private void PLayRotateTween()
        {
            rotTween.Kill();
            Vector3 randomRotation = new Vector3(Random.Range(-animParams.rotateDirection, animParams.rotateDirection),
                                                 Random.Range(-animParams.rotateDirection, animParams.rotateDirection),
                                                 Random.Range(-animParams.rotateDirection, animParams.rotateDirection));

            rotTween = BroTween.Rotation(thisGameObject.transform, randomRotation, animParams.rotateDuration, RotateMode.Full).ToSafe();
            rotTween.Play();
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