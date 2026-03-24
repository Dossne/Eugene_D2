using UnityEngine;

using Features.LevelComplete;
using Infrastructure.Ads;


namespace Features.LevelConfiguration
{
    [CreateAssetMenu(fileName = "LevelScriptableObject", menuName = "Config/Game/LevelScriptableObject")]
    public class LevelScriptableObject : ScriptableObject
    {
        public string id;
        public string levelJson;
        public string taskJson;
        public float  time;
        public LevelDifficulty difficulty;
        public LevelTableSkin defaultSkin;

        public void FromLevelData(LevelData data) 
        {
            id          = data.id;
            levelJson   = data.levelJson;
            taskJson    = data.taskJson;
            time        = data.time;
            difficulty  = data.difficulty;
            defaultSkin = data.defaultSkin;
        }

        public (LevelData levelData, Level level) GetLevel() 
        {
            LevelData levelData = ToLevelData();
            Level level = null;

            if (!LevelUtil.TryGetLevel(id, levelData.levelJson, out level))
            {
                string errorMessage = $"Level <b>{id}</b> is broken!";
                Debug.LogError(errorMessage);
                AnalyticSender.SendEvent("vortex_unity_exception", errorMessage);
            }            
            return (levelData, level);
        }
        
        public LevelData ToLevelData()
        {
            LevelData data = new();
            data.id          = id         ;
            data.levelJson   = levelJson  ;
            data.taskJson    = taskJson   ;
            data.time        = time       ;
            data.difficulty  = difficulty ;
            data.defaultSkin = defaultSkin;
            return data;
        }
    }
}