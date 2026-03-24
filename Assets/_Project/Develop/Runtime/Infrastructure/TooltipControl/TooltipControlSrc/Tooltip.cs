using Infrastructure.Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.TooltipControl
{
    public class Tooltip : MonoBehaviour
    {
        [Header("Base")]
        [SerializeField] protected RectTransform rootTransform;
        [SerializeField] protected OpenCloseAnimator openCloseAnimator;
        [SerializeField] private TextMeshProUGUI bodyTxt;
        [SerializeField] private Button closeButton;

        private bool isInit;

        public void Initialize()
        {
            if (isInit)
                return;

            openCloseAnimator.Initialize();
            openCloseAnimator.InstantClose();
            closeButton.onClick.AddListener(Close);
            OnInitialize();
            
            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            closeButton.onClick.RemoveListener(Close);
            openCloseAnimator.Deinitialize();
            OnDeinitialize();
            
            isInit = false;
        }

        public void Show(string bodyTxt)
        {
            this.bodyTxt.text = bodyTxt;
            Open();
        }

        public void Show(string bodyTxt, Vector3 position)
        {
            rootTransform.position = position;
            this.bodyTxt.text = bodyTxt;
            Open();
        }

        public void Open()
        {
            openCloseAnimator.Open();
        }

        public void Close()
        {
            openCloseAnimator.Close();
        }

        protected virtual void OnInitialize() { }
        protected virtual void OnDeinitialize() { }
    }
}