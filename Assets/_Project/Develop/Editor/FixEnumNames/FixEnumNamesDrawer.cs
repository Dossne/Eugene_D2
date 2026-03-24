using UnityEngine;
using UnityEditor;


[CustomPropertyDrawer(typeof(FixEnumNamesAttribute))]
public class FixEnumNamesDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Rect contentRect = EditorGUI.PrefixLabel(position, label);
        property.enumValueIndex = EditorGUI.Popup(contentRect, property.enumValueIndex, property.enumNames, EditorStyles.popup);
    }
}