using System;
using System.Collections.Generic;
using Infrastructure.ApplicationInterrupt;
using Infrastructure.Configs;
using Infrastructure.DateTimeControl;
using Infrastructure.PersistentProgress;
using Infrastructure.SystemsLifeCycle;
using Infrastructure.Utilities;
using UnityEngine;

namespace Infrastructure.TimeCycles
{
    public class TimeCyclesService : ISavable, ISystemTickable
    {
        public event Action<TimeCycleType> OnCycleReset;

        private readonly Dictionary<TimeCycleType, CycleItem> cycles = new();
        private readonly DateTimeService dateTimeService;
        private readonly AppInterruptObserver appInterruptObserver;
        private readonly TimeCyclesConfig cyclesConfig;
        private readonly bool showDebug = false;
        private TimeCycleState state;
        private bool isAppInterrupt;
        private bool isInit;


        public TimeCyclesService(DateTimeService dateTimeService, AppInterruptObserver appInterruptObserver, ConfigProvider configProvider)
        {
            this.dateTimeService = dateTimeService;
            this.appInterruptObserver = appInterruptObserver;
            this.cyclesConfig = configProvider.TimeCyclesConfig;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            bool isSyncWithServer = dateTimeService.TryGetServerTime(out DateTime serverTime);

            if (!isSyncWithServer)
                LogFailGetServerTime();

            InitializeCycleItems(serverTime);

            appInterruptObserver.Interrupt += AppInterruptObserver_Interrupt;
            appInterruptObserver.Resume += AppInterruptObserver_Resume;
            dateTimeService.OnChange += DateTimeService_OnChange;

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            appInterruptObserver.Interrupt -= AppInterruptObserver_Interrupt;
            appInterruptObserver.Resume -= AppInterruptObserver_Resume;
            dateTimeService.OnChange -= DateTimeService_OnChange;

            foreach (var entryPair in cycles)
            {
                entryPair.Value.Deinitialize();
            }

            cycles.Clear();
            isInit = false;
        }


        void ISavable.Load(Progress progress)
        {
            state = progress.appState.timeCycle;
        }


        void ISavable.Save(Progress progress)
        {

        }


        void ISystemTickable.Tick()
        {
            if (isAppInterrupt)
                return;

            float dt = Time.unscaledDeltaTime;

            foreach (var entryPair in cycles)
            {
                entryPair.Value.Tick(dt);
            }
        }


        /// <summary>
        /// Readable only info for systems from outer scope
        /// </summary>
        /// <param name="cycleType"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryGetCycleItemReadable(TimeCycleType cycleType, out ITimeCycleReadable result)
        {
            if (!cycles.TryGetValue(cycleType, out CycleItem cycle))
            {
                result = null;
                return false;
            }

            result = cycle;
            return true;
        }


        /// <summary>
        /// Calculates next reset date (searches forward from checkTime)
        /// </summary>
        /// <param name="checkTime">DateTime for start search</param>
        /// <param name="cycleType">TimeCycleType</param>
        /// <param name="targetHour">Hour of result will equal targetHour</param>
        /// <param name="targetMinute">Minute of result will equal targetMinute</param>
        /// <param name="targetDow">Target day of week need to search for cycleType that contains "Weekly"</param>
        /// <param name="isIncludeCheckTime">result can be equal CheckTime or not, actual only for cycleType that contains "Weekly"</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static DateTime GetNextResetDate(DateTime checkTime,
                                                TimeCycleType cycleType,
                                                int targetHour,
                                                int targetMinute,
                                                DayOfWeek targetDow = DayOfWeek.Sunday,
                                                bool isIncludeCheckTime = false)
        {
            int targetSecond = 0;
            return cycleType switch
            {
                TimeCycleType.Daily               => TimeUtils.GetNextDay(checkTime, targetHour, targetMinute, targetSecond),
                TimeCycleType.Weekly              => TimeUtils.GetNextWeekDay(checkTime, targetDow, targetHour, targetMinute, targetSecond, isIncludeCheckTime),
                TimeCycleType.SuperDiscountWeekly => TimeUtils.GetNextWeekDay(checkTime, targetDow, targetHour, targetMinute, targetSecond, isIncludeCheckTime),
                TimeCycleType.FreePaidOfferWeekly => TimeUtils.GetNextWeekDay(checkTime, targetDow, targetHour, targetMinute, targetSecond, isIncludeCheckTime),
                TimeCycleType.Monthly             => TimeUtils.GetNextMonthFirstDay(checkTime, targetHour, targetMinute, targetSecond),
                _                                 => throw new ArgumentOutOfRangeException(nameof(cycleType), cycleType, null)
            };
        }


        private void InitializeCycleItems(DateTime serverTime)
        {
            foreach (TimeCycleData data in cyclesConfig.Items)
            {
                CycleItemState itemState = GetOrAddItemState(data.cycleType);
                CycleItem cycleItem = new CycleItem(data, itemState);
                double totalSeconds = (itemState.nextResetDate - serverTime).TotalSeconds;
                cycleItem.Reset(itemState.nextResetDate, Math.Max(0, totalSeconds));
                cycleItem.OnComplete += CycleItem_OnComplete;
                cycles.TryAdd(data.cycleType, cycleItem);
            }
        }


        private void ResetCycle(CycleItem item)
        {
            bool isSyncWithServer = dateTimeService.TryGetServerTime(out DateTime serverTime);

            if (!isSyncWithServer)
                LogFailGetServerTime();

            TimeCycleData data = item.ConfigData;
            DateTime targetDateTime = GetNextResetDate(serverTime, data.cycleType, data.targetHour, data.targetMinute, data.targetDayOfWeek);
            double secondsLeft = (targetDateTime - serverTime).TotalSeconds;
            item.Reset(targetDateTime, secondsLeft);
            OnCycleReset?.Invoke(item.ConfigData.cycleType);
            LogReset(item, targetDateTime, secondsLeft);
        }


        private CycleItemState GetOrAddItemState(TimeCycleType cycleType)
        {
            foreach (var item in state.states)
            {
                if (item.cycleType != cycleType)
                    continue;

                return item;
            }

            var result = new CycleItemState
            {
                cycleType = cycleType,
                nextResetDate = DateTime.UnixEpoch
            };

            state.states.Add(result);
            return result;
        }


        private void LogFailGetServerTime()
        {
            if (!showDebug)
                return;

            Debug.LogWarning($"[{nameof(TimeCyclesService)}] Failed to get server time. Continue by system time");
        }


        private void LogReset(CycleItem item, in DateTime targetDateTime, in double secondsLeft)
        {
            if (!showDebug)
                return;

            Debug.Log($"[{nameof(TimeCyclesService)}] Cycle {item.ConfigData.cycleType} has been reset. " +
                      $"NextResetDate = {targetDateTime:dd-MM-yyyy HH:mm:ss}. " +
                      $"SecondsLeft = {(int)secondsLeft}");
        }


        private void RefreshTimersState()
        {
            dateTimeService.TryGetServerTime(out DateTime serverTime);

            foreach (var cycleItem in cycles.Values)
            {
                double totalSeconds = (cycleItem.NextResetDate - serverTime).TotalSeconds;
                cycleItem.Reset(cycleItem.NextResetDate, Math.Max(0, totalSeconds));
            }
        }


        private void CycleItem_OnComplete(CycleItem item)
        {
            ResetCycle(item);
        }


        private void AppInterruptObserver_Resume()
        {
            isAppInterrupt = false;
            RefreshTimersState();
        }


        private void AppInterruptObserver_Interrupt()
        {
            isAppInterrupt = true;
        }


        private void DateTimeService_OnChange(TimeChangeReason reason)
        {
            RefreshTimersState();
        }


#region Cheats

#if PR_CHEAT || UNITY_EDITOR

        private float cheatSetValue = 10f;


        public void CheatResetDaily()
        {
            CheatResetByType(TimeCycleType.Daily);
        }


        public void CheatResetWeekly()
        {
            CheatResetByType(TimeCycleType.Weekly);
        }


        public void CheatResetMonthly()
        {
            CheatResetByType(TimeCycleType.Monthly);
        }


        public void CheatResetSuperDiscount()
        {
            CheatResetByType(TimeCycleType.SuperDiscountWeekly);
        }


        public void CheatResetFreePaidOffer()
        {
            CheatResetByType(TimeCycleType.FreePaidOfferWeekly);
        }


        public void CheatResetAll()
        {
            foreach (var item in cycles)
            {
                CheatResetItem(item.Value);
            }
        }


        private void CheatResetItem(CycleItem item)
        {
            item.Reset(DateTime.UtcNow.AddSeconds(-cheatSetValue), cheatSetValue);
        }


        private void CheatResetByType(TimeCycleType cycleType)
        {
            if (cycles.TryGetValue(cycleType, out CycleItem cycle))
            {
                CheatResetItem(cycle);
            }
        }
#endif

#endregion


    }
}