using System;
using UnityEngine;

namespace Infrastructure.Pool.Particles
{
    /// <summary>
    ///
    /// Usage:
    /// 1. Place ReusableParticleSystem on root;
    /// 2. Place ParticleSystemListener on main particleSystem within root;
    /// 3. Set StopAction = Callback on particle system where ParticleSystemListener is set;
    /// 4. Set root of particle as rootGameObject in Editor (usually where ReusableParticleSystem is set);
    ///
    /// OR just press SetupComponents button in editor
    /// </summary>
    public sealed class ReusableParticleSystem3d : ReusableParticleSystem
    {
        [SerializeField] private ParticleSystem particle;
        [SerializeField] private ParticleSystemListener psListener;

        private Action stopCallback;
        private bool isInit;

        public override void Construct(Action stopCallback)
        {
            this.stopCallback = stopCallback;
        }

        public override void Initialize()
        {
            if (isInit)
                return;

            psListener.SetStopCallback(stopCallback);
            SetupComponents();
            isInit = true;
        }

        public override void Deinitialize()
        {
            if (!isInit)
                return;

            stopCallback = null;
            isInit = false;
        }

#if UNITY_EDITOR
        [TriInspector.Button]
#endif
        public override void Play()
        {
            particle.Play();
        }

#if UNITY_EDITOR
        [TriInspector.Button]
#endif
        public override void Stop()
        {
            particle.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public override void Restart()
        {
            particle.Play();
        }

#if UNITY_EDITOR
        [TriInspector.Button]
#endif
        private void SetupComponents()
        {
            if (particle == null)
            {
                particle = GetComponentInChildren<ParticleSystem>(true);
            }

            if (psListener == null)
            {
                psListener = particle.gameObject.AddComponent<ParticleSystemListener>();
            }

            if (particle != null)
            {
                InitializeParticle(particle);
            }
        }
    }
}