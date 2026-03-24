using System;
using System.Collections.Generic;

using UnityEngine;

using Features.LevelComplete;

#if UNITY_EDITOR
using UnityEditor;
#endif



namespace Features.LevelConfiguration
{
    [CreateAssetMenu(fileName = "LevelDifficultyCache", menuName = "Config/Game/LevelDifficultyCache")]
    public class LevelDifficultyCache : ScriptableObject
    {
        [Serializable]
        public class LevelDifficultyData 
        {
            public string levelId;
            public LevelDifficulty levelDifficulty;
        }

        [SerializeField] private List<LevelDifficultyData> levelDifficultyData = new();

        public void Add(string lvlId, LevelDifficulty levelDifficulty)
        {
#if UNITY_EDITOR
            levelDifficultyData.Add(new() { levelId = lvlId, levelDifficulty = levelDifficulty });
            EditorUtility.SetDirty(this);
#endif
        }

        public void Clear() 
        {
#if UNITY_EDITOR
            levelDifficultyData.Clear();
            EditorUtility.SetDirty(this);
#endif
        }

        public bool TryGet(string lvlId, out LevelDifficulty levelDifficulty)
        {
            var data = levelDifficultyData.Find(x => x.levelId == lvlId);
            levelDifficulty = data != null ? data.levelDifficulty : LevelDifficulty.Default;
            return data != null;
        }
    }
}