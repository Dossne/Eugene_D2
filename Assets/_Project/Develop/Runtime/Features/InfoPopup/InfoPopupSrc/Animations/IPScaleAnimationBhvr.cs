using Infrastructure.BroTweens;
using TriInspector;
using UnityEngine;

namespace Features.InfoPopup
{
    public class IPScaleAnimationBhvr : IPAnimationBehaviour
    {
        [SerializeField] private bool useGlobal = true;
        [SerializeField, ShowIf("useGlobal")] private IPScaleAnimationGlobal globalParam;
        [SerializeField, HideIf("useGlobal")] private IPScaleAnimationParams individualParams;
        [SerializeField] private RectTransform target;
        [SerializeField] private Vector3 initScale = Vector3.zero;
        private BroTweenSafe animSeq;


        public override void Initialize()
        {
            if (target == null)
            {
                Debug.LogError("Target is null", this);
            }
        }


        public override void Deinitialize()
        {
            animSeq.Kill();
        }


        public override void Prepare()
        {
            if (target == null)
            {
                Debug.LogError("Target is null", this);
                return;
            }

            target.localScale = initScale;
        }


        public override void Play()
        {
            CreateAnimation();
            animSeq.Play();
        }


        public override void Stop()
        {
            animSeq.Kill();
        }


        private void CreateAnimation()
        {
            var seq = BroTween.Sequence().SetUpdate(true);
            IPScaleAnimationParams param = useGlobal ? globalParam.param : individualParams;
            seq.AppendInterval(param.startDelaySec);
            seq.Append(BroTween.ScaleByCurve(target, Vector3.one, param.durationSec, param.curve));
            animSeq = seq.ToSafe();
        }
    }
}