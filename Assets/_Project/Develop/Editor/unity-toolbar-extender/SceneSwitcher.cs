using Infrastructure.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace unity_toolbar_extender
{
	static class ToolbarStyles
	{
		public static readonly GUIStyle commandButtonStyle;

		static ToolbarStyles()
		{
            commandButtonStyle = new GUIStyle("Command")
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageAbove,
                fontStyle = FontStyle.Bold,
                fixedWidth = 100
			};
		}
	}

	[InitializeOnLoad]
	public class SceneSwitchLeftButton
	{
        
        static SceneSwitchLeftButton()
		{
			ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
		}

		static void OnToolbarGUI()
		{
            string play = "PLAY";
            string stop = "STOP";

        GUILayout.FlexibleSpace();

            string ButtonText = EditorApplication.isPlaying ? stop : play;

            if (GUILayout.Button(new GUIContent(ButtonText, "Play Game from MainScene Scene"), ToolbarStyles.commandButtonStyle))
			{
				SceneHelper.StartScene(SceneNames.Main);
			}
		}
	}

	static class SceneHelper
	{
		static string sceneToOpen;

		public static void StartScene(string sceneName)
		{
			if(EditorApplication.isPlaying)
			{
				EditorApplication.isPlaying = false;
                return;
            }

			sceneToOpen = sceneName;
			EditorApplication.update += OnUpdate;
		}

		static void OnUpdate()
		{
			if (sceneToOpen == null || EditorApplication.isPlaying || EditorApplication.isPaused || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
			{
				return;
			}

			EditorApplication.update -= OnUpdate;

			if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			{
                    EditorSceneManager.OpenScene(UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(0));
                    EditorApplication.isPlaying = true;
			}
			sceneToOpen = null;
		}
	}
}
