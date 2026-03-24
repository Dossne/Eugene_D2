using System;

namespace Infrastructure.BroTweens
{
    [Serializable]
    public enum BroTweenState : byte
    {
        None = 0,
        ReadyToPlay = 1,
        Playing = 2,
        Completed = 3,
    }
}