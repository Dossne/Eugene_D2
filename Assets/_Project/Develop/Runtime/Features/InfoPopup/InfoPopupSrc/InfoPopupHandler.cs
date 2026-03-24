using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Features.InfoPopup
{
    public class InfoPopupHandler : MonoBehaviour
    {
        [SerializeField] private Button[] closeButton;
        [SerializeField] private List<InfoPopupElement> infoElements;
        [SerializeField] private float activateTapDelaySec = 2f;

        private float activateTapDelayCurrent;
        private bool isTimerOn;


        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            foreach (var infoElement in infoElements)
                infoElement.Tick(dt);

            HandleDelayForActivateCloseBtn(dt);
        }


        public void Initialize()
        {
            foreach (var infoElement in infoElements)
                infoElement.Initialize();
        }


        public void Deinitialize()
        {
            foreach (var infoElement in infoElements)
                infoElement.Deinitialize();
        }


        public void Open()
        {
            BeginDelayForActivateCloseBtns();
            foreach (var infoElement in infoElements)
                infoElement.PlayAnimation();
        }


        public void Close()
        {
            foreach (var infoElement in infoElements)
                infoElement.StopAnimation();
        }


        private void BeginDelayForActivateCloseBtns()
        {
            activateTapDelayCurrent = activateTapDelaySec;
            SetCloseTappable(false);
            isTimerOn = true;
        }


        private void HandleDelayForActivateCloseBtn(float dt)
        {
            activateTapDelayCurrent -= dt;

            if (isTimerOn && activateTapDelayCurrent <= 0)
            {
                SetCloseTappable(true);
                isTimerOn = false;
            }
        }


        private void SetCloseTappable(bool value)
        {
            foreach (var btn in closeButton)
            {
                btn.interactable = value;
            }
        }
    }
}