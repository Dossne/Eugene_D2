using Infrastructure.Utilities;
using UnityEngine;

namespace Features.RewardTrack
{
    public class RewardTrackHudProvider : MonoBehaviour
    { 
        [SerializeField] private RewardTrackHudProgress hudProgress;

        public RewardTrackHudProgress HUDProgress => hudProgress;


        public void SetObjectActive(bool value)
        {
            gameObject.SetObjectActive(value);
        }
    }
}