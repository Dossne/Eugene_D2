using Infrastructure.Configuration;
using UnityEditor;
using UnityEngine;

namespace Features.LevelConfiguration
{
    [CustomEditor(typeof(LevelConfig), true)]
    public class LevelConfigEditor : JsonConvertableConfigEditor
    {
        protected override void OnBeforeSpreadSheetButtons()
        {
            LevelConfig myScript = target as LevelConfig;


            GUILayout.Space(20.0f);
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Validate", GUILayout.Height(40.0f)))
            {
                myScript?.ValidateData_Editor();
            }

            GUILayout.EndHorizontal();
        }
    }
}