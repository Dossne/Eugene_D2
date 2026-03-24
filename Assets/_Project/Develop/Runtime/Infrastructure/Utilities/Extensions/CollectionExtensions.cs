using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Infrastructure.Utilities
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Chech whether IEnumerable is null or doesn't contain anything.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                return true;

            if (enumerable is ICollection<T> collection)
                return collection.Count == 0;

            return !enumerable.Any();
        }

        public static bool IsNullOrEmpty(this string str) =>
            string.IsNullOrWhiteSpace(str);

        public static T PickRandom<T>(this IEnumerable<T> collection)
        {
            T[] enumerable = collection as T[] ?? collection.ToArray();
            return enumerable[Random.Range(0, enumerable.Length)];
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, T toExcept)
        {
            return enumerable.Except(new[] { toExcept });
        }

        public static T ElementAtOrFirst<T>(this T[] array, int index)
        {
            return index < array.Length ? array[index] : array[0];
        }

        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> self)
        {
            return self ?? Enumerable.Empty<T>();
        }

        public static IEnumerable<T> NoNulls<T>(this IEnumerable<T> self)
        {
            return self.Where(element => element != null);
        }

        /// <summary>
        /// Removes last item in the list.
        /// </summary>
        public static void RemoveLastItem<T>(this List<T> list, Predicate<T> predicate = null)
        {
            if (list == null || list.Count == 0) return;

            var elementIndex = predicate == null ? list.Count - 1 : list.FindLastIndex(predicate);
            if (elementIndex >= 0)
                list.RemoveAt(elementIndex);
        }

        /// <summary>
        /// Returns last <paramref name="count"/> elements in the collection.
        /// In case collection length is less then <paramref name="count"/>, will return less elements.
        /// </summary>
        public static IEnumerable<T> TakeLast<T>(this IReadOnlyCollection<T> source, int count)
        {
            var skipCount = Mathf.Max(0, source.Count - count);
            return source.Skip(skipCount);
        }

        public static int GetArrayHashCode<T>(this T[] array)
        {
            return ArrayEqualityComparer<T>.GetHashCode(array);
        }

        public static bool IsIndexValid<T>(this T[] array, int index)
        {
            return array.Length > 0 && index >= 0 && index < array.Length;
        }

        public static bool IsIndexValid<T>(this List<T> list, int index)
        {
            return list.Count > 0 && index >= 0 && index < list.Count;
        }

        public static bool IsIndexValid<T>(this IReadOnlyCollection<T> list, int index)
        {
            return list.Count > 0 && index >= 0 && index < list.Count;
        }

        public static int IndexOf<T>(this IReadOnlyList<T> list, T itemToFind)
        {
            var i = 0;
            foreach (var item in list)
            {
                if (Equals(item, itemToFind)) return i;
                i++;
            }

            return -1;
        }

        public static int IndexOf<T>(this IList<T> list, Predicate<T> predicate)
        {
            var i = 0;
            foreach (var item in list)
            {
                if (predicate(item)) return i;
                i++;
            }

            return -1;
        }

        public static int IndexOf<T>(this IReadOnlyList<T> list, Predicate<T> predicate)
        {
            var i = 0;
            foreach (var item in list)
            {
                if (predicate(item)) return i;
                i++;
            }

            return -1;
        }

        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property,
            IEqualityComparer<TKey> propertyComparer = null)
        {
            var comparer = new GeneralPropertyComparer<T, TKey>(property, propertyComparer);
            return items.Distinct(comparer);
        }

        public static float ProgressOf<T>(this IList<T> list, T currentItem)
        {
            return list.IndexOf(currentItem) / (float)list.Count;
        }

        public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            (list[indexA], list[indexB]) = (list[indexB], list[indexA]);
            return list;
        }

        public static int RemoveAll<T>(this LinkedList<T> list, Predicate<T> match)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (match == null) throw new ArgumentNullException(nameof(match));

            var count = 0;
            var node = list.First;
            while (node != null)
            {
                var next = node.Next;
                if (match(node.Value))
                {
                    list.Remove(node);
                    count++;
                }

                node = next;
            }

            return count;
        }

        /// <summary>
        /// Orders the elements of <paramref name="source"/> collection in a way that no element depends on any previous element.
        /// </summary>
        /// <param name="source">The collection to order.</param>
        /// <param name="getDependencies">Function used to retrieve element's dependencies.</param>
        ///  <param name="warnCyclic">Whether to warn about cyclic dependencies.</param>
        /// <remarks>Based on: https://www.codeproject.com/Articles/869059/Topological-sorting-in-Csharp </remarks>
        public static IList<T> TopologicalOrder<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> getDependencies, bool warnCyclic = true)
        {
            var sorted = new List<T>();
            var visited = new Dictionary<T, bool>();

            foreach (var item in source)
                Visit(item);

            return sorted;

            void Visit(T item)
            {
                var alreadyVisited = visited.TryGetValue(item, out var inProcess);

                if (alreadyVisited)
                {
                    if (inProcess && warnCyclic)
                        Debug.LogWarning($"[LINQ_EXTENTION] Cyclic dependency found while performing topological ordering of {typeof(T).Name}.");
                }
                else
                {
                    visited[item] = true;

                    var dependencies = getDependencies(item);
                    if (dependencies != null)
                    {
                        foreach (var dependency in dependencies)
                            Visit(dependency);
                    }

                    visited[item] = false;
                    sorted.Add(item);
                }
            }
        }
    }
}