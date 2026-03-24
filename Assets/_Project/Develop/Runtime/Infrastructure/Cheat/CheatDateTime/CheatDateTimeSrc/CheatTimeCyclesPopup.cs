#if PR_CHEAT
using System;
using Infrastructure.Popups;
using Infrastructure.TimeCycles;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Infrastructure.Cheat
{
    public sealed class CheatTimeCyclesPopup : PopupBase
    {
        [Header("Info texts")]
        [SerializeField] private TextMeshProUGUI dailyNextTxt;
        [SerializeField] private TextMeshProUGUI dailyResetLeftTxt;

        [SerializeField] private TextMeshProUGUI weeklyNextTxt;
        [SerializeField] private TextMeshProUGUI weeklyResetLeftTxt;

        [SerializeField] private TextMeshProUGUI superDiscountNextTxt;
        [SerializeField] private TextMeshProUGUI superDiscountResetLeftTxt;

        [SerializeField] private TextMeshProUGUI freePaidOfferNextTxt;
        [SerializeField] private TextMeshProUGUI freePaidOfferResetLeftTxt;

        [SerializeField] private TextMeshProUGUI monthlyNextTxt;
        [SerializeField] private TextMeshProUGUI monthlyResetLeftTxt;

        [Header("Buttons")]
        [SerializeField] private Button dailyResetBtn;
        [SerializeField] private Button weeklyResetBtn;
        [SerializeField] private Button superDiscountResetBtn;
        [SerializeField] private Button freePaidOfferResetBtn;
        [SerializeField] private Button monthlyResetBtn;
        [SerializeField] private Button allResetBtn;

        private TimeCyclesService timeCyclesService;
        private float second;


        [Inject]
        public void Construct(TimeCyclesService timeCyclesService)
        {
            this.timeCyclesService = timeCyclesService;
        }


        protected override void OnInitialize()
        {
            dailyResetBtn.onClick.AddListener(CheatResetDaily);
            weeklyResetBtn.onClick.AddListener(CheatResetWeekly);
            superDiscountResetBtn.onClick.AddListener(CheatResetSuperDiscount);
            freePaidOfferResetBtn.onClick.AddListener(CheatResetFreePaidOffer);
            monthlyResetBtn.onClick.AddListener(CheatResetMonthly);
            allResetBtn.onClick.AddListener(CheatResetAll);
        }


        protected override void OnDeinitialize()
        {
            dailyResetBtn.onClick.RemoveAllListeners();
            weeklyResetBtn.onClick.RemoveAllListeners();
            superDiscountResetBtn.onClick.RemoveAllListeners();
            freePaidOfferResetBtn.onClick.RemoveAllListeners();
            monthlyResetBtn.onClick.RemoveAllListeners();

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
            Refresh(TimeCycleType.Daily, dailyNextTxt, dailyResetLeftTxt);
            Refresh(TimeCycleType.Weekly, weeklyNextTxt, weeklyResetLeftTxt);
            Refresh(TimeCycleType.SuperDiscountWeekly, superDiscountNextTxt, superDiscountResetLeftTxt);
            Refresh(TimeCycleType.FreePaidOfferWeekly, freePaidOfferNextTxt, freePaidOfferResetLeftTxt);
            Refresh(TimeCycleType.Monthly, monthlyNextTxt, monthlyResetLeftTxt);
        }


        private void Refresh(TimeCycleType cycleType, TextMeshProUGUI next, TextMeshProUGUI left)
        {
            if (!timeCyclesService.TryGetCycleItemReadable(cycleType, out ITimeCycleReadable item))
                return;

            next.text = $"{cycleType.ToString()} next: {TimeUtils.GetStringFromDateTime(item.NextResetDate)}";
            left.text = $"{cycleType.ToString()} reset in: {TimeUtils.GetTimeString(TimeSpan.FromSeconds(item.TimeRest))}";
        }


        private void CheatResetDaily()
        {
            timeCyclesService.CheatResetDaily();
            Vibrate();

        }


        private void CheatResetWeekly()
        {
            timeCyclesService.CheatResetWeekly();
            Vibrate();
        }


        private void CheatResetSuperDiscount()
        {
            timeCyclesService.CheatResetSuperDiscount();
            Vibrate();
        }


        private void CheatResetFreePaidOffer()
        {
            timeCyclesService.CheatResetFreePaidOffer();
            Vibrate();
        }

        private void CheatResetMonthly()
        {
            timeCyclesService.CheatResetMonthly();
            Vibrate();
        }


        private void CheatResetAll()
        {
            timeCyclesService.CheatResetAll();
            Vibrate();
        }


        private void Vibrate()
        {
            HapticControl.HapticService.I.HapticLight();
        }
    }
}
#endif