using Infrastructure.DateTimeControl;
using Infrastructure.TimeCycles;
using Infrastructure.Utilities;
using System;
using UnityEngine;

namespace Infrastructure.PurchaseSystem
{
    public class SuperDiscountTimeCycle : PurchaseOfferTimeCycle
    {
        private const double TWO_DAY_SECONDS = 172800;
        private double restSec = TWO_DAY_SECONDS;
        private ITimeCycleReadable timeCycle = null;
        private bool stopTickRequired = true;

        public SuperDiscountTimeCycle(TimeCyclesService timeCyclesService, DateTimeService dateTimeService) : base(timeCyclesService, dateTimeService)
        {
            Offers = new() { OfferId.rh_super_discount_1,
                             OfferId.rh_super_discount_2,
                             OfferId.rh_super_discount_3,
                             OfferId.rh_super_discount_4,
                             OfferId.rh_super_discount_5,
                             OfferId.rh_super_discount_6,
                             OfferId.rh_super_discount_7 };
        }

        public override bool IsOfferActive() => restSec > 0;

        public override double OfferActiveTimeRest() => Math.Floor(restSec);

        public override void Tick()
        {
            if (timeCycle == null && timeCyclesService.TryGetCycleItemReadable(TimeCycleType.SuperDiscountWeekly, out ITimeCycleReadable result))
                timeCycle = result;

            if (timeCycle == null)
                return;

            var lastRestSec = OfferActiveTimeRest();
            var maxTime = Math.Max(timeCycle.TimeRest - (TimeUtils.WEEK_SECONDS - TWO_DAY_SECONDS), TWO_DAY_SECONDS);
            restSec = Math.Clamp(timeCycle.TimeRest - (TimeUtils.WEEK_SECONDS - TWO_DAY_SECONDS), 0.0, maxTime);

            if (OfferActiveTimeRest() < lastRestSec || OfferActiveTimeRest() == 0 && stopTickRequired)
            {
                OnSecondTick?.Invoke(this);

                if (OfferActiveTimeRest() >= lastRestSec && OfferActiveTimeRest() == 0)
                {
                    stopTickRequired = false;
                }
            }
        }

        protected override void TimeCyclesService_OnCycleReset(TimeCycleType type)
        {
            if (type != TimeCycleType.SuperDiscountWeekly)
                return;
            stopTickRequired = true;
            Tick();
            OnCycleReset?.Invoke(this);
        }

        protected override void DateTimeService_OnChange(TimeChangeReason reason)
        {
            stopTickRequired = true;
        }
    }
}