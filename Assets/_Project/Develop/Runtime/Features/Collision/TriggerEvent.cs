using System;
using UnityEngine;

namespace Features.Collision
{
    public class TriggerEvent : MonoBehaviour
    {

        public event Action<Collider> OnEnter;
        public event Action<Collider> OnExit;

        private void OnTriggerEnter(Collider other)
        {
            OnEnter?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            OnExit?.Invoke(other);
        }
    }
}