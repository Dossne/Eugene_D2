using System.Collections.Generic;
using System.Linq;
using Infrastructure.Utilities;
using UnityEngine;

namespace Features.LavaQuest
{
    public class LavaQuestEventZone : MonoBehaviour
    {
        [SerializeField] private int id;
        private List<LavaQuestPlatform> platforms;

#if UNITY_EDITOR
        [SerializeField, TriInspector.ShowIf("isDebugDraw")]
#endif
        private List<LavaQuestFallZone> fallZones;

        public int ID => id;
        public List<LavaQuestPlatform> Platform => platforms;
        public List<LavaQuestFallZone> FallZones => fallZones;

        private bool isInit;


        public void Initialize()
        {
            Collect();
            
            foreach (var platform in platforms)
            {
                platform.Initialize();
            }
        }


        public void SetObjectActive(bool value)
        {
            gameObject.SetObjectActive(value);
        }


        public bool TryGetPlatform(int idx, out LavaQuestPlatform platform)
        {
            if (idx < 0 || idx >= platforms.Count)
            {
                Debug.LogError($"Invalid platform index {idx}");
                platform = null;
                return false;
            }

            platform = platforms[idx];
            return true;
        }


        private void Collect()
        {
            platforms = GetComponentsInChildren<LavaQuestPlatform>(true).ToList().OrderBy(x => x.Step).ToList();
            fallZones = GetComponentsInChildren<LavaQuestFallZone>(true).ToList();
        }


#if UNITY_EDITOR
        [TriInspector.ValidateInput(nameof(ValidateFallZones))]
        [SerializeField] private bool isDebugDraw;
        [SerializeField] private Color debugColorFallZone = Color.white;


        private void OnDrawGizmos()
        {
            if (!isDebugDraw)
                return;

            Gizmos.color = debugColorFallZone;

            foreach (var fallZone in FallZones)
            {
                RectTransform root = fallZone.Root;
                Gizmos.DrawCube(root.position, new Vector3(root.rect.size.x, root.rect.size.y, 1f));
            }
        }

       
        
        public TriInspector.TriValidationResult ValidateFallZones()
        {
            if (!isDebugDraw)
                return TriInspector.TriValidationResult.Valid;

            if (fallZones == null || fallZones.Count == 0)
                return TriInspector.TriValidationResult.Error("Collect fall zones for debug draw").WithFix(Collect, "Collect");

            return TriInspector.TriValidationResult.Info("Do not forget to collect fall zones if added new").WithFix(Collect, "Collect");
        }
        
#endif

    }
}