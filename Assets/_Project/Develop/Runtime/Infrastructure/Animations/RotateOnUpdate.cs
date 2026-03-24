using UnityEngine;

namespace Infrastructure.Animations
{
    public class RotateOnUpdate : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float xAngle;
        [SerializeField] private float yAngle;
        [SerializeField] private float zAngle;


        private void Update()
        {
            target.Rotate(xAngle, yAngle, zAngle, Space.Self);
        }
    }
}