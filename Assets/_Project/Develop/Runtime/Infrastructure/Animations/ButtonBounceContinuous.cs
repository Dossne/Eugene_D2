using Infrastructure.BroTweens;
using UnityEngine;

namespace Infrastructure.Animations
{
    /// <summary>
    /// Cycle bounce
    /// </summary>
    public class ButtonBounceContinuous : MonoBehaviour
    {
        [SerializeField] public RectTransform target;
        [SerializeField] private AnimationCurve bounceCurve;
        [SerializeField] private float bounceDuration;
        [SerializeField] public float betweenDelay = 3f;
        [SerializeField] public int loopCount = -1; //-1 is infinite
        [SerializeField] private BroTweenSafe bounceLoop;
        private Vector3 defaultScale = Vector3.one;


        private void OnDisable()
        {
            KillLoop();
        }


        private void OnEnable()
        {
            PlayBounce();
        }


        private void Start()
        {
            if (target != null)
                defaultScale = target.localScale;
        }


        public void ResetTimer()
        {
            PlayBounce();
        }


        public void SetBounceRect(RectTransform rectTransform)
        {
            this.target = rectTransform;
            KillLoop();
            CreateLoop();
        }


        private void PlayBounce()
        {
            if (!bounceLoop.TryRewind())
            {
                KillLoop();
                CreateLoop();
            }

            bounceLoop.Play();
        }


        private void CreateLoop()
        {
            var seq = BroTween.SequenceLoop(betweenDelay, loopCount)
                              .Append(BroTween.ScaleByCurve(target, defaultScale, bounceDuration, bounceCurve))
                              .SetUpdate(true);
            
            bounceLoop = seq.ToSafe();
        }


        private void KillLoop()
        {
            bounceLoop.Kill();
        }
    }
}