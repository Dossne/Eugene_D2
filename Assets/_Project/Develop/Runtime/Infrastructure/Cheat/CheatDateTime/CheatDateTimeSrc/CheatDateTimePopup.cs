#if PR_CHEAT
using System;
using System.Globalization;
using Infrastructure.DateTimeControl;
using Infrastructure.Popups;
using Infrastructure.TimeCycles;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Infrastructure.Cheat
{
    public sealed class CheatDateTimePopup : PopupBase
    {
        [Header("Info texts")]
        [SerializeField] private TextMeshProUGUI statusTxt;
        [SerializeField] private TextMeshProUGUI serverTimeTxt;
        [SerializeField] private TextMeshProUGUI systemTimeTxt;
        [SerializeField] private TextMeshProUGUI diffTxt;
        [SerializeField] private TextMeshProUGUI shiftTimeTxt;
        [SerializeField] private TextMeshProUGUI sinceLastSyncTxt;

        [Header("Components")]
        [SerializeField] private TimeShiftPanel timeShiftPanel;

        [Header("Buttons")]
        [SerializeField] private Button timeShiftBtn;

        private DateTimeService dateTimeService;
        private float second;


        [Inject]
        public void Construct(DateTimeService dateTimeService)
        {
            this.dateTimeService = dateTimeService;
        }


        protected override void OnInitialize()
        {
            timeShiftPanel.Construct(dateTimeService);
            timeShiftPanel.Initialize();
            timeShiftBtn.onClick.AddListener(OpenTimeShiftPanel);
        }


        protected override void OnDeinitialize()
        {
            timeShiftPanel.Deinitialize();
            timeShiftBtn.onClick.RemoveAllListeners();
        }


        protected override void OnBeginOpen()
        {
            RefreshInfo();
        }


        private void Update()
        {
            second += Time.unscaledDeltaTime;

            if (second < 1)
                return;

            second -= 1;
            RefreshInfo();
        }


        private void RefreshInfo()
        {
            bool isSync = dateTimeService.TryGetServerTime(out DateTime serverTime);
            statusTxt.text = isSync ? "Status: SYNC".ToColor(ColorHex.Green) : "Status: NOT SYNC".ToColor(ColorHex.Red);
            serverTimeTxt.text = $"Server: {TimeUtils.GetStringFromDateTime(serverTime)}";
            systemTimeTxt.text = $"System: {TimeUtils.GetStringFromDateTime(dateTimeService.GetSystemTime())}";
            diffTxt.text = $"Diff: {dateTimeService.TimeDifferenceUtc.TotalSeconds.ToString(CultureInfo.InvariantCulture)} sec";
            shiftTimeTxt.text = $"Cheat shift: {TimeUtils.GetTimeString(dateTimeService.CheatTimeShift)}";
            sinceLastSyncTxt.text = $"Since last sync: {dateTimeService.TimeSinceLastSync.ToString(CultureInfo.InvariantCulture)} sec";
        }


        private void OpenTimeShiftPanel()
        {
            timeShiftPanel.SetObjectActive(true);
            HapticControl.HapticService.I.HapticLight();
        }
    }
}
#endif