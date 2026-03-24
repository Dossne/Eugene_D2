using UnityEngine;

namespace Features.InfoPopup
{
    public abstract class IPAnimationBehaviour : MonoBehaviour
    {
        public abstract void Initialize();

        public abstract void Deinitialize();

        public abstract void Prepare();

        public abstract void Play();

        public abstract void Stop();
    }
}