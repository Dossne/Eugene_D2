using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Infrastructure.Utilities
{
    enum TargetAxis
    {
        X = 0,
        Z = 1
    }
    
    public class CircleObjectPlacer : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField, TriInspector.ReadOnly] private List<Transform> objects = new List<Transform>();
        [SerializeField] private int objectsCount = 8;
        [SerializeField] private float circleRadius = 0.5f;
        [SerializeField] private TargetAxis axis = TargetAxis.Z;
        [SerializeField] private GameObject prefab;
        
        [TriInspector.Button]
        private void SetByCircle()
        {
            // Проверяем корректность количества
            if (objectsCount <= 0)
            {
                Debug.LogError("Количество объектов должно быть больше 0!");
                return;
            }
            
            // Очищаем предыдущие объекты
            ClearPreviousObjects();
            objects.Clear();
            
            // Если префаб установлен, создаем объекты из него
            if (prefab != null)
            {
                // Создаем новые объекты из префаба
                for (int i = 0; i < objectsCount; i++)
                {
                    GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab, transform);
                    if (newObject != null)
                    {
                        objects.Add(newObject.transform);
                    }
                }
            }
            else
            {
                // Если префаб не установлен, работаем с существующими дочерними объектами
                foreach (Transform child in transform)
                {
                    objects.Add(child);
                }
                
                // Если дочерних объектов меньше чем нужно, выводим предупреждение
                if (objects.Count < objectsCount)
                {
                    Debug.LogWarning($"Недостаточно дочерних объектов! Найдено: {objects.Count}, требуется: {objectsCount}. Установите префаб или добавьте объекты вручную.");
                }
                
                // Если объектов больше чем нужно, используем только первые objectsCount
                if (objects.Count > objectsCount)
                {
                    objects.RemoveRange(objectsCount, objects.Count - objectsCount);
                }
                
                if (objects.Count == 0)
                {
                    Debug.LogError("Нет объектов для размещения! Установите префаб или добавьте дочерние объекты.");
                    return;
                }
            }
            
            // Размещаем объекты по кругу
            ArrangeObjectsInCircle();
        }
        
        [TriInspector.Button]
        private void ArrangeExistingChildren()
        {
            // Работаем только с существующими дочерними объектами
            objects.Clear();
            
            foreach (Transform child in transform)
            {
                objects.Add(child);
            }
            
            if (objects.Count == 0)
            {
                Debug.LogWarning("Нет дочерних объектов для размещения!");
                return;
            }
            
            // Размещаем объекты по кругу
            ArrangeObjectsInCircle();
        }
        
        private void ArrangeObjectsInCircle()
        {
            if (objects.Count == 0) return;
            
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i] == null) continue;
                
                float phase = 2 * Mathf.PI * i / objects.Count;
                
                // Позиционируем объект
                Vector3 circlePosition = new Vector3(
                    Mathf.Cos(phase) * circleRadius,
                    0f,
                    Mathf.Sin(phase) * circleRadius
                );
                
                objects[i].position = transform.position + circlePosition;
                
                // Направляем объект к центру
                Vector3 directionToCenter = (transform.position - objects[i].position).normalized;
                
                if (axis == TargetAxis.X)
                {
                    // Направляем локальную ось X к центру
                    Vector3 right = directionToCenter;
                    Vector3 up = Vector3.up;
                    Vector3 forward = Vector3.Cross(right, up).normalized;
                    
                    objects[i].rotation = Quaternion.LookRotation(forward, up);
                }
                else // TargetAxis.Z
                {
                    // Направляем локальную ось Z к центру
                    objects[i].rotation = Quaternion.LookRotation(directionToCenter, Vector3.up);
                }
            }
        }

        [TriInspector.Button]
        private void ClearAll()
        {
            ClearPreviousObjects();
            objects.Clear();
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
        
        // Для визуализации в Scene View
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, circleRadius);
            
            // Показываем предполагаемые позиции объектов
            if (objectsCount > 0)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < objectsCount; i++)
                {
                    float phase = 2 * Mathf.PI * i / objectsCount;
                    Vector3 pos = transform.position + new Vector3(
                        Mathf.Cos(phase) * circleRadius,
                        0f,
                        Mathf.Sin(phase) * circleRadius
                    );
                    Gizmos.DrawWireSphere(pos, 0.1f);
                }
            }
        }
#endif
    }
}