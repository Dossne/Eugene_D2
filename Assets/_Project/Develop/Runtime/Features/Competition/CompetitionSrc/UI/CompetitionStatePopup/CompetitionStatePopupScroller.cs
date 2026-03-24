using Cysharp.Threading.Tasks;
using Features.ScrollList;
using Infrastructure.BroTweens;
using UnityEngine;

namespace Features.Competition
{
    public class CompetitionStatePopupScroller : ScrollElementList<CompetitionSlotView>
    {
        public async UniTask InstantScrollAsync(int toIdx, float elementHeight)
        {
            await UniTask.DelayFrame(2, cancellationToken: gameObject.GetCancellationTokenOnDestroy());
            var to = GetContentRootPositionY(toIdx, elementHeight);
            scrollRect.StopMovement();
            SetContentRootPosition(new Vector2(0, to));
        }

        public float GetContentRootPositionY(int targetIdx, float elementHeight)
        {
            var offset = scrollRect.viewport.rect.height * 0.5f - elementHeight * 0.5f;
            return GetPosition(targetIdx, offset);
        }

        public BroTweenBase GetScrollTween(int toIdx, float elementHeight, float duration, AnimationCurve easeCurve)
        {
            var to = GetContentRootPositionY(toIdx, elementHeight);
            return GetScrollTween(new Vector2(0, to), duration, true, Ease.Default, easeCurve);
        }
    }
}