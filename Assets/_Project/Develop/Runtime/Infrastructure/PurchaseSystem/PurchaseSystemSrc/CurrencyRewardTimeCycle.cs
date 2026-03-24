using Infrastructure.DateTimeControl;
using Infrastructure.TimeCycles;
using System;

namespace Infrastructure.PurchaseSystem
{
    public class CurrencyRewardTimeCycle : PurchaseOfferTimeCycle
    {
        private double restSec = 86400;
        private ITimeCycleReadable timeCycle = null;
        private bool stopTickRequired = true;

        public CurrencyRewardTimeCycle(TimeCyclesService timeCyclesService, DateTimeService dateTimeService) : base(timeCyclesService, dateTimeService)
        {
            Offers = new() { OfferId.rh_rewarded_coins };
        }

        public override bool IsOfferActive() => restSec > 0;

        public override double OfferActiveTimeRest() => Math.Floor(restSec);

        public override void Tick()
        {
            if (timeCycle == null && timeCyclesService.TryGetCycleItemReadable(TimeCycleType.Daily, out ITimeCycleReadable result))
                timeCycle = result;

            if (timeCycle == null)
                return;

            var lastRestSec = OfferActiveTimeRest();
            restSec = timeCycle.TimeRest;

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
            if (type != TimeCycleType.Daily)
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