using System;

namespace Infrastructure.TimeCycles
{
    public class CycleItem : ITimeCycleReadable
    {
        public event Action<CycleItem> OnComplete;

        private readonly TimeCycleData configData;
        private readonly CycleItemState state;

        private double timeRest;
        private double timeMax;
        private bool isCompleted;

        public TimeCycleData ConfigData => configData;
        public DateTime NextResetDate => state.nextResetDate;
        public double TimeRest => timeRest;


        public CycleItem(TimeCycleData configData, CycleItemState  state)
        {
            this.configData = configData;
            this.state = state;
        }


        public void Reset(DateTime nextResetDate, double timeSeconds)
        {
            this.state.nextResetDate = nextResetDate;
            timeMax = timeSeconds;
            timeRest = timeSeconds;
            isCompleted = false;
        }


        public void Tick(float dt)
        {
            if (isCompleted)
                return;
            
            timeRest -= dt;
            
            if(timeRest > 0f)
                return;
            
            isCompleted = true;
            OnComplete?.Invoke(this);
        }


        public void Deinitialize()
        {
            OnComplete = null;
        }
    }
}