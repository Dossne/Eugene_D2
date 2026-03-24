using UnityEngine;

namespace LevelEditor
{
    [CreateAssetMenu(fileName = "LevelDebug", menuName = "Config/Tools/LevelDebug")]
    public class LevelDebug : ScriptableObject
    {
        [SerializeField] private string forceLevelId;

        [TriInspector.Button]
        public void SetLevelIdToLocalSave()
        {
            SetToLocalSave(forceLevelId);
            Debug.Log($"{forceLevelId} is set to local save as forceLevelId");
        }

        [TriInspector.Button]
        public void RemoveLevelIdFromLocalSave()
        {
            SetToLocalSave(null);
            Debug.Log("forceLevelId is removed from local save");
        }

        private void SetToLocalSave(string levelId)
        {
#if PR_CHEAT
            Infrastructure.PersistentProgress.SaveStorage saveStorage = new (null);
            saveStorage.Initialize();
            saveStorage.Progress.cheatState.forceLevelId = levelId;
            saveStorage.Save(); 
#endif
        }
    }
}