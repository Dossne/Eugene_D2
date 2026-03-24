using System.Collections.Generic;
using Infrastructure.BroTweens;
using Infrastructure.HapticControl;
using UnityEngine;

namespace Features.InfoPopup
{
    public class IPShowPlaceBhvr : IPAnimationBehaviour
    {
        [SerializeField] private List<Transform> transforms;
        [SerializeField] private IPShowPlaceAnimationParams animParams;
        [SerializeField] private Vector3 initScale = Vector3.zero;

        private BroTweenSafe animSeq;

        private readonly List<Vector3> defaultPos = new();


        public override void Initialize()
        {
            for (var i = 0; i < transforms.Count; i++)
            {
                var target = transforms[i];

                if (target == null)
                {
                    Debug.LogError("Target is null", this);
                    continue;
                }

                defaultPos.Add(target.localPosition);
            }
        }


        public override void Deinitialize()
        {
            animSeq.Kill();
            defaultPos.Clear();
        }


        public override void Prepare()
        {
            for (var i = 0; i < transforms.Count; i++)
            {
                var target = transforms[i];

                if (target == null)
                {
                    Debug.LogError("Target is null", this);
                    continue;
                }

                target.localScale = initScale;
                target.localPosition = defaultPos[i] + animParams.startOffsetPos;
            }
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
            var seq  = BroTween.Sequence().SetUpdate(true);
            seq.AppendInterval(animParams.startDelaySec);

            for (var i = 0; i < defaultPos.Count; i++)
            {
                var target = transforms[i];

                if (target == null)
                {
                    Debug.LogError("Target is null", this);
                    continue;
                }


                float firstPhase = animParams.startDelaySec + i * animParams.itemDelaySec;
                seq.Insert(firstPhase, BroTween.Float(t=> ScaleByXYCurves(target, t), animParams.showScaleDurationSec));

                float secondPhase = firstPhase + animParams.showScaleDurationSec;
                seq.Insert(secondPhase, BroTween.PositionLocal(target, target.localPosition - animParams.startOffsetPos, animParams.moveDurationSec)
                                                .SetEase(animParams.moveCurve));

                if (animParams.doHaptics && i <= animParams.hapticCount)
                {
                    seq.InsertCallback(secondPhase, PlayHaptic);
                }

                seq.Insert(secondPhase, BroTween.Float(t=> BounceByXYCurves(target, t), animParams.bounceDurationSec));
            }

            animSeq = seq.ToSafe();
        }


        private void ScaleByXYCurves(Transform targetTransform, float t)
        {
            targetTransform.localScale = new Vector3(animParams.showScaleXCurve.Evaluate(t), animParams.showScaleYCurve.Evaluate(t), 1f);
        }

        
        private void BounceByXYCurves(Transform targetTransform, float t)
        {
            targetTransform.localScale = new Vector3(animParams.bounceScaleXCurve.Evaluate(t), animParams.bounceScaleYCurve.Evaluate(t), 1f);
        }
        
        
        private void PlayHaptic()
        {
            HapticService.I.Haptic(animParams.hapticType);
        }
    }
}