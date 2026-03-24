using System;
using Infrastructure.BroTweens;
using Infrastructure.Pool;
using UnityEngine;

namespace Features.ScoringVisualize
{
    public class ScoringIconView : MonoBehaviour, IPoolableObject<ScoringIconView>
    {
        public event Action<ScoringIconView> RequestReleaseToPool;
        public event Action<ScoringIconView> ObserveDestroy;

        [TriInspector.Title("Main")]
        [SerializeField] private GameObject thisGameObject;
        [SerializeField] private Transform scoringTextTarget;

        [SerializeField] private RectTransform mainRect;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform iconRoot;
        [SerializeField] private Animator anim;
        [SerializeField] private string jumpAnimationName = "ScoringIconJump";

        private AnimationCurve verticalOffsetCurve;
        private AnimationCurve scaleCurveX;
        private AnimationCurve scaleCurveY;
        private Action completeCallback;
        private Vector2 fromPos;
        private Vector2 toPos;
        private float offsetVerticalMulti;

        [TriInspector.ShowInInspector] public int PooledId { get; set; }
        [TriInspector.ShowInInspector] public bool IsInPool { get; set; }
        public Vector3 ScoringTextTargetOffset => scoringTextTarget.position - mainRect.position;

        private void OnDestroy()
        {
            ObserveDestroy?.Invoke(this);
            ObserveDestroy = null;
            RequestReleaseToPool = null;
        }

        public BroTweenBase GetTween(ScoringIconAnimationParams animParams, Vector2 from, Vector2 to, Action completeCallback)
        {
            Construct(animParams, from, to, completeCallback);

            var sequence = BroTween.Sequence().SetUpdate(true);

            sequence.AppendCallback(() => SetObjectActive(true));

            sequence.Append(BroTween.FadeCanvasGroup(canvasGroup, 0, 1, animParams.fadeInDuration).SetEase(animParams.fadeInEase));
            sequence.Append(BroTween.Float(ScaleByCurve, animParams.bounceDuration));

            sequence.AppendInterval(animParams.stayDuration);

            sequence.AppendCallback(this, target => target.PlayAnimatorJumpClip());
            sequence.Append(BroTween.Float(MoveByCurves, in animParams.moveDuration).SetEase(animParams.movementEase));
            sequence.OnComplete(this, target => target.ActionsOnComplete());
            return sequence;
        }

        private void ActionsOnComplete()
        {
            anim.enabled = false;
            completeCallback?.Invoke();
            ReleaseToPool();
        }

        private void PlayAnimatorJumpClip()
        {
            anim.enabled = true;
            anim.Play(jumpAnimationName, 0, 0);
        }

        private void Construct(ScoringIconAnimationParams animParams, Vector2 from, Vector2 to, Action completeCallback = null)
        {
            this.fromPos = from;
            this.toPos = to;

            mainRect.position = from;
            this.completeCallback = completeCallback;
            canvasGroup.alpha = 0;

            verticalOffsetCurve = animParams.verticalOffsetCurve;
            offsetVerticalMulti = animParams.offsetVerticalMulti;
            scaleCurveX = animParams.scaleCurveX;
            scaleCurveY = animParams.scaleCurveY;
            ResetState();
        }

        void IPoolableObject<ScoringIconView>.OnCreate()
        {
            ResetState();
        }

        void IPoolableObject<ScoringIconView>.OnPoolGet() { }

        void IPoolableObject<ScoringIconView>.OnPoolRelease()
        {
            SetObjectActive(false);
        }

        void IPoolableObject<ScoringIconView>.SetParent(Transform parent)
        {
            if (thisGameObject.transform.parent != parent)
                thisGameObject.transform.SetParent(parent);
        }

        void IPoolableObject<ScoringIconView>.Destroy()
        {
            Destroy(gameObject);
        }

        private void MoveByCurves(float progress)
        {
            Vector2 basePos = Vector2.LerpUnclamped(fromPos, toPos, progress);
            float yOffset = verticalOffsetCurve.Evaluate(progress);
            basePos.y += yOffset * offsetVerticalMulti;
            mainRect.position = basePos;
        }

        private void ScaleByCurve(float progress)
        {
            float x = scaleCurveX.Evaluate(progress);
            float y = scaleCurveY.Evaluate(progress);
            mainRect.localScale = new Vector3(x, y, 1);
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

        private void ResetState()
        {
            anim.enabled = false;
            iconRoot.offsetMin = Vector2.zero;
            iconRoot.offsetMax = Vector2.zero;
        }

/*#if UNITY_EDITOR
        [SerializeField] private Transform fromTr;
        [SerializeField] private Transform toTr;
        [SerializeField] private ScoringVisualizeConfig config;

        [TriInspector.Button]
        public void TestPlay()
        {
            GetTween(config.iconAnimParams, icon.sprite, fromTr.position, toTr.position, null).Play();
        }

#endif*/
    }
}