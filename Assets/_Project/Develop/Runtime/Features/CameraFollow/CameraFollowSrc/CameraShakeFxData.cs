using System;

namespace Features.CameraFollow
{
    [Serializable]
    public class CameraShakeFxData
    {
        public float duration;
        public float strength;
        public int vibrato;
        public float randomness;
        public bool fadeout;
        public bool isFullRandomness;
    }
}