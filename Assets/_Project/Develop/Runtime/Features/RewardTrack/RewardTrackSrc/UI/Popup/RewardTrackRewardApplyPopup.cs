using Infrastructure.Popups;
using UnityEngine;

namespace Features.RewardTrack
{
    public class RewardTrackRewardApplyPopup : PopupBase
    {
        [SerializeField] private RectTransform startFlyTransform;

        public Vector3 StartFlyPosition => startFlyTransform.position;
    }
}