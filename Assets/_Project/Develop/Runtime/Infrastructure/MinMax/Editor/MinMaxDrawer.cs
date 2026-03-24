using UnityEditor;
using UnityEngine;
using Infrastructure.MinMax;


[CustomPropertyDrawer(typeof(MinMax<>))]
public class MinMaxDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position = EditorGUI.PrefixLabel(position, label);

        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty minProp = property.FindPropertyRelative("min");
        SerializedProperty maxProp = property.FindPropertyRelative("max");

        float labelWidth = 50f;
        float spacing = 8f;
        float fieldWidth = (position.width - labelWidth * 2 - spacing - 4) / 2;

        Rect minLabelRect = new Rect(position.x, position.y, labelWidth, position.height);
        Rect minFieldRect = new Rect(minLabelRect.xMax, position.y, fieldWidth, position.height);

        Rect maxLabelRect = new Rect(minFieldRect.xMax + spacing, position.y, labelWidth, position.height);
        Rect maxFieldRect = new Rect(maxLabelRect.xMax, position.y, fieldWidth, position.height);

        EditorGUI.LabelField(minLabelRect, "Min");
        EditorGUI.PropertyField(minFieldRect, minProp, GUIContent.none);

        EditorGUI.LabelField(maxLabelRect, "Max");
        EditorGUI.PropertyField(maxFieldRect, maxProp, GUIContent.none);

        EditorGUI.EndProperty();
    }
}
