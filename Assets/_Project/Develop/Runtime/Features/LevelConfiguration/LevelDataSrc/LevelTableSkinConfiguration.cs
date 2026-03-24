using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Features.LevelConfiguration
{
    [CreateAssetMenu(fileName = "LevelTableSkinConfiguration", menuName = "Config/Game/LevelTableSkinConfiguration")]
    public class LevelTableSkinConfiguration : ScriptableObject
    {
        [SerializeField] private SerializedDictionary<LevelTableSkin, LevelTableSkinData> skins = new();

        public LevelTableSkinData GetLevelTableSkinData(LevelTableSkin skin)
        {
            if (skins.ContainsKey(skin))
                return skins[skin];
            return null;
        }

        public List<LevelTableSkin> GetSkins() => skins.Keys.ToList();
    }
}