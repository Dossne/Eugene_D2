using Cysharp.Threading.Tasks;
using Features.RewardTrack;
using Features.ScrollList;
using UnityEngine;

namespace Features.Competition
{
    public class CompetitionRewardTrackTooltipScroller : ScrollElementList<RewardTrackStateUiElement>
    {
        public async UniTask InstantScrollAsync(int toIdx, float bottomOffset)
        {
            await UniTask.DelayFrame(2, cancellationToken: gameObject.GetCancellationTokenOnDestroy());
            var to = GetContentRootPositionY(toIdx, bottomOffset);
            scrollRect.StopMovement();
            SetContentRootPosition(new Vector2(0, to));
        }

        private float GetContentRootPositionY(int targetIdx, float bottomOffset)
        {
            var offset = scrollRect.viewport.rect.height * 0.5f + bottomOffset;
            return GetPosition(targetIdx, offset);
        }
    }
}