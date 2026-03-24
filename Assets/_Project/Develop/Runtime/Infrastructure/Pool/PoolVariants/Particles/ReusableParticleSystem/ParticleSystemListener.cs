using System;
using UnityEngine;

namespace Infrastructure.Pool.Particles
{
    public class ParticleSystemListener : MonoBehaviour
    {
        private Action stopCallback;

        private void OnParticleSystemStopped()
        {
            stopCallback?.Invoke();
        }

        public void SetStopCallback(Action stopCallback)
        {
            this.stopCallback = stopCallback;
        }
    }
}