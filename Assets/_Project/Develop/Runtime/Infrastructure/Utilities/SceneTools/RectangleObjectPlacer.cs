using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Infrastructure.Utilities
{
    public class RectangleObjectPlacer : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField, TriInspector.ReadOnly] private List<Transform> objects;
        [SerializeField] private int NumberOfObjectsByX = 5;
        [SerializeField] private int NumberOfObjectsByY = 5;
        [SerializeField] private float OffsetByX = 1f;
        [SerializeField] private float OffsetByY = 1f;
        [SerializeField] private bool isFill = false;
        
        [SerializeField] private GameObject prefab;
        
        [TriInspector.Button]
        private void Arrange()
        {
            // Очищаем предыдущие объекты
            ClearPreviousObjects();
            
            // Проверяем наличие префаба
            if (prefab == null)
            {
                Debug.LogError("Prefab не установлен!");
                return;
            }
            
            // Проверяем корректность размеров
            if (NumberOfObjectsByX <= 0 || NumberOfObjectsByY <= 0)
            {
                Debug.LogError("Количество объектов должно быть больше 0!");
                return;
            }
            
            objects.Clear();
            
            if (isFill)
            {
                // Заполняем весь прямоугольник
                CreateFilledRectangle();
            }
            else
            {
                // Создаем только периметр
                CreateRectanglePerimeter();
            }
        }
        
        private void ClearPreviousObjects()
        {
            // Удаляем все дочерние объекты
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                DestroyImmediate(child.gameObject);
            }
        }
        
        private void CreateFilledRectangle()
        {
            // Создаем объекты для заполненного прямоугольника
            for (int y = 0; y < NumberOfObjectsByY; y++)
            {
                for (int x = 0; x < NumberOfObjectsByX; x++)
                {
                    CreateObjectAtPosition(x, y);
                }
            }
        }
        
        private void CreateRectanglePerimeter()
        {
            // Если размер 1x1, создаем только один объект
            if (NumberOfObjectsByX == 1 && NumberOfObjectsByY == 1)
            {
                CreateObjectAtPosition(0, 0);
                return;
            }
            
            // Создаем верхнюю и нижнюю стороны
            for (int x = 0; x < NumberOfObjectsByX; x++)
            {
                // Верхняя сторона
                CreateObjectAtPosition(x, 0);
                
                // Нижняя сторона (если высота больше 1)
                if (NumberOfObjectsByY > 1)
                {
                    CreateObjectAtPosition(x, NumberOfObjectsByY - 1);
                }
            }
            
            // Создаем левую и правую стороны (без углов, так как они уже созданы)
            for (int y = 1; y < NumberOfObjectsByY - 1; y++)
            {
                // Левая сторона
                CreateObjectAtPosition(0, y);
                
                // Правая сторона (если ширина больше 1)
                if (NumberOfObjectsByX > 1)
                {
                    CreateObjectAtPosition(NumberOfObjectsByX - 1, y);
                }
            }
        }
        
        private void CreateObjectAtPosition(int x, int y)
        {
            // Вычисляем позицию объекта
            Vector3 position = transform.position + new Vector3(
                x * OffsetByX, 
                0, 
                y * OffsetByY);
            
            // Создаем объект
            GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab, transform);
            newObject.transform.SetPositionAndRotation(position, Quaternion.identity);
            // Добавляем в список для отслеживания
            objects.Add(newObject.transform);
            
            // Даем понятное имя
            newObject.name = $"{prefab.name}_({x},{y})";
        }
        
        [TriInspector.Button]
        private void ClearAll()
        {
            ClearPreviousObjects();
            objects.Clear();
        }
#endif
    }
}