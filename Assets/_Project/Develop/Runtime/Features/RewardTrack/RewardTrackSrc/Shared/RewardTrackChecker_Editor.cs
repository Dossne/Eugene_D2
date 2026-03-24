#if UNITY_EDITOR
using System;
using Infrastructure.Utilities;
using TriInspector;
using Infrastructure.TimeCycles;
#endif
using UnityEngine;

namespace Features.RewardTrack
{
    /// <summary>
    /// For test dates in editor
    /// </summary>
    public class RewardTrackChecker_Editor : MonoBehaviour
    {
#if UNITY_EDITOR

        [Title("Config")]
        [SerializeField] private RewardTrackConfig rewardTrackConfig;
        [SerializeField] private TimeCyclesConfig timeCyclesConfig;

        [Title("Input")]
        [SerializeField] private SerializableDateTime checkDate;

        [Title("Result")]
        [SerializeField, ReadOnly] private string resetDate;
        [SerializeField, ReadOnly] private string timeUntilReset;
        [SerializeField, ReadOnly] private RewardTrackData currentRewardTrack;


        [Button]
        public void InitCheckDateBySystemTime()
        {
            checkDate = DateTime.UtcNow.ToSerializableDateTime();
        }


        [Button]
        public void Calculate()
        {
            foreach (var dataItem in timeCyclesConfig.Items)
            {
                if (dataItem.cycleType == TimeCycleType.Daily)
                {
                    DateTime currentDateTime = checkDate.ToDateTime();
                    DateTime nextResetDate = TimeCyclesService.GetNextResetDate(currentDateTime, TimeCycleType.Daily, dataItem.targetHour, dataItem.targetMinute);

                    if (!RewardTrackStateController.TryGetTrackAndNextDow(nextResetDate, rewardTrackConfig.TrackDatas, out currentRewardTrack, out DateTime nextDow))
                    {
                        resetDate = "Not found active track";
                        timeUntilReset = "Not found";
                        return;
                    }

                    resetDate = $"Next: {TimeUtils.GetStringFromDateTime(nextDow)}. Day of week: {nextDow.DayOfWeek}";
                    timeUntilReset = $"Reset in: {TimeUtils.GetTimeString(TimeSpan.FromSeconds((nextDow - currentDateTime).TotalSeconds), ":", true)}";
                    return;
                }
            }
        }


#endif

    }
}