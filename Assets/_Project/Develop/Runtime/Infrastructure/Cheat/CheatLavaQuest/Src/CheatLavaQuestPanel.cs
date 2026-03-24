#if PR_CHEAT
using System.Collections.Generic;
using Features.LavaQuest;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Infrastructure.Cheat.CheatLavaQuest
{
    public class CheatLavaQuestPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI statusStateTxt;
        [SerializeField] private TextMeshProUGUI statusDatesTxt;
        [SerializeField] private TextMeshProUGUI statusTimeTxt;
        [SerializeField] private Button stepIdxAddBtn;
        [SerializeField] private Button stepIdxRemoveBtn;
        [SerializeField] private Button winBtn;
        [SerializeField] private Button looseBtn;
        [SerializeField] private Button resetBtn;
        [SerializeField] private Button expireTimerBtn;

        [SerializeField] private List<Button> closeBtns;

        private LavaQuestStateController stateController;
        private LavaQuestEventPopupController eventPopupController;

        private float second;
        private bool isInit;

        public bool IsInit => isInit;


        private void Update()
        {
            second += Time.unscaledDeltaTime;

            if (second < 1)
                return;

            second -= 1;
            RefreshInfo();
        }


        [Inject]
        public void Inject(LavaQuestStateController stateController,
                           LavaQuestEventPopupController eventPopupController)
        {
            this.stateController = stateController;
            this.eventPopupController = eventPopupController;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            foreach (var btn in closeBtns)
            {
                btn.onClick.AddListener(Close);
            }

            stepIdxAddBtn.onClick.AddListener(IncreaseStep);
            stepIdxRemoveBtn.onClick.AddListener(DecreaseStep);
            winBtn.onClick.AddListener(Win);
            looseBtn.onClick.AddListener(Loose);
            resetBtn.onClick.AddListener(ResetAll);
            expireTimerBtn.onClick.AddListener(ExpireTimer);

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            foreach (var btn in closeBtns)
            {
                btn.onClick.RemoveListener(Close);
            }

            stepIdxAddBtn.onClick.RemoveAllListeners();
            stepIdxRemoveBtn.onClick.RemoveAllListeners();
            winBtn.onClick.RemoveAllListeners();
            looseBtn.onClick.RemoveAllListeners();
            resetBtn.onClick.RemoveAllListeners();
            expireTimerBtn.onClick.RemoveAllListeners();

            isInit = false;
        }


        public void Open()
        {
            second = 0;
            RefreshInfo();
            SetObjectActive(true);
        }


        private void SetObjectActive(bool value)
        {
            gameObject.SetObjectActive(value);
        }


        private void Close()
        {
            SetObjectActive(false);
        }


        private void IncreaseStep()
        {
            stateController.CheatIncreaseStep();
            RefreshInfo();
        }


        private void DecreaseStep()
        {
            stateController.CheatDecreaseStep();
            RefreshInfo();
        }


        private void Win()
        {
            if (!stateController.IsStartedState())
                return;

            stateController.CheatScheduleWin();
            eventPopupController.CheatClosePopup();
            eventPopupController.TryOpenPopupOnScheduledAction();
            RefreshInfo();
        }


        private void Loose()
        {
            if (!stateController.IsStartedState())
                return;

            stateController.CheatScheduleLoose();
            eventPopupController.CheatClosePopup();
            eventPopupController.TryOpenPopupOnScheduledAction();
            RefreshInfo();
        }


        private void ExpireTimer()
        {
            stateController.CheatExpireTimer();
            RefreshInfo();
        }


        private void ResetAll()
        {
            stateController.CheatReset();
            eventPopupController.CheatReset();
            RefreshInfo();
        }


        private void RefreshInfo()
        {
            RefreshState();
            RefreshDates();
            RefreshTime();
        }


        private void RefreshState()
        {
            statusStateTxt.text = $"State: {stateController.CurrentState.ToString()}." +
                                  $"\nChain id: {stateController.CurrentChainId}. Step: {stateController.CurrentStepIdx} / {stateController.MaxStepIdx}." +
                                  $"\nReady start times: {stateController.ReadyStartTimes}";
        }


        private void RefreshDates()
        {
            string endDate = TimeUtils.GetStringFromDateTime(stateController.EndDate);
            string nextDailyReset = TimeUtils.GetStringFromDateTime(stateController.NextResetDate);

            statusDatesTxt.text = $"EndDate: {endDate}." +
                                  $"\nNextDailyReset: {nextDailyReset}";
        }


        private void RefreshTime()
        {
            string timeStr = TimeUtils.GetTimeString(stateController.TimeRest);
            statusTimeTxt.text = $"Time left: {timeStr}";
        }
    }
}
#endif