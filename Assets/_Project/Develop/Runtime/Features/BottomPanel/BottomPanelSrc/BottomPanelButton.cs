using System;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Infrastructure.Localization;
using Infrastructure.Utilities;
using Infrastructure.BroTweens;

namespace Features.BottomPanel
{
    public class BottomPanelButton : MonoBehaviour
    {
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image buttonImage;
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private float toggleAnimationDuration = 0.3f;
        [SerializeField] private AnimationCurve toggleAnimationCurve = new();
        [SerializeField] private Sprite toggledSprite;
        [SerializeField] private Sprite untoggledSprite;
        [SerializeField] private GameObject notifier;
        private Action callback;
        private BottomPanelButtonData buttonData;
        private BroTweenSafe toggleSeq;

        public bool IsToggled {  get; private set; } = false;        

        public void Construct(BottomPanelButtonData bottomPanelButtonData, Sprite icon, Action callback)
        {
            this.buttonData = bottomPanelButtonData; 
            layoutElement.minWidth = buttonData.layoutMinWidth;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
            buttonText.text = LocalizationService.I.Get(bottomPanelButtonData.textKey);
            buttonText.gameObject.SetObjectActive(false);
            this.callback = callback;
            iconImage.sprite = icon;
            iconImage.transform.localScale = Vector3.one * buttonData.untoggleIconScale;
            buttonImage.sprite = untoggledSprite;
        }

        public void SetToggled(bool isToggled)
        {
            bool needAnimation = IsToggled != isToggled;
            IsToggled = isToggled;

            if (!needAnimation)
                return;

            float startWidth   = isToggled ? buttonData.layoutMinWidth       : buttonData.layoutPrefferedWidth;
            float targetWidth  = isToggled ? buttonData.layoutPrefferedWidth : buttonData.layoutMinWidth      ;
            float startScale   = isToggled ? buttonData.untoggleIconScale : buttonData.toggleIconScale  ;
            float targetScale  = isToggled ? buttonData.toggleIconScale   : buttonData.untoggleIconScale;

            toggleSeq.Kill();
            toggleSeq = BroTween.Sequence()
                                .Insert(0, BroTween.Float(SetPrefferedWidth, startWidth, targetWidth, toggleAnimationDuration).SetEase(toggleAnimationCurve))
                                .Insert(0, BroTween.Float(SetIconScale     , startScale, targetScale, toggleAnimationDuration).SetEase(toggleAnimationCurve))
                                .SetUpdate(true)
                                .ToSafe();
            toggleSeq.Play();

            buttonText.gameObject.SetObjectActive(isToggled);
            buttonImage.sprite = isToggled ? toggledSprite : untoggledSprite;
        }

        public void SetNotifierActive(bool isActive)
        {
            if (notifier != null)
                notifier.SetObjectActive(isActive);
        }

        public void StopAnimation() 
        {
            toggleSeq.Kill();
        }

        private void SetPrefferedWidth(float value)
        {
            if (layoutElement != null)
                layoutElement.preferredWidth = value;
        }

        private void SetIconScale(float value)
        {
            if (iconImage != null)
                iconImage.transform.localScale = Vector3.one * value;
        }

        private void HandleClick() 
        {
            callback.Invoke();
        }
    }
}