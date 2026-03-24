using System;
using Infrastructure.Localization;
using Infrastructure.Popups;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.LavaQuest
{
    public class LavaQuestStartPopup : PopupBase
    {
        public event Action OnStartRequested;

        [Header("Components")]
        [SerializeField] private Button startBtn;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI headerTxt;
        [SerializeField] private TextMeshProUGUI descriptionTxt;
        [SerializeField] private TextMeshProUGUI startBtnTxt;
        [SerializeField] private TextMeshProUGUI timerTxt;

        private float timeRest;
        private float refreshTimeCurrent;


        private void Update()
        {
            if (timeRest <= 0)
                return;

            refreshTimeCurrent += Time.unscaledDeltaTime;
            timeRest -= Time.unscaledDeltaTime;

            if (refreshTimeCurrent < 1)
                return;

            SetTimeText();
            refreshTimeCurrent = 1;
        }


        protected override void OnInitialize()
        {
            headerTxt.text = LocalizationService.I.Get(LocKeys.LavaQuest.Name);
            startBtnTxt.text = LocalizationService.I.Get(LocKeys.LavaQuest.Start);
            startBtn.onClick.AddListener(OnClickStart);
        }


        protected override void OnDeinitialize()
        {
            startBtn.onClick.RemoveAllListeners();
        }


        public void SetDescriptionText(string value)
        {
            descriptionTxt.text = value;
        }


        public void SetTimeRest(float timeLeft)
        {
            this.timeRest = timeLeft;
            this.refreshTimeCurrent = 1;
        }


        public void SwitchState(bool isTimerActive)
        {
            timerTxt.gameObject.SetObjectActive(isTimerActive);
            startBtn.gameObject.SetObjectActive(!isTimerActive);
        }


        private void SetTimeText()
        {
            string timeStr = TimeUtils.GetTimeString(timeRest);
            timerTxt.text = timeStr;
        }


        private void OnClickStart()
        {
            OnStartRequested?.Invoke();
        }
    }
}