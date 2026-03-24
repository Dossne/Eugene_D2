using UnityEngine;

namespace Infrastructure.BroTweens
{
    public class Example_PunchScale : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 direction = new(-0.25f, -0.25f, -0.25f);

        [SerializeField] private float duration = 1f;
        [SerializeField] private int shakeCount = 8;
        [SerializeField] private float elasticity = 1;
        [SerializeField] private int repeatCount = 1;
        [SerializeField] private float betweenDelay;

        [SerializeField, TriInspector.ReadOnly] private BroTweenSafe tween;



        [TriInspector.Button]
        public void Test_BroTween()
        {
            tween.Kill();
            var seq = BroTween.SequenceLoop(betweenDelay, repeatCount);
            seq.Append(BroTween.PunchScale(target, direction, duration, shakeCount, elasticity));
            tween = seq.ToSafe();
            tween.Play();
        }
    }
}