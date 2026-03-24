using UnityEngine;

namespace Infrastructure.Utilities
{
    public class TableCorner : MonoBehaviour
    {
        public enum Orientation
        {
            Outer = 0,
            Inner = 1,
            Clockwise = 2,
            AntiClockwise = 3,
        }

        [SerializeField] private GameObject outer;
        [SerializeField] private GameObject inner;
        [SerializeField] private Orientation orientation;

        public Orientation CurrentOrientation => orientation;

        public void SetOrientation(Orientation orientation)
        {
            this.orientation = orientation;
            switch (orientation)
            {
                case Orientation.Outer:
                    inner.SetActive(false);
                    outer.SetActive(true);
                    break;
                case Orientation.Inner:
                    inner.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    inner.SetActive(true);
                    outer.SetActive(false);
                    break;
                case Orientation.Clockwise:
                    inner.transform.localRotation = Quaternion.Euler(0, 90, 0);
                    inner.SetActive(true);
                    outer.SetActive(false);
                    break;
                case Orientation.AntiClockwise:
                    inner.transform.localRotation = Quaternion.Euler(0, -90, 0);
                    inner.SetActive(true);
                    outer.SetActive(false);
                    break;
                default:
                    break;
            }
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void SetObjectActive(bool isActive)
        {
            if (gameObject.activeSelf == isActive)
                return;

            gameObject.SetActive(isActive);
        }

        private void Start()
        {
#if !UNITY_EDITOR
            enabled = false;
#endif
        }
    }
}