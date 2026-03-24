using System;

namespace Infrastructure.DateTimeControl
{
    [Serializable]
    public class DateTimeState
    {
#if PR_CHEAT || UNITY_EDITOR
       public long cheatShiftTicks;
#endif
    }
}