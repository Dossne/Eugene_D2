using Infrastructure.DateTimeControl;
using Infrastructure.TimeCycles;
using System;
using System.Collections.Generic;

namespace Infrastructure.PurchaseSystem
{
    public abstract class PurchaseOfferTimeCycle
    {
        public Action<PurchaseOfferTimeCycle> OnSecondTick;
        public Action<PurchaseOfferTimeCycle> OnActiveTimeOff;
        public Action<PurchaseOfferTimeCycle> OnCycleReset;

        protected TimeCyclesService timeCyclesService;
        protected DateTimeService dateTimeService;
        protected bool isFirstTick = true;

        public List<OfferId> Offers { get; protected set; } = new();

        public PurchaseOfferTimeCycle(TimeCyclesService timeCyclesService, DateTimeService dateTimeService)
        {
            this.timeCyclesService = timeCyclesService;
            this.timeCyclesService.OnCycleReset += TimeCyclesService_OnCycleReset;
            this.dateTimeService = dateTimeService;
            this.dateTimeService.OnChange += DateTimeService_OnChange;
        }    

        public abstract void Tick();
        public abstract double OfferActiveTimeRest();
        public abstract bool IsOfferActive();
        protected abstract void TimeCyclesService_OnCycleReset(TimeCycleType type);
        protected abstract void DateTimeService_OnChange(TimeChangeReason reason);
    }
}