using AYellowpaper.SerializedCollections;
using Features.WinStreak;
using TriInspector;
using UnityEngine;

namespace Features.ScoringVisualize
{
    [CreateAssetMenu(fileName = "ScoringVisualizeConfig", menuName = "Config/System/ScoringVisualizeConfig")]
    public class ScoringVisualizeConfig : ScriptableObject
    {
        [Title("Features")]
        public float animationsBeginDelay = 0.5f;
        public SerializedDictionary<ScoringFeature, ScoringConfigData> scoringData = new();

        [Title("View")]
        public int maxIconCount = 11;
        public float betweenDelayMin;
        public float betweenDelayMax;
        public Vector2 minSpawnPosition;
        public Vector2 maxSpawnPosition;
        public ScoringIconAnimationParams iconAnimParams;

        [Title("Text")]
        public ScoringTextAnimationParams textAnimParams;
        public WinStreakLabelAnimation winStreakAnimParams;

        [Title("Widget")]
        public WidgetAnimationParams widgetAnimParams;
    }
}