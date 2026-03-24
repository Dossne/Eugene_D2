using System;
using Infrastructure.BroTweens;
using Infrastructure.Pool.Particles;
using TMPro;
using UnityEngine;

namespace Features.InfoPopup
{

    public class IPChangeTextByArrayBhvr : IPAnimationBehaviour
    {
        [SerializeField] private TextMeshProUGUI target;
        [SerializeField] private IPChangeTextByArrayAnimationParams animParams;
        [SerializeField] private ReusableParticleSystem vfx;

        private BroTweenSafe animSeq;

        private Action<int> changeTextDelegate;
        private Action vfxDelegate;
        
        
        public override void Initialize()
        {
            if (target == null)
            {
                Debug.LogError("Target is null", this);
            }

            vfx.Initialize();
            changeTextDelegate = ChangeTextByIdx;
            vfxDelegate = () => vfx.Play();
        }


        public override void Deinitialize()
        {
            vfx.Deinitialize();
            animSeq.Kill();
            changeTextDelegate = null;
            vfxDelegate = null;
        }


        public override void Prepare()
        {
            if (target == null)
            {
                Debug.LogError("Target is null", this);
                return;
            }

            target.text = animParams.texts[0];
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
            BroSequence seq = BroTween.Sequence().SetUpdate(true);
            seq.AppendInterval(animParams.startDelaySec);
            seq.Append(GetChangeTextByCurveTween());

            for (int i = 0; i < animParams.bounces.Length; i++)
            {
                IPScaleAnimationParams currentBounce = animParams.bounces[i];
                seq.Insert(animParams.startDelaySec + currentBounce.startDelaySec,
                           BroTween.ScaleByCurve(target.transform, target.transform.localScale, currentBounce.durationSec, currentBounce.curve));
            }

            animSeq = seq.ToSafe();
        }


        private BroTweenBase GetChangeTextByCurveTween()
        {
            int to = animParams.texts.Length - 1;
            return BroTween.Int(changeTextDelegate, 0, to, animParams.changeTextDuration)
                           .SetEase(animParams.changeCurve)
                           .OnComplete(vfxDelegate);
        }


        private void ChangeTextByIdx(int currIdx)
        {
            target.text = animParams.texts[currIdx];
        }
    }
}