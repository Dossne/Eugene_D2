using System;
using TriInspector;

namespace Infrastructure.Utilities
{
    /// <summary>
    /// Serialize date time in unity editor
    /// </summary>
    [Serializable]
    public struct SerializableDateTime
    {
        [Title("Time")]
        [Dropdown(nameof(hourValues))] public int hour;
        [Dropdown(nameof(minuteValues))] public int minute;
        [Dropdown(nameof(minuteValues))] public int second;

        [Title("Date")]
        [Dropdown(nameof(dayValues))] public int day;
        [Dropdown(nameof(monthValues))] public int month;
        public int years;

        [Title("Other")]
        public int millisecond;
        public DateTimeKind kind;

        
        private static int[] monthValues = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

        private static int[] dayValues =
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
            11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31
        };

        private static int[] hourValues = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 };

        private static int[] minuteValues =
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
            10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
            20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
            30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
            40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
            50, 51, 52, 53, 54, 55, 56, 57, 58, 59
        };
    }
}