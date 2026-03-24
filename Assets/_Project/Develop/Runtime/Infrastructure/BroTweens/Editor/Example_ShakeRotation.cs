using UnityEngine;

namespace Infrastructure.BroTweens
{
    public class Example_ShakeRotation : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 strength = new Vector3(0, 0, 45);

        [SerializeField] private float duration = 1f;
        [SerializeField] private int shakeCount = 15;
        [SerializeField] private float randomness = 1;
        [SerializeField] private bool fadeOut = true;
        [SerializeField] private bool isFullRandomness;
        [SerializeField] private int repeatCount = 1;
        [SerializeField] private float betweenDelay;

        [SerializeField, TriInspector.ReadOnly] private BroTweenSafe tween;


        [TriInspector.Button]
        public void Test_BroTween()
        {
            tween.Kill();
            var seq = BroTween.SequenceLoop(betweenDelay, repeatCount);
            seq.Append(BroTween.ShakeRotation(target, strength, duration, shakeCount, randomness, fadeOut, isFullRandomness));
            tween = seq.ToSafe();
            tween.Play();
        }
    }
}