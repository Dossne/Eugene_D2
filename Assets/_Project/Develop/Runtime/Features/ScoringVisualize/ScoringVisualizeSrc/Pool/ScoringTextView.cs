using System;
using Features.WinStreak;
using Infrastructure.BroTweens;
using Infrastructure.Pool;
using TMPro;
using UnityEngine;

namespace Features.ScoringVisualize
{
    public class ScoringTextView : MonoBehaviour, IPoolableObject<ScoringTextView>
    {
        public event Action<ScoringTextView> RequestReleaseToPool;
        public event Action<ScoringTextView> ObserveDestroy;

        private GameObject thisGameObject;

        [TriInspector.Title("Main")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform moveRectTransform;

        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private WinStreakLabelView winStreakLabel;
        [SerializeField] private Transform leftWsLabelRoot;
        [SerializeField] private Transform rightWsLabelRoot;

        private ParticlesPool particlesPool;
        private Color winStreakColor;
        private string winStreakText;

        [TriInspector.ShowInInspector] public int PooledId { get; set; }

        [TriInspector.ShowInInspector] public bool IsInPool { get; set; }


        private void OnDestroy()
        {
            ObserveDestroy?.Invoke(this);
            ObserveDestroy = null;
            RequestReleaseToPool = null;
        }

        public void SetPosition(Vector3 pos)
        {
            transform.position = pos;
        }

        public BroTweenBase GetTween(ParticlesPool particlesPool,
                                     ScoringTextAnimationParams animParams,
                                     WinStreakLabelAnimation labelAnimation,
                                     string baseText,
                                     bool isWinStreak,
                                     string winStreakText,
                                     bool isWsLabelLeftPos)
        {
            Construct(particlesPool, labelAnimation, baseText, isWinStreak, winStreakText, isWsLabelLeftPos);

            var sequence = BroTween.Sequence().SetUpdate(true);

            sequence.Append(BroTween.FadeCanvasGroup(canvasGroup, 0, 1, animParams.fadeInDuration).SetEase(animParams.fadeInEase));

            if (isWinStreak)
            {
                sequence.Append(winStreakLabel.GetMoveTween(labelAnimation, moveRectTransform.position));
                sequence.AppendCallback(this, target => target.ChangeTextToWinStreak());
                sequence.Append(BroTween.ScaleByCurve(labelText.transform, labelAnimation.targetScaleDuration, labelAnimation.targetScaleCurve));
            }

            sequence.AppendInterval(animParams.stayDuration);

            var currDuration = sequence.Duration;

            sequence.Insert(currDuration, BroTween.AnchoredPosition(moveRectTransform,
                                                                    moveRectTransform.anchoredPosition,
                                                                    animParams.hidePosOffset,
                                                                    animParams.hideMoveDuration)
                                                  .SetEase(animParams.hideMoveEase));

            sequence.Insert(currDuration, BroTween.FadeCanvasGroup(canvasGroup, 1, 0, animParams.fadeOutDuration).SetEase(animParams.fadeOutEase));

            sequence.OnComplete(this, target => target.ReleaseToPool());
            return sequence;
        }

        private void ChangeTextToWinStreak()
        {
            if (particlesPool.TryGetItem(PoolableParticleType.CurrencyCountSparks, out var sparksFx))
            {
                sparksFx.transform.position = moveRectTransform.position;
            }

            winStreakLabel.SetObjectActive(false);
            labelText.text = winStreakText;
            this.labelText.color = winStreakColor;
        }

        private void Construct(ParticlesPool particlesPool, WinStreakLabelAnimation labelAnimation, string baseText, bool isWinStreak, string winStreakText, bool isWsLabelLeftPos)
        {
            this.particlesPool = particlesPool;
            this.labelText.text = baseText;
            this.labelText.color = labelAnimation.defaultColor;
            this.winStreakText = winStreakText;
            moveRectTransform.anchoredPosition = Vector2.zero;
            canvasGroup.alpha = 0;
            winStreakLabel.SetObjectActive(isWinStreak);

            if (isWinStreak)
            {
                winStreakLabel.SetPosition(isWsLabelLeftPos ? leftWsLabelRoot.position : rightWsLabelRoot.position);
                winStreakColor = labelAnimation.winStreakColor;
            }
        }

        void IPoolableObject<ScoringTextView>.OnCreate()
        {
            thisGameObject = gameObject;
        }

        void IPoolableObject<ScoringTextView>.OnPoolGet()
        {
            SetObjectActive(true);
        }

        void IPoolableObject<ScoringTextView>.OnPoolRelease()
        {
            SetObjectActive(false);
        }

        void IPoolableObject<ScoringTextView>.SetParent(Transform parent)
        {
            if (thisGameObject.transform.parent != parent)
                thisGameObject.transform.SetParent(parent);
        }

        void IPoolableObject<ScoringTextView>.Destroy()
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

/*#if UNITY_EDITOR
        [SerializeField] private ScoringVisualizeConfig config;
        [SerializeField] private bool isWsLeft;
        [SerializeField] private bool isWs;
        [SerializeField] private ParticlesPool poolDebug;

        [TriInspector.Button]
        public void TestPlay()
        {
            GetTween(poolDebug, config.textAnimParams, config.winStreakAnimParams, "+10", isWs, "+20", isWsLeft).Play();
        }

#endif*/
    }
}