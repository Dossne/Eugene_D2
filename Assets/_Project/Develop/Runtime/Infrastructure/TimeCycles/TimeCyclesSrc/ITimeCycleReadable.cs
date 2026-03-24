using System;

namespace Infrastructure.TimeCycles
{
    public interface ITimeCycleReadable
    {
        public DateTime NextResetDate { get; }
        public double TimeRest { get; }
    }
}