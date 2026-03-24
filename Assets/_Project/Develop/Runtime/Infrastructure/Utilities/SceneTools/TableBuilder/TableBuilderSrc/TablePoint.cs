using UnityEngine;


namespace Infrastructure.Utilities
{
    public class TablePoint : MonoBehaviour
    {
        private void OnValidate()
        {
            var pos = transform.position;
            pos.y = 0;
            transform.position = pos;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 1f);
        }
    }
}