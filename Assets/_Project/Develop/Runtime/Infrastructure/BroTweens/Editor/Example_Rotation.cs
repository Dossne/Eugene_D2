using UnityEngine;

namespace Infrastructure.BroTweens
{
    public class Example_Rotation : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 from = new(0, 0, 0);
        [SerializeField] private Vector3 to = new(0, 0, 750);
        [SerializeField] private float duration = 1f;
        [SerializeField] private RotateMode mode;

        [SerializeField, TriInspector.ReadOnly] private BroTweenSafe tween;


        [TriInspector.Button]
        public void Test_Rotate_Vector()
        {
            tween.Kill();
            tween = BroTween.Rotation(target, from, to, duration, mode).ToSafe();
            tween.Play();
        }


        [TriInspector.Button]
        public void Test_RotateLocal_Vector()
        {
            tween.Kill();
            tween = BroTween.RotationLocal(target, from, to, duration, mode).ToSafe();
            tween.Play();
        }


        [TriInspector.Button]
        public void Test_Rotate_Quaternion()
        {
            tween.Kill();
            tween = BroTween.Rotation(target, Quaternion.identity, Quaternion.Euler(to), duration).ToSafe();
            tween.Play();
        }
        
        
        [TriInspector.Button]
        public void Test_RotateLocal_Quaternion()
        {
            tween.Kill();
            tween = BroTween.RotationLocal(target, Quaternion.identity, Quaternion.Euler(to), duration).ToSafe();
            tween.Play();
        }
    }
}