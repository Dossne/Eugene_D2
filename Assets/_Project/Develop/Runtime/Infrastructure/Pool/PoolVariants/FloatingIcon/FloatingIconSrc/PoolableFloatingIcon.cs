using System;
using Infrastructure.BroTweens;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.Pool.FloatingIcon
{
    public class PoolableFloatingIcon : MonoBehaviour, IPoolableObject<PoolableFloatingIcon>
    {
        public event Action<PoolableFloatingIcon> RequestReleaseToPool;
        public event Action<PoolableFloatingIcon> ObserveDestroy;

        [Header("Main")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform mainRect;
        [SerializeField] private Image icon;

        [Header("Animation")]
        [SerializeField] private FloatingIconAnimationParams anim;

        [TriInspector.ShowInInspector] private BroTweenSafe seq;
        private GameObject thisGameObject;
        private Vector2 startPosition;
        private Vector2 endPosition;

        private Action<PoolableFloatingIcon> onComplete;
        private object requesterObject;
        private object onCompleteCallback;

        [TriInspector.ShowInInspector] public bool IsInPool { get; set; }
        [TriInspector.ShowInInspector] public int PooledId { get; set; }

        public float MoveDuration => anim.moveDuration;


        private void OnDestroy()
        {
            ObserveDestroy?.Invoke(this);
            ObserveDestroy = null;
            RequestReleaseToPool = null;
            KillSequence();
        }


        public void ShowAndFly(Sprite icon, Vector2 startPosition, Vector2 endPosition)
        {
            Show(icon, startPosition, endPosition);
            Fly();
        }


        public void ShowAndFly(Sprite icon, Transform startPosition, Vector3 finishPosShiftedFromStart)
        {
            ShowAndFly(icon, startPosition.position, startPosition.position + finishPosShiftedFromStart);
        }


        public void Show(Sprite icon, Vector2 startPosition, Vector2 endPosition)
        {
            if (icon != null)
                this.icon.sprite = icon;

            this.startPosition = startPosition;
            this.endPosition = endPosition;
            SetDefaultParams();
        }


        private void Fly()
        {
            if (!seq.TryRewind())
            {
                KillSequence();
                seq = GetSequence().SetAutoKill(false).ToSafe();
            }
            
            seq.Play();
        }

        
        public void SetOnCompleteCallback<T>(T target, Action<T> onComplete) where T : class
        { 
            if (target == null)
            {
                Debug.LogError($"{nameof(target)} is null or has been destroyed");
                return;
            }

            requesterObject = target;
            this.onCompleteCallback = onComplete;
            this.onComplete = tween =>
            {
                Action<T> cal = tween.onCompleteCallback as Action<T>;
                T tar = tween.requesterObject as T;
                cal?.Invoke(tar);
            };
        }


        public BroSequence GetSequence()
        {
            var sequence = BroTween.Sequence().SetUpdate(true);

            if (anim.moveDuration > 0)
                sequence.Insert(0, GetMoveTween());

            if (anim.scaleDuration > 0)
                sequence.Insert(0, GetScaleTween());

            if (Mathf.Abs(anim.rotateSpeedZ) > 0 && anim.rotateDuration > 0)
                sequence.Insert(0, GetRotateTween());

            sequence.OnComplete(target: this, target => target.OnEndFly());

            return sequence;
        }


        public void SetCanvasGroupActive()
        {
            canvasGroup.alpha = 1;
        }


        public void SetCanvasGroupNotActive()
        {
            canvasGroup.alpha = 0;
        }


        void IPoolableObject<PoolableFloatingIcon>.OnCreate()
        {
            thisGameObject = gameObject;
        }


        void IPoolableObject<PoolableFloatingIcon>.OnPoolGet()
        {
            SetCanvasGroupActive();
        }


        void IPoolableObject<PoolableFloatingIcon>.OnPoolRelease()
        {
            onComplete = null;
            requesterObject = null;
            onCompleteCallback = null;
            SetCanvasGroupNotActive();
            CompleteSequence();
        }


        void IPoolableObject<PoolableFloatingIcon>.SetParent(Transform parent)
        {
            if (thisGameObject.transform.parent != parent)
                thisGameObject.transform.SetParent(parent);
        }


        void IPoolableObject<PoolableFloatingIcon>.Destroy()
        {
            Destroy(gameObject);
        }


        private void ReleaseToPool()
        {
            RequestReleaseToPool?.Invoke(this);
        }


        private void SetDefaultParams()
        {
            mainRect.position = startPosition;
            mainRect.localScale = Vector3.one * anim.scaleCurve.Evaluate(0);
        }


        private void OnEndFly()
        {
            onComplete?.Invoke(this);
            ReleaseToPool();
        }


        private BroTweenBase GetScaleTween()
        {
            return BroTween.ScaleByCurve(mainRect, Vector3.one, in anim.scaleDuration, anim.scaleCurve);
        }


        private BroTweenBase GetMoveTween()
        {
            return BroTween.Float(this, (target, value) => target.MoveByCurvesComplex(value), in anim.moveDuration)
                           .SetEase(anim.movementEaseCurve);
        }


        private BroTweenBase GetRotateTween()
        {
            return BroTween.RotationLocal(mainRect, new Vector3(0.0f, 0.0f, anim.rotateSpeedZ), anim.rotateDuration, RotateMode.Full)
                           .SetEase(anim.rotateEaseCurve);
        }


        private void MoveByCurvesComplex(float value)
        {
            float xOffset = anim.horizontalOffsetCurve.Evaluate(value);
            float yOffset = anim.verticalOffsetCurve.Evaluate(value);

            float basePosX = Mathf.LerpUnclamped(startPosition.x, endPosition.x, anim.movementCurveX.Evaluate(value));
            basePosX += xOffset * anim.offsetHorizontalMulti;

            float basePosY = Mathf.LerpUnclamped(startPosition.y, endPosition.y, anim.movementCurveY.Evaluate(value));
            basePosY += yOffset * anim.offsetVerticalMulti;

            mainRect.position = new Vector3(basePosX, basePosY, mainRect.position.z);
        }


        private void KillSequence()
        {
            seq.Kill();
        }

        private void CompleteSequence()
        {
            seq.Complete();
        }
        

#if UNITY_EDITOR

        [TriInspector.Button, TriInspector.ShowInPlayMode]
        public void FlyLast()
        {
            SetCanvasGroupActive();
            Fly();
        }
#endif

    }
}