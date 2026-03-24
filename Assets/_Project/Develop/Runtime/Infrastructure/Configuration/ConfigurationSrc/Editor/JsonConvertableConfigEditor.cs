using LevelEditor;
using UnityEditor;
using UnityEngine;

namespace Infrastructure.Configuration
{
    [CustomEditor(typeof(JsonConvertableConfig), true), CanEditMultipleObjects]
    public class JsonConvertableConfigEditor : Editor
    {
        private Vector2 scrollPosition;
        private bool showPosition = false;
        private string status = "Show Json";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            DrawComponents();
        }


        protected virtual void DrawComponents()
        {
            JsonConvertableConfig myScript = target as JsonConvertableConfig;
            if (!myScript.IsRemote)
                return;

            GUILayout.Space(20.0f);

            NameForLauncher();

            FileNameLabel();

            GoogleSheetNameLabel();

            OnBeforeSpreadSheetButtons();
                
            GoogleSpreadSheetButtons();

            JsonTextArea();

            JsonCopyButton();
        }




        protected void FileNameLabel()
        {
            JsonConvertableConfig myScript = target as JsonConvertableConfig;

            if (myScript.IsLoadRequired || myScript.IsSaveRequired)
            {

            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("File Name", GUILayout.MaxWidth(145.0f));
            myScript.fileName = GUILayout.TextField(myScript.fileName);
            EditorGUILayout.EndHorizontal();

            EditorUtility.SetDirty(myScript);
        }

        protected void NameForLauncher()
        {
            JsonConvertableConfig myScript = target as JsonConvertableConfig;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Remote name: ", GUILayout.MaxWidth(145.0f));
            GUILayout.Label(myScript.name);
            if (GUILayout.Button("Copy", GUILayout.MaxWidth(150.0f)))
            {
                EditorGUIUtility.systemCopyBuffer = myScript.name;
            }
            EditorGUILayout.EndHorizontal();
        }


        protected void GoogleSheetNameLabel()
        {
            JsonConvertableConfig myScript = target as JsonConvertableConfig;

            if (myScript.IsLoadRequired
                || myScript.IsSaveRequired)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Sheet Name", GUILayout.MaxWidth(145.0f));
                myScript.GoogleSheetName = GUILayout.TextField(myScript.GoogleSheetName);
                GUILayout.EndHorizontal();
            }
        }


        protected void GoogleSpreadSheetButtons()
        {
            JsonConvertableConfig myScript = target as JsonConvertableConfig;

            GUILayout.BeginHorizontal();


            if (myScript.IsVerifyRequired
                && GUILayout.Button("Verify", GUILayout.Height(40.0f)))
            {
                myScript.Verify_Editor();
            }


            if (myScript.IsLoadRequired
                && GUILayout.Button("IMPORT in Unity", GUILayout.Height(40.0f)))
            {
                myScript.FromGoogleSpreadSheet_Editor();
                EditorUtility.SetDirty(myScript);
            }

            if (myScript.IsSaveRequired
                && GUILayout.Button("EXPORT To Sheets", GUILayout.Height(40.0f), GUILayout.MaxWidth(150.0f)))
            {
                myScript.ToGoogleSpreadSheet_Editor();
            }
            GUILayout.EndHorizontal();
        }


        protected void JsonTextArea()
        {
            JsonConvertableConfig myScript = target as JsonConvertableConfig;

            GUILayout.Space(10.0f);

            showPosition = EditorGUILayout.Foldout(showPosition, status);
            if (showPosition)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
                GUILayout.TextArea(myScript.GetJson_Editor(), GUILayout.MaxHeight(300));
                GUILayout.EndScrollView();
                status = "Hide Json";
            }
            else
            {
                status = "Show Json";
            }
        }


        protected void JsonCopyButton()
        {
            JsonConvertableConfig myScript = target as JsonConvertableConfig;

            if (GUILayout.Button("Copy Json to Clipboard", GUILayout.Height(40.0f)))
            {
                GUIUtility.systemCopyBuffer = myScript.GetJson_Editor();
            }
        }
        
        protected virtual void OnBeforeSpreadSheetButtons() { }
    }
    
    [CustomEditor(typeof(LevelIdRenamer)), CanEditMultipleObjects]
    public class ELevelIdRenamerEditor : JsonConvertableConfigEditor
    {
        protected override void DrawComponents()
        {
            GUILayout.Space(20.0f);

            FileNameLabel();

            GoogleSheetNameLabel();

            SceneButtons();

            GoogleSpreadSheetButtons();

            JsonTextArea();

            JsonCopyButton();
        }


        private void SceneButtons()
        {
            LevelIdRenamer myScript = target as LevelIdRenamer;

            NameForLauncher();

            GUILayout.Space(20.0f);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Rename", GUILayout.Height(40.0f)))
            {
                myScript.Rename();
                EditorUtility.SetDirty(myScript);
            }
            
            GUILayout.EndHorizontal();
        }
    }
}
