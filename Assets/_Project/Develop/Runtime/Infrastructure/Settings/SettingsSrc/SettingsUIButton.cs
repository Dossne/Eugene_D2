using System;
using Infrastructure.BroTweens;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.Settings
{
    public class SettingsUIButton : MonoBehaviour
    {
        public event Action OnClick;
        [SerializeField] private Button button;
        [SerializeField] private RectTransform buttonRect;


        public void Initialize()
        {
            button.onClick.AddListener(Click);
        }


        public void Deinitialize()
        {
            button.onClick.RemoveListener(Click);
        }


        private void Click()
        {
            BroTween.ClickBounceWithCallBack(button, buttonRect, callbackOwner: this, target => target.InvokeOnClick()).Play();
        }


        private void InvokeOnClick()
        {
            OnClick?.Invoke();
        }
    }
}