using UnityEngine;

namespace Infrastructure.BroTweens
{
    public class Example_PunchPosition : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 direction = new(50, 50, 50);

        [SerializeField] private float duration = 0.5f;
        [SerializeField] private int shakeCount = 15;
        [SerializeField] private float elasticity = 1;
        [SerializeField] private int repeatCount = 1;
        [SerializeField] private float betweenDelay;

        [SerializeField, TriInspector.ReadOnly] private BroTweenSafe tween;


        [TriInspector.Button]
        public void Test_BroTween()
        {
            tween.Kill();
            var seq = BroTween.SequenceLoop(betweenDelay, repeatCount);
            seq.Append(BroTween.PunchPosition(target, direction, duration, true, shakeCount, elasticity));
            tween = seq.ToSafe();
            tween.Play();
        }
    }
}