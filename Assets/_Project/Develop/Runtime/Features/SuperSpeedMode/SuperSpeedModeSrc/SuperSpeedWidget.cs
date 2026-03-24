using Infrastructure.Localization;
using Infrastructure.TooltipControl;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Features.SuperSpeedMode
{
    public class SuperSpeedWidget : MonoBehaviour
    {
        [SerializeField] private Image progressImage;
        [SerializeField] private GameObject lockImage;
        [SerializeField] private TextMeshProUGUI counterText;
        [SerializeField] private TextMeshProUGUI lockedText;
        [SerializeField] private Image superSpeedImage;
        [SerializeField] private Button button;
        [SerializeField] private GameObject lockPanel;
        [SerializeField] private GameObject counterPanel;
        [SerializeField] private Tooltip tooltip;
        [SerializeField] private Animator animator;
        private Action callback;
        private bool isInit;

        public void Construct(Sprite sprite, Action callback) 
        {
            superSpeedImage.sprite = sprite;
            this.callback = callback;
        }

        public void Initialize() 
        {
            if (isInit)
                return;
            button.onClick.AddListener(HandleClick);
            tooltip.Initialize();
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;
            button.onClick.RemoveListener(HandleClick);
            tooltip.Deinitialize();
        }

        public void SetWidgetState(bool isFeatureEnabled, 
                                   bool isAnnounced, 
                                   bool isUnlocked, 
                                   float winCount, 
                                   float winMaxCount) 
        {
            gameObject.SetActive(isFeatureEnabled && isAnnounced);
            lockImage.SetActive(!isUnlocked);
            lockPanel.SetActive(!isUnlocked);
            counterPanel.SetActive(isUnlocked);

            if (!gameObject.activeInHierarchy)
                return;
            
            if (isUnlocked && winCount == winMaxCount)
                animator.Play("Active");
            else
                animator.Play("Idle");           

            float fakeValue = Mathf.Clamp01(winCount / winMaxCount) * 0.8f;
            progressImage.fillAmount = fakeValue;
            lockedText.text = LocalizationService.I.Get(LocKeys.Common.Locked);
            counterText.text = isUnlocked ? $"{winCount}/{winMaxCount}" : "";            
        }

        public void ShowTooltip(string text)
        {
            tooltip.Show(text);
        }

        private void HandleClick()
        {
            callback?.Invoke();
        }
    }
}