
using System.Collections.Generic;
using UnityEngine;

namespace Features.LevelSequence
{
    public class ShuffledSelector<T>
    {
        private List<T> items;
        private List<int> shuffledIndices;
        private int currentIndex;
        private System.Random pseudoRandom;

        private T currentItem;
        private int cachedIndex;
        
        public ShuffledSelector(List<T> items)
        {
            this.items = new List<T>(items);
            Reset();
        }

        public ShuffledSelector(List<T> items, int seed)
        {
            this.items = new List<T>(items);
            pseudoRandom = new System.Random(seed);
            Reset();
        }

        public T GetRandomItem(int index)
        {
            // Доп логика для текущей реализации с тем что у нас случайный элемент берется в 3-х разных местах LevelTimeManager
            // LevelTaskManager и LevelCreateManager Чтобы он у всех был одинаковый - кэшируем пока не изменится индекс текущего уровня
            if (index == cachedIndex && currentItem != null)
            {
                return currentItem;
            }

            currentItem = GetRandomItem();
            cachedIndex = index;
            return currentItem;
        }

        private T GetRandomItem()
        {
            if (currentIndex >= shuffledIndices.Count)
            {
                Debug.LogWarning("Все элементы были выбраны!");
                Reset();
                return GetRandomItem();
            }

            T selectedItem = items[shuffledIndices[currentIndex]];
            currentIndex++;
            return selectedItem;
        }

        public void Reset()
        {
            // Создаем список индексов
            shuffledIndices = new List<int>();
            for (int i = 0; i < items.Count; i++)
            {
                shuffledIndices.Add(i);
            }

            // Перемешиваем по алгоритму Fisher-Yates
            for (int i = shuffledIndices.Count - 1; i > 0; i--)
            {
                int randomIndex = GetRandomIndex(0, i + 1);
                (shuffledIndices[i], shuffledIndices[randomIndex]) = (shuffledIndices[randomIndex], shuffledIndices[i]);
            }

            currentIndex = 0;
        }

        public void ResetWithSeed(int seed)
        {
            pseudoRandom = new System.Random(seed);
            Reset();
        }

        private int GetRandomIndex(int min, int max)
        {
            if (pseudoRandom != null)
            {
                return pseudoRandom.Next(min, max);
            }
            return Random.Range(min, max);
        }

        public bool HasItems => currentIndex < shuffledIndices.Count;
        public int RemainingCount => shuffledIndices.Count - currentIndex;
    }
}