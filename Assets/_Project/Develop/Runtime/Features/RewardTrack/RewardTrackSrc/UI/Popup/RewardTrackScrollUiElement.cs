using System;
using Features.PurchaseUi;
using Features.ScrollList;
using Infrastructure.AudioControl;
using Infrastructure.BroTweens;
using Infrastructure.Localization;
using Infrastructure.Reward;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.RewardTrack
{
    public class RewardTrackStateUiElement : ScrollElement<RewardTrackStateUiElement>
    {
        public event Action<int> OnClick;

        [SerializeField] private PurchaseRewardItemView rewardItemView;
        [SerializeField] private GameObject completeGo;
        [SerializeField] private RectTransform root;
        [SerializeField] private TextMeshProUGUI stepNumberTxt;
        [SerializeField] private GameObject chainToNextStepGo;
        [SerializeField] private GameObject currentSetHighLightFx;

        [SerializeField] private Image stepIcon;
        [SerializeField] private Sprite nonCompleteStepIcon;
        [SerializeField] private Sprite completeStepIcon;

        [Header("Click bounce")]
        [SerializeField] private Button slotBodyBtn;
        [SerializeField] private Transform tooltipTarget;
        [SerializeField] private RectTransform lockTransform;
        [SerializeField] private RewardTrackStateUiElementAnimationParams animParams;

        [Header("Slot body")]
        [SerializeField] private GameObject lastRewardBg;
        [SerializeField] private TextMeshProUGUI lastRewardTxt;
        [SerializeField] private Image slotBodyImage;
        [SerializeField] private string defaultSlotBgName;
        [SerializeField] private string lastSlotBgName;

        private BroTweenSafe bounceSeq;
        private int index;
        private int stepNumber;

        public RectTransform Root => root;
        public Transform TooltipTarget => tooltipTarget;
        public int StepNumber => stepNumber;
        public int Index => index;
        public string LastSlotBgName => lastSlotBgName;
        public string DefaultSlotBgName => defaultSlotBgName;

        private bool isInit;


        private void Start()
        {
            slotBodyBtn.onClick.AddListener(OnBounceBtnClick);
        }


        private void OnDestroy()
        {
            OnClick = null;
            slotBodyBtn.onClick.RemoveAllListeners();
        }


        public void Construct(Sprite iconSprite, Sprite bgSprite, string rewardText, int stepNumber, bool isDisplayRibbon, bool isDisplayInfinityIcon, bool isLast, int index)
        {
            rewardItemView.Construct(iconSprite, rewardText, null, isDisplayRibbon, isDisplayInfinityIcon);
            slotBodyImage.sprite = bgSprite;
            this.stepNumber = stepNumber;
            stepNumberTxt.text = stepNumber.ToString();
            SetChainToNextActive(!isLast);
            lastRewardBg.SetObjectActive(isLast);
            
            if (isLast)
                lastRewardTxt.text = LocalizationService.I.Get(LocKeys.RewardTrack.LastReward);
            
            this.index = index;
        }

        public void RefreshState(bool isCurrent, bool isComplete)
        {
            completeGo.SetObjectActive(isComplete);
            rewardItemView.SetObjectActive(!isComplete);
            currentSetHighLightFx.SetObjectActive(isCurrent);
            stepIcon.sprite = isCurrent || !isComplete ? nonCompleteStepIcon : completeStepIcon;
            slotBodyBtn.interactable = !isComplete;
        }

        public void SetObjectActive(bool value)
        {
            gameObject.SetObjectActive(value);
        }

        private void SetChainToNextActive(bool isActive)
        {
            chainToNextStepGo.SetObjectActive(isActive);
        }

        private void OnBounceBtnClick()
        {
            bounceSeq.Kill(true);

            bounceSeq = BroTween.Sequence()
                                .Append(BroTween.ScaleByCurve(slotBodyBtn.transform, animParams.rootBounceDuration, animParams.scaleCurve))
                                .AppendCallback(() => OnClick?.Invoke(index))
                                .Append(BroTween.RotationLocal(lockTransform, Vector3.zero, new Vector3(0, 0, animParams.maxZAngle), animParams.lockRotateDuration)
                                                .SetEase(animParams.rotateCurve))
                                .SetUpdate(true)
                                .ToSafe();
            bounceSeq.Play();
            AudioService.I.PlaySfx(SfxType.ClickUI);
        }

        protected override void InitializeImpl() { }
        protected override void DeinitializeImpl() { }
    }
}