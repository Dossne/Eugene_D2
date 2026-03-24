using Infrastructure.Configuration;
using UnityEditor;
using UnityEngine;

namespace LevelEditor
{
    [CustomEditor(typeof(LevelPrefabsShowHide), true)]
    public class LevelPrefabsShowHideEditor : JsonConvertableConfigEditor
    {
        protected override void DrawComponents()
        {
            GUILayout.Space(20.0f);

            FileNameLabel();

            GoogleSheetNameLabel();

            GoogleSpreadSheetButtons();

            ShowCustomButtons();
        }


        private void ShowCustomButtons()
        {
            LevelPrefabsShowHide myScript = target as LevelPrefabsShowHide;

            DrawSetVisible_Move(myScript);
            DrawSetVisible_Copy(myScript);
            DrawHide_List(myScript);

            DrawClearList(myScript);
            DrawOpenHiddenFolder(myScript);
            
            DrawShow_All(myScript);
            DrawHide_All(myScript);
        }


        private void DrawSetVisible_Move(LevelPrefabsShowHide myScript)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("SET VISIBLE (MOVE)", GUILayout.Height(30.0f)))
            {
                myScript.SetPrefabsVisible();
                EditorUtility.SetDirty(myScript);
            }

            GUILayout.EndHorizontal();
        }


        private void DrawSetVisible_Copy(LevelPrefabsShowHide myScript)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("SET VISIBLE (COPY HIDDEN)", GUILayout.Height(30.0f)))
            {
                myScript.CopyHiddenPrefabsByList();
                EditorUtility.SetDirty(myScript);
            }

            GUILayout.EndHorizontal();
        }


        private void DrawHide_List(LevelPrefabsShowHide myScript)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("HIDE", GUILayout.Height(30.0f)))
            {
                myScript.HidePrefabsByList();
                EditorUtility.SetDirty(myScript);
            }

            GUILayout.EndHorizontal();
        }


        private void DrawClearList(LevelPrefabsShowHide myScript)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("CLEAR LIST", GUILayout.Height(30.0f)))
            {
                myScript.ClearList();
                EditorUtility.SetDirty(myScript);
            }

            GUILayout.EndHorizontal();
        }


        private void DrawOpenHiddenFolder(LevelPrefabsShowHide myScript)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("OPEN HIDDEN FOLDER", GUILayout.Height(30.0f)))
            {
                myScript.OpenHiddenFolder();
                EditorUtility.SetDirty(myScript);
            }

            GUILayout.EndHorizontal();
        }


        private void DrawShow_All(LevelPrefabsShowHide myScript)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("SHOW ALL (MOVE)", GUILayout.Height(50.0f)))
            {
                myScript.ShowAllPrefabs();
                EditorUtility.SetDirty(myScript);
            }

            GUILayout.EndHorizontal();
        }
        
        
        private void DrawHide_All(LevelPrefabsShowHide myScript)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("HIDE ALL (MOVE)", GUILayout.Height(50.0f)))
            {
                myScript.HideAllPrefabs();
                EditorUtility.SetDirty(myScript);
            }

            GUILayout.EndHorizontal();
        }
    }
}