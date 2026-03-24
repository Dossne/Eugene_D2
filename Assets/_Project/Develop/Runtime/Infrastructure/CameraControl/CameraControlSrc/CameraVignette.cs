using Infrastructure.BroTweens;
using UnityEngine;

namespace Infrastructure.CameraControl
{
    public class CameraVignette : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Debug")]
        [SerializeField] private BroTweenSafe fadeSeq;


        public virtual void Show(AnimationCurve curve, float from, float to, float duration, bool yoyo)
        {
            PlayAnimation(curve, from, to, duration, yoyo, true);
        }


        public virtual void InstantHide()
        {
            Hide();
        }

        public void Hide()
        {
            KillFadeSequence();
            canvasGroup.alpha = 0;
        }

        public virtual void Hide(AnimationCurve curve, float from, float to, float duration)
        {
            PlayAnimation(curve, from, to, duration, false, true);
        }


        protected void PlayAnimation(AnimationCurve curve, float from, float to, float duration, bool yoyo, bool hideOnComplete)
        {
            KillFadeSequence();
            var seq = BroTween.SequenceLoop(1);
            seq.Append(BroTween.FadeCanvasGroup(canvasGroup, from, to, duration).SetEase(curve));
            
            if (yoyo)
            {
                seq.SetParams(0, -1, LoopType.Yoyo);
            }

            if (hideOnComplete)
            {
                seq.OnComplete(this, target => target.Hide());
            }
            
            fadeSeq = seq.ToSafe();
            fadeSeq.Play();
        }


        private void KillFadeSequence()
        {
            fadeSeq.Kill();
        }
    }
}