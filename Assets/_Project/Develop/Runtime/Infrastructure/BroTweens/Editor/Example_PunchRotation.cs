using UnityEngine;

namespace Infrastructure.BroTweens
{
    public class Example_PunchRotation : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 direction = new(0, 0, 45);

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
            seq.Append(BroTween.PunchRotation(target, direction, duration, shakeCount, elasticity));
            tween = seq.ToSafe();
            tween.Play();
        }
    }
}