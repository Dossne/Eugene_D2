using Infrastructure.Popups;
using System;
using UnityEngine;


namespace Infrastructure.TooltipControl
{
    public abstract class CustomTooltip : MonoBehaviour
    {
        public Action OnClose;

        [SerializeField] private RectTransform rootTransform;
        [SerializeField] private OpenCloseAnimator openCloseAnimator;

        private bool isInit;

        public void Initialize()
        {
            if (isInit)
                return;
            
            Setup();
            
            openCloseAnimator.Initialize();

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            openCloseAnimator.Close();

            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            openCloseAnimator.Deinitialize();
            isInit = false;
        }

        public void Show(CustomTooltipData customTooltipData)
        {
            rootTransform.anchorMin        = customTooltipData.anchorMin;
            rootTransform.anchorMax        = customTooltipData.anchorMax;
            rootTransform.pivot            = customTooltipData.pivot;
            rootTransform.anchoredPosition = customTooltipData.anchoredPosition;
            openCloseAnimator.Open();
        }

        public void Close()
        {
            openCloseAnimator.Close();
            openCloseAnimator.OnCloseComplete += OpenCloseAnimator_OnCloseComplete;
        }

        public void Dispose() 
        {
            Deinitialize();
            Destroy(gameObject);
        }

        protected abstract void Setup();

        private void OpenCloseAnimator_OnCloseComplete()
        {
            openCloseAnimator.OnCloseComplete -= OpenCloseAnimator_OnCloseComplete;
            OnClose?.Invoke();
        }
    }
}