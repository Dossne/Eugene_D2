using AYellowpaper.SerializedCollections;
using Features.Tutorial;
using Infrastructure.MainUICanvasControl;
using Infrastructure.Popups;
using System;
using TMPro;
using UnityEngine;

namespace Infrastructure.TooltipControl
{
    public class SimpleTutorialTooltip : MonoBehaviour
    {
        public Action OnClose;

        [SerializeField] private RectTransform rootTransform;
        [SerializeField] private RectTransform tailTransform;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private OpenCloseAnimator openCloseAnimator;
        [SerializeField] private SerializedDictionary<UiOrientationType, RectTransform> tailAnchors = new();

        private bool isInit;

        public void Initialize()
        {
            if (isInit)
                return;

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

        public void Show(string text, UiOrientationType orientationType)
        {
            this.text.text = text;
            tailTransform.SetPositionAndRotation(tailAnchors[orientationType].position, tailAnchors[orientationType].rotation);
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

        private void OpenCloseAnimator_OnCloseComplete()
        {
            openCloseAnimator.OnCloseComplete -= OpenCloseAnimator_OnCloseComplete;
            OnClose?.Invoke();
        }
    }
}