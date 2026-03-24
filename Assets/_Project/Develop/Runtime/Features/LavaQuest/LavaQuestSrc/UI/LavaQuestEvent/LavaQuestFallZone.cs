using UnityEngine;

namespace Features.LavaQuest
{
    public class LavaQuestFallZone : MonoBehaviour
    {
        [SerializeField] private RectTransform root;

        public RectTransform Root => root;
    }
}