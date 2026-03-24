using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.WinStreak;
using Infrastructure.Pool.FloatingIcon;
using Infrastructure.BroTweens;
using Infrastructure.HapticControl;
using Infrastructure.Pool;
using Infrastructure.Pool.Particles;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.RewardTrack
{
    public class PoolableFloatingIconRewardTrack : MonoBehaviour, IPoolableObject<PoolableFloatingIconRewardTrack>
    {
        public event Action<PoolableFloatingIconRewardTrack> RequestReleaseToPool;
        public event Action<PoolableFloatingIconRewardTrack> ObserveDestroy;

        [Header("Main")]
        [SerializeField] private Image icon;
        [SerializeField] private RectTransform iconRoot;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private PoolableFloatingIcon floatingIcon;

        [Header("WinStreak label")]
        [SerializeField] private WinStreakLabelView wsLabel;
        [SerializeField] private Transform winStreakTargetPoint;
        [SerializeField] private ReusableParticleSystem winStreakSplashFx;

        [Header("Animation")]
        [SerializeField] private RewardTrackIconAnimationParams anim;

        [TriInspector.ShowInInspector] private BroTweenSafe showSeq;
        [TriInspector.ShowInInspector] private BroTweenSafe flySeq;
        private GameObject thisGameObject;
        private string multipliedCountText;

        [TriInspector.ShowInInspector] public bool IsInPool { get; set; }
        [TriInspector.ShowInInspector] public int PooledId { get; set; }


        private void OnDestroy()
        {
            ObserveDestroy?.Invoke(this);
            ObserveDestroy = null;
            RequestReleaseToPool = null;
            KillSequences();
        }


        public UniTask Show(Sprite icon, string countText, Vector2 startPosition, Vector2 endPosition, bool isWinStreakMax, string multipliedCountText,
                            CancellationToken cancellationToken)
        {
            this.countText.text = countText;
            this.countText.color = anim.winStreakLabelAnimation.defaultColor;

            floatingIcon.Show(icon, startPosition, endPosition);
            wsLabel.SetObjectActive(false);

            var seq = BroTween.Sequence()
                              .SetUpdate(true)
                              .Append(GetMainIconScaleTween(anim.showScaleCurve, anim.showScaleDuration));

            if (isWinStreakMax)
            {
                this.multipliedCountText = multipliedCountText;
                seq.Append(wsLabel.GetMoveTween(anim.winStreakLabelAnimation, winStreakTargetPoint.position));
                seq.AppendCallback(this, target => target.PlaFxWinStreakBonus());
                seq.Append(GetMainIconScaleTween(anim.winStreakLabelAnimation.targetScaleCurve, anim.winStreakLabelAnimation.targetScaleDuration));
            }

            showSeq = seq.ToSafe();
            showSeq.Play();

            return showSeq.ToUniTask(BroTweenCancelBehaviour.KillWithCallback, cancellationToken: cancellationToken);
        }


        public UniTask Fly(CancellationToken cancellationToken)
        {
            var seq = BroTween.Sequence()
                              .Append(floatingIcon.GetSequence())
                              .Insert(0, GetTextFadeTween())
                              .AppendCallback(this, target => target.ReleaseToPool());

            flySeq = seq.ToSafe();
            flySeq.Play();

            return flySeq.ToUniTask(BroTweenCancelBehaviour.KillWithCallback, cancellationToken: cancellationToken);
        }


        void IPoolableObject<PoolableFloatingIconRewardTrack>.OnCreate()
        {
            winStreakSplashFx.Initialize();
            thisGameObject = gameObject;
        }


        void IPoolableObject<PoolableFloatingIconRewardTrack>.OnPoolGet()
        {
            floatingIcon.SetCanvasGroupActive();
            thisGameObject.SetObjectActive(true);
        }


        void IPoolableObject<PoolableFloatingIconRewardTrack>.OnPoolRelease()
        {
            floatingIcon.SetCanvasGroupNotActive();
            thisGameObject.SetObjectActive(false);
            KillSequences();
        }


        void IPoolableObject<PoolableFloatingIconRewardTrack>.SetParent(Transform parent)
        {
            if (thisGameObject.transform.parent != parent)
                thisGameObject.transform.SetParent(parent);
        }


        void IPoolableObject<PoolableFloatingIconRewardTrack>.Destroy()
        {
            winStreakSplashFx.Deinitialize();
            Destroy(gameObject);
        }


        private void ReleaseToPool()
        {
            RequestReleaseToPool?.Invoke(this);
        }


        private void PlaFxWinStreakBonus()
        {
            wsLabel.SetObjectActive(false);
            countText.text = multipliedCountText;
            countText.color = anim.winStreakLabelAnimation.winStreakColor;;
            winStreakSplashFx.Play();
            HapticService.I.HapticSelection();
        }


        private BroTweenBase GetMainIconScaleTween(AnimationCurve curve, float duration)
        {
            return BroTween.ScaleByCurve(iconRoot, Vector3.one, in duration, curve);
        }


        private BroTweenBase GetTextFadeTween()
        {
            SetAlpha(1);
            return BroTween.FadeText(countText, 0, anim.fadeTextDuration)
                           .SetEase(anim.fadeTextCurve);
        }


        private void SetAlpha(float value)
        {
            countText.alpha = value;
        }


        private void KillSequences()
        {
            flySeq.Kill();
            showSeq.Kill();
        }
    }
}