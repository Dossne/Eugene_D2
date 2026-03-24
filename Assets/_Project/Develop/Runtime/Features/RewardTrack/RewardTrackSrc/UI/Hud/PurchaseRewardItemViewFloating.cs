using Features.PurchaseUi;
using Infrastructure.Animations;
using Infrastructure.Reward;
using Infrastructure.Utilities;
using UnityEngine;

namespace Features.RewardTrack
{
    public class PurchaseRewardItemViewFloating : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private PurchaseRewardItemView rewardItemView;
        [SerializeField] protected FloatingTextAnimatorBase anim;

        private bool isInit;

        public float TotalTime => anim.TotalTime;

        public void Initialize()
        {
            if (isInit)
                return;

            Hide();
            anim.OnStopPlay += Hide;
            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            anim.OnStopPlay -= Hide;
            isInit = false;
        }

        public void Construct(Sprite icon, string labelText, bool isDisplayRibbon, bool isDisplayInfinityIcon)
        {
            rewardItemView.Construct(icon, labelText, null, isDisplayRibbon, isDisplayInfinityIcon);
        }

        [TriInspector.Button]
        public void StartPlay()
        {
            anim.StartPlay();
            Show();
        }

        public void Show()
        {
            SetObjectActive(true);
        }

        public void Hide()
        {
            SetObjectActive(false);
        }

        public void SetParent(Transform parent)
        {
            transform.SetParent(parent);
        }

        public void ResetFlotingRoot()
        {
            anim.ResetState();
        }

        public void SetAnchoredPosition(Vector2 anchoredPosition)
        {
            rectTransform.anchoredPosition = anchoredPosition;
        }

        private void SetObjectActive(bool value)
        {
            gameObject.SetObjectActive(value);
        }
    }
}