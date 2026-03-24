using System;
using Infrastructure.Utilities;
using TriInspector;
using UnityEngine;

namespace Infrastructure.TimeCycles
{
    /// <summary>
    /// For test dates in editor
    /// </summary>
    public class TimeCycleChecker_Editor : MonoBehaviour
    {
        [Title("Config")]
        [SerializeField] private TimeCyclesConfig timeCyclesConfig;

        [Title("Input")]
        [SerializeField] private TimeCycleType cycleType;
        [SerializeField] private SerializableDateTime checkDate;

        [Title("Result")]
        [SerializeField, ReadOnly] private string result;
        [SerializeField, ReadOnly] private string timeUntilReset;


        [Button]
        public void InitCheckDateBySystemTime()
        {
            checkDate = DateTime.UtcNow.ToSerializableDateTime();
        }


        [Button]
        public void GetNextDate()
        {
            foreach (var dataItem in timeCyclesConfig.Items)
            {
                if (dataItem.cycleType == cycleType)
                {
                    DateTime check = checkDate.ToDateTime();
                    DateTime nextDate = TimeCyclesService.GetNextResetDate(check, cycleType, dataItem.targetHour, dataItem.targetMinute,
                                                                           dataItem.targetDayOfWeek);

                    result = $"{cycleType.ToString()} next: {TimeUtils.GetStringFromDateTime(nextDate)}";
                    timeUntilReset = $"Reset in: {TimeUtils.GetTimeString(TimeSpan.FromSeconds((nextDate - check).TotalSeconds), ":", true)}";
                }
            }
        }
    }
}