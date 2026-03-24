using System;
using UnityEngine;

namespace Infrastructure.Animations
{
    public abstract class FloatingTextAnimatorBase : MonoBehaviour
    {
        public event Action OnStopPlay;

        [SerializeField] private GameObject mainRoot;

        public virtual float TotalTime { get; } = 0f;
        public abstract void StartPlay();
        public abstract void ForceStop();
        public abstract void ResetState();

        protected void StopPlay()
        {
            SetUpdateEnabled(false);
            OnStopPlay?.Invoke();
        }

        public void SetUpdateEnabled(bool value)
        {
            this.enabled = value;
        }

        protected void SetObjectActive(bool isActive)
        {
            if (mainRoot.activeSelf != isActive)
                mainRoot.SetActive(isActive);
        }
    }
}