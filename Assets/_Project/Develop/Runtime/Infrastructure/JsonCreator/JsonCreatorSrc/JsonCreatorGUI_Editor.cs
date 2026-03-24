#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Infrastructure.JsonCreator
{
    [CustomEditor(typeof(JsonCreator_Editor<>), true), CanEditMultipleObjects]
    public class JsonCreatorGUI_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            DrawComponents();
        }

        protected void DrawComponents()
        {
            ScriptableObject myScript = target as ScriptableObject;

            GUILayout.Space(20.0f);

            IJsonCreator creator = (IJsonCreator)target;
            if (GUILayout.Button("ToJson"))
            {
                creator.ToJson();
            }

            if (GUILayout.Button("FromJson"))
            {
                creator.FromJson();
            }
        }
    }
}

#endif