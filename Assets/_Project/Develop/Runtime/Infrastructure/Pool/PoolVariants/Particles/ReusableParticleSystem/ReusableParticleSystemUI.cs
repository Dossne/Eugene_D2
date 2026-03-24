using System;
using Coffee.UIExtensions;
using UnityEngine;

namespace Infrastructure.Pool.Particles
{
    /// <summary>
    /// USED ONLY FOR UI with CoffeeUi component!
    /// All CoffeeUi particles should be disabled when not in use. Otherwise, UiParticle will be managed by UIParticleUpdater every Canvas.willRenderCanvases.
    ///
    /// Usage:
    /// 1. Place ReusableParticleSystemUI on root where UIParticle component is;
    /// 2. Place ParticleSystemListener on main particleSystem within root;
    /// 3. Set StopAction = Callback on particle system where ParticleSystemListener is set;
    /// 4. Set root of particle as rootGameObject in Editor (usually where ReusableParticleSystemUI is set);
    ///
    /// OR just press SetupComponents button in editor
    /// </summary>
    public sealed class ReusableParticleSystemUI : ReusableParticleSystem
    {
        [SerializeField] private ParticleSystemListener psListener;
        [SerializeField] private UIParticle particle;

        private Action stopCallback;
        private bool partEnabled;
        private bool isInit;

        public override void Construct(Action stopCallback)
        {
            this.stopCallback = stopCallback;
        }

        public override void Initialize()
        {
            if (isInit)
                return;

            SetupComponents();
            stopCallback += () => SetParticleEnabled(false);
            psListener.SetStopCallback(stopCallback);
            particle.enabled = false;
            partEnabled = false;

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
            SetParticleEnabled(true);

            if (!particle.particles[0].IsAlive())
                particle.Play();
        }

#if UNITY_EDITOR
        [TriInspector.Button]
#endif
        public override void Stop()
        {
            particle.Stop();
            SetParticleEnabled(false);
        }

#if UNITY_EDITOR
        [TriInspector.Button]
#endif
        public override void Restart()
        {
            SetParticleEnabled(true);
            particle.Play();
        }

        private void SetParticleEnabled(bool value)
        {
            if (partEnabled == value)
                return;

            particle.enabled = value;
            partEnabled = value;
        }

#if UNITY_EDITOR
        [TriInspector.Button]
#endif
        private void SetupComponents()
        {
            if (particle == null)
            {
                particle = GetComponent<UIParticle>();
            }

            if (psListener == null)
            {
                var targetPs = GetComponentInChildren<ParticleSystem>(true);
                psListener = targetPs.gameObject.AddComponent<ParticleSystemListener>();
            }

            if (psListener != null && psListener.gameObject.TryGetComponent(out ParticleSystem ps))
            {
                InitializeParticle(ps);
            }
        }
    }
}