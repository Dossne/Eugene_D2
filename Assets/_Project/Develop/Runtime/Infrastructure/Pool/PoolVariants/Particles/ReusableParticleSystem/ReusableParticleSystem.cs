using System;
using UnityEngine;

namespace Infrastructure.Pool.Particles
{
    public abstract class ReusableParticleSystem : MonoBehaviour
    {
        public abstract void Construct(Action stopCallback);
        public abstract void Initialize();
        public abstract void Deinitialize();
        public abstract void Play();
        public abstract void Stop();
        public abstract void Restart();
        
        
        protected void InitializeParticle(ParticleSystem ps)
        {
            ParticleSystem.MainModule main = ps.main;
            main.stopAction = ParticleSystemStopAction.Callback;
            main.playOnAwake = false;
        }
    }
}