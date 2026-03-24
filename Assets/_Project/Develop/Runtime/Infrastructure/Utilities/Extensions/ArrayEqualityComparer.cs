using System.Collections.Generic;

namespace Infrastructure.Utilities
{
    /// <summary>
    /// Allows comparing arrays using equality comparer of the array items.
    /// Type of the array items should provide a valid comparer for this to work.
    /// Implementation based on: https://stackoverflow.com/a/7244729/1202251
    /// </summary>
    /// <typeparam name="T">Type of the items contained in the array.</typeparam>
    public sealed class ArrayEqualityComparer<T> : IEqualityComparer<T[]>
    {
        private static readonly EqualityComparer<T> ITEMS_COMPARER = EqualityComparer<T>.Default;

        public static bool Equals(T[] first, T[] second)
        {
            if (first == second) return true;
            if (first == null || second == null) return false;
            if (first.Length != second.Length) return false;
            for (int i = 0; i < first.Length; i++)
                if (!ITEMS_COMPARER.Equals(first[i], second[i]))
                    return false;
            return true;
        }

        public static int GetHashCode(T[] array)
        {
            unchecked
            {
                if (array == null) return 0;
                var hash = 17;
                foreach (var item in array)
                    hash = hash * 31 + ITEMS_COMPARER.GetHashCode(item);
                return hash;
            }
        }

        bool IEqualityComparer<T[]>.Equals(T[] first, T[] second)
        {
            return Equals(first, second);
        }

        int IEqualityComparer<T[]>.GetHashCode(T[] array)
        {
            return GetHashCode(array);
        }
    }
}