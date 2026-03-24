using System;


namespace Infrastructure.Utilities
{
    public static class EnumExtensions
    {
        public static T Next<T>(this T src) where T : struct, Enum
        {
            T[] values = (T[])Enum.GetValues(src.GetType());
            int index = Array.IndexOf(values, src) + 1;
            return index == values.Length ? values[0] : values[index];
        }
    }
}

