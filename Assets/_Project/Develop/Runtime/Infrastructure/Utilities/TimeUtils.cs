using System;
using System.Globalization;
using Infrastructure.Localization;
using UnityEngine;

namespace Infrastructure.Utilities
{
    public class TimeUtils
    {
        private static string dayLoc = LocalizationService.I.Get(LocKeys.Utils.TimeD);
        private static string dayEditor = "d";

        public static string hour = LocalizationService.I.Get(LocKeys.Utils.TimeH);
        public static string minute = LocalizationService.I.Get(LocKeys.Utils.TimeM);
        private static string second = LocalizationService.I.Get(LocKeys.Utils.TimeS);

        public const double WEEK_SECONDS = 604800;

        /// <summary>
        /// Get time string converted from seconds
        /// </summary>
        /// <param name="seconds">Seconds to convert</param>
        /// <param name="lettersSizePerc">Scale of letters m, d, s in percent. 100 is default font size</param>
        /// <param name="isHHMMSSformat"></param>
        public static string GetTimeString(double seconds, float lettersSizePerc, bool isHHMMSSformat = false, bool showMinorZeroValues = true)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);

            if (isHHMMSSformat)
            {
                int totalHours = Mathf.FloorToInt((float)timeSpan.TotalHours);

                return totalHours > 0
                    ? $"{totalHours:D2} {hour} {timeSpan.Minutes:D2} {minute} {timeSpan.Seconds:D2} {second}"
                    : $"{timeSpan.Minutes:D2} {minute} {timeSpan.Seconds:D2} {second}";
            }

            string lettersScaleStr = lettersSizePerc.ToString(CultureInfo.InvariantCulture);

            if (timeSpan.Days > 0)
            {
                string timeString = $"{timeSpan.Days:D1}<size={lettersScaleStr}%>{dayLoc}</size>";
                if (timeSpan.Hours > 0)
                {
                    timeString += $" {timeSpan.Hours:D1}<size={lettersScaleStr}%>{hour}</size>";
                }

                return timeString;
            }

            if (timeSpan.Hours > 0)
            {
                string timeString = $"{timeSpan.Hours:D1}<size={lettersScaleStr}%>{hour}</size>";
                if (showMinorZeroValues || timeSpan.Minutes > 0)
                {
                    timeString += $" {timeSpan.Minutes:D2}<size={lettersScaleStr}%>{minute}</size> ";
                }

                return timeString;
            }

            else if (timeSpan.Minutes > 0)
            {
                string timeString = $"{timeSpan.Minutes:D2}<size={lettersScaleStr}%>{minute}</size>";
                if (showMinorZeroValues || timeSpan.Seconds > 0)
                {
                    timeString += $" {timeSpan.Seconds:D2}<size={lettersScaleStr}%>{second}</size> ";
                }

                return timeString;
            }

            return $"{timeSpan.Seconds:D2}<size={lettersScaleStr}%>{second}</size>";
        }


        /// <summary>
        /// Get time string converted from seconds
        /// </summary>
        /// <param name="seconds">Seconds to convert</param>       
        public static string GetTimeString(double seconds, string delimiter = ":", bool trimFirstZero = false)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);

            int totalHours = Mathf.FloorToInt((float)timeSpan.TotalHours);

            if (trimFirstZero)
            {
                return totalHours > 0
                    ? $"{totalHours:D1}{delimiter}{timeSpan.Minutes:D2}{delimiter}{timeSpan.Seconds:D2}"
                    : $"{timeSpan.Minutes:D1}{delimiter}{timeSpan.Seconds:D2}";
            }
            else 
            {
                return totalHours > 0
                    ? $"{totalHours:D2}{delimiter}{timeSpan.Minutes:D2}{delimiter}{timeSpan.Seconds:D2}"
                    : $"{timeSpan.Minutes:D2}{delimiter}{timeSpan.Seconds:D2}";
            }                
        }


        public static string GetTimeString(TimeSpan timeSpan, string delimiter = ":", bool isEditor = false)
        {
            int totalDays = Mathf.FloorToInt((float)timeSpan.TotalDays);
            if (totalDays > 0)
            {
                var dStr = isEditor ? dayEditor : dayLoc;
                return $"{totalDays}{dStr}. {timeSpan.Hours:D2}{delimiter}{timeSpan.Minutes:D2}{delimiter}{timeSpan.Seconds:D2}";
            }

            int totalHours = Mathf.FloorToInt((float)timeSpan.TotalHours);

            return totalHours > 0
                ? $"{totalHours:D2}{delimiter}{timeSpan.Minutes:D2}{delimiter}{timeSpan.Seconds:D2}"
                : $"{timeSpan.Minutes:D2}{delimiter}{timeSpan.Seconds:D2}";
        }


        public static string GetStringFromDateTime(DateTime dateTime)
        {
            return $"{dateTime:HH:mm:ss dd-MM-yyyy}";
        }


        /// <summary>
        /// Get next day from checkTime with targetHour, targetMinute, targetSecond. Result in UTC
        /// </summary>
        public static DateTime GetNextDay(DateTime checkTime, int targetHour, int targetMinute, int targetSecond)
        {
            DateTime result = new DateTime(checkTime.Year, checkTime.Month, checkTime.Day, targetHour, targetMinute, targetSecond, DateTimeKind.Utc);

            if (result <= checkTime)
                result = result.AddDays(1);

            return result;
        }


        /// <summary>
        /// Get next day from checkTime with target day of week, targetHour, targetMinute, targetSecond. Result in UTC
        /// </summary>
        public static DateTime GetNextWeekDay(DateTime checkDate, DayOfWeek targetDayOfWeek, int targetHour, int targetMinute, int second,
                                              bool isIncludeCheckTime = false)
        {
            int daysUntilTarget = ((int)targetDayOfWeek - (int)checkDate.DayOfWeek + 7) % 7;
            DateTime result =
                new DateTime(checkDate.Year,
                             checkDate.Month,
                             checkDate.Day,
                             targetHour,
                             targetMinute,
                             second,
                             DateTimeKind.Utc)
                   .AddDays(daysUntilTarget);

            if (isIncludeCheckTime)
            {
                if (result < checkDate)
                    result = result.AddDays(7);
            }
            else
            {
                if (result <= checkDate)
                    result = result.AddDays(7);
            }

            return result;
        }


        /// <summary>
        /// Get next first day of month from checkTime with targetHour, targetMinute, targetSecond. Result in UTC
        /// </summary>
        public static DateTime GetNextMonthFirstDay(DateTime checkTime, int targetHour, int targetMinute, int targetSecond)
        {
            DateTime result = new DateTime(checkTime.Year, checkTime.Month, 1, targetHour, targetMinute, targetSecond, DateTimeKind.Utc);

            if (result <= checkTime)
                result = result.AddMonths(1);

            return result;
        }


        public static bool IsBetween(DayOfWeek check, DayOfWeek start, DayOfWeek end)
        {
            if (start <= end)
            {
                return start <= check && check <= end;
            }

            return start <= check || check <= end;
        }
        
        public static DateTime GetDayOfWeekFrom(DateTime fromDate, DayOfWeek targetDayOfWeek, int targetHour, int targetMinute, int second)
        {
            int daysUntilTarget = ((int)targetDayOfWeek - (int)fromDate.DayOfWeek + 7) % 7;
            
            return new DateTime(fromDate.Year,
                                fromDate.Month,
                                fromDate.Day,
                                targetHour,
                                targetMinute,
                                second,
                                fromDate.Kind)
                   .AddDays(daysUntilTarget);
        }
    }
}