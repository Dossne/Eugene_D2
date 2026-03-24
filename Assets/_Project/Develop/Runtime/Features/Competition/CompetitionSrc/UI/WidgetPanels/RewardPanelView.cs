using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.RewardTrack;
using Infrastructure.BroTweens;
using Infrastructure.Pool.Particles;
using Infrastructure.Reward;
using Infrastructure.Utilities;
using UnityEngine;

namespace Features.Competition
{
    [Serializable]
    public class RewardPanelAnimations
    {
        [Header("Panel show")]
        public float beginDelay = 0.25f;
        public AnimationCurve showEase;
        public float showDuration = 1f;

        [Header("Icon")]
        public float beginIconAnimDelay = 0.35f;

        [Header("PanelStay")]
        public float panelStayDuration = 0.5f;

        [Header("Panel hide")]
        public AnimationCurve hideEase;
        public float hideDuration = 1f;
    }

    public class RewardPanelView : MonoBehaviour
    {
        [Header("Main")]
        [SerializeField] private RectTransform mainRoot;
        [SerializeField] private PurchaseRewardItemViewFloating rewardItemViewFloating;
        [SerializeField] private Transform itemViewRootDefault;
        [SerializeField] private Transform itemViewRootParent;
        [SerializeField] private ReusableParticleSystemUI splashFx;

        [SerializeField] private Vector2 showPosition;
        [SerializeField] private Vector2 hidePos;

        [Header("Animation")]
        [SerializeField] private RewardPanelAnimations animParams;

        [Header("Debug")]
        [SerializeField] private BroTweenSafe seq;

        private bool isInit;

        private void OnDisable()
        {
            seq.Kill();
        }

        public void Construct(RewardItemVisualData reward)
        {
            rewardItemViewFloating.Construct(reward.icon, reward.amountText, reward.isDisplayRibbon, reward.isDisplayInfinityIcon);
        }

        public void Initialize()
        {
            if (isInit)
                return;

            CreateSequence();
            SetViewParent(itemViewRootDefault);
            rewardItemViewFloating.SetAnchoredPosition(Vector2.zero);
            rewardItemViewFloating.ResetFlotingRoot();
            rewardItemViewFloating.Show();
            mainRoot.anchoredPosition = hidePos;
            splashFx.Initialize();

            SetObjectActive(true);

            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            SetObjectActive(false);

            isInit = false;
        }

        public UniTask PlayAsync(CancellationToken token)
        {
            seq.Play();
            return seq.ToUniTask(cancellationToken: token);
        }

        private void CreateSequence()
        {
            var sequence = BroTween.Sequence().SetUpdate(true);

            //panel show
            sequence.AppendInterval(animParams.beginDelay);
            sequence.Append(BroTween.AnchoredPosition(mainRoot, hidePos, showPosition, animParams.showDuration).SetEase(animParams.showEase));

            //icon animation
            var iconAnimateTime = animParams.beginDelay + animParams.showDuration + animParams.beginIconAnimDelay;
            sequence.InsertCallback(iconAnimateTime, AnimateRewardIcon);
            sequence.InsertCallback(iconAnimateTime + rewardItemViewFloating.TotalTime, rewardItemViewFloating.Hide);

            //panel hide
            var hideTime = animParams.beginDelay + animParams.showDuration + animParams.panelStayDuration;
            sequence.Insert(hideTime, BroTween.AnchoredPosition(mainRoot, showPosition, hidePos, animParams.hideDuration).SetEase(animParams.hideEase));

            seq = sequence.ToSafe();
        }

        private void AnimateRewardIcon()
        {
            splashFx.Play();
            SetViewParent(itemViewRootParent);
            rewardItemViewFloating.StartPlay();
        }

        private void SetObjectActive(bool value)
        {
            mainRoot.gameObject.SetObjectActive(value);
        }

        private void SetViewParent(Transform transform1)
        {
            rewardItemViewFloating.SetParent(transform1);
        }

#region UNITY_EDITOR

        [TriInspector.Button]
        private void SetShowPos_Editor()
        {
            showPosition = mainRoot.anchoredPosition;
        }

        [TriInspector.Button]
        private void SetHidePos_Editor()
        {
            hidePos = mainRoot.anchoredPosition;
        }

#endregion
    }
}