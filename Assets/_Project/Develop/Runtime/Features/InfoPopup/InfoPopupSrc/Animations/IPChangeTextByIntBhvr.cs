using System;
using Infrastructure.BroTweens;
using Infrastructure.Pool.Particles;
using TMPro;
using UnityEngine;

namespace Features.InfoPopup
{
    /// <summary>
    /// ChangeText from defaultValue to targetValue => bounce => play fx
    /// </summary>
    public class IPChangeTextByIntBhvr : IPAnimationBehaviour
    {
        [SerializeField] private TextMeshProUGUI targetText;
        [SerializeField] private Transform targetBounce;
        [SerializeField] private ReusableParticleSystem vfx;
        [SerializeField] private IPChangeTextByIntAnimationParams animParams;
        private BroTweenSafe animSeq;
        private Action<int> changeTextDelegate;
        private Action finalizeTextDelegate;
        private Action vfxDelegate;
        
        private int fromValue;
        private int toValue;


        public void Construct(int fromValue, int toValue)
        {
            this.fromValue = fromValue;
            this.toValue = toValue;
        }


        public override void Initialize()
        {
            vfx.Initialize();

            changeTextDelegate = ChangeText;
            finalizeTextDelegate = () => targetText.text = toValue.ToString();
            vfxDelegate = () => vfx.Play();
        }


        public override void Deinitialize()
        {
            vfx.Deinitialize();

            animSeq.Kill();
            changeTextDelegate = null;
            finalizeTextDelegate = null;
            vfxDelegate = null;
        }


        public override void Prepare()
        {
            targetText.text = fromValue.ToString();
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
            seq.AppendInterval(animParams.startDelaySec);
            seq.Append(GetChangeText());
            seq.Append(GetScale());
            seq.AppendCallback(vfxDelegate);
            animSeq = seq.ToSafe();
        }


        private ScaleByCurveTween GetScale()
        {
            return BroTween.ScaleByCurve(targetBounce, Vector3.one, animParams.bounceDuration, animParams.bounceCurve);
        }


        private IntTween GetChangeText()
        {
            return BroTween.Int(changeTextDelegate, fromValue, toValue, animParams.changeDuration)
                           .SetEase(animParams.changeCurve)
                           .OnComplete(finalizeTextDelegate);
        }


        private void ChangeText(int currValue)
        {
            targetText.text = currValue.ToString();
        }
    }
}