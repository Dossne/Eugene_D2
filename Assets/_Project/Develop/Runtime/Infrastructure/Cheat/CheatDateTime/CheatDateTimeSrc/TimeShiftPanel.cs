#if PR_CHEAT
using System;
using Infrastructure.DateTimeControl;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.Cheat
{
    public class TimeShiftPanel : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private TextMeshProUGUI shiftTimeTxt;

        [Header("Input")]
        [SerializeField] private IntField field_hh;
        [SerializeField] private IntField field_mm;
        [SerializeField] private IntField field_s;
        [SerializeField] private IntField field_dd;
        [SerializeField] private IntField field_MM;
        [SerializeField] private IntField field_yy;

        [Header("Actions")]
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button inputAsServerBtn;
        [SerializeField] private Button setTimeBtn;
        [SerializeField] private Button shiftRemoveBtn;

        private DateTimeService dateTimeService;
        private bool isInit;


        public void Construct(DateTimeService dateTimeService)
        {
            this.dateTimeService = dateTimeService;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            closeBtn.onClick.AddListener(() => SetObjectActive(false));
            inputAsServerBtn.onClick.AddListener(SetInputAsServer);
            setTimeBtn.onClick.AddListener(SetTimeToService);
            shiftRemoveBtn.onClick.AddListener(RemoveShift);

            field_hh.Initialize();
            field_mm.Initialize();
            field_s.Initialize();
            field_dd.Initialize();
            field_MM.Initialize();
            field_yy.Initialize();

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            closeBtn.onClick.RemoveAllListeners();
            inputAsServerBtn.onClick.RemoveAllListeners();
            setTimeBtn.onClick.RemoveAllListeners();
            shiftRemoveBtn.onClick.RemoveAllListeners();

            field_hh.Deinitialize();
            field_mm.Deinitialize();
            field_s.Deinitialize();
            field_dd.Deinitialize();
            field_MM.Deinitialize();
            field_yy.Deinitialize();

            isInit = false;
        }


        public void SetObjectActive(bool value)
        {
            gameObject.SetObjectActive(value);
            
            if (value)
            {
                SetInputAsServer();
                RefreshText();
            }
        }


        private void SetInputAsServer()
        {
            dateTimeService.TryGetServerTime(out DateTime serverTime);
            field_hh.Set(serverTime.Hour);
            field_mm.Set(serverTime.Minute);
            field_s.Set(serverTime.Second);
            field_dd.Set(serverTime.Day);
            field_MM.Set(serverTime.Month);
            field_yy.Set(serverTime.Year);
        }


        private void RefreshText()
        {
            shiftTimeTxt.text = $"Cheat shift: {TimeUtils.GetTimeString(dateTimeService.CheatTimeShift)}";
        }


        private void SetTimeToService()
        {
            try
            {
                DateTime targetDateTime = new DateTime(field_yy.CurrentValue,
                                                       field_MM.CurrentValue,
                                                       field_dd.CurrentValue,
                                                       field_hh.CurrentValue,
                                                       field_mm.CurrentValue,
                                                       field_s.CurrentValue,
                                                       DateTimeKind.Utc);
                dateTimeService.TryGetServerTime(out DateTime serverTime);
                DateTime serverTimeRaw = serverTime - dateTimeService.CheatTimeShift;



                TimeSpan shift = targetDateTime - serverTimeRaw;
                dateTimeService.CheatSetTimeShift(shift);
                RefreshText();
                Vibrate();
            }
            catch(Exception e)
            {
                shiftTimeTxt.text = $"Cheat shift: <color=red><size=70>Incorrect Date!</size></color>";
                UnityEngine.Debug.LogError(e.ToString());
            }
        }


        private void RemoveShift()
        {
            dateTimeService.CheatSetTimeShift(TimeSpan.Zero);
            RefreshText();
            Vibrate();
        }
        
        private void Vibrate()
        {
            HapticControl.HapticService.I.HapticLight();
        }
    }
}
#endif