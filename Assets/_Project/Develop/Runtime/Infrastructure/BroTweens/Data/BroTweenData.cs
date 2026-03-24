using System;
using System.Runtime.InteropServices;

namespace Infrastructure.BroTweens
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BroTweenData
    {
        //4 = int, float
        public int uniqueId;
        public int generation;
        public int playVersion;
        public float duration;
        public float currentTime;
        public float delay;

        //1 = byte, bool
        public Ease ease;
        public BroTweenState state;
        public bool isLiveBetweenScenes;
        public bool isPlayBackwards;
        public bool isUnscaledDt;
        public bool isKillOnComplete;
        public bool isProcessing;
        public bool isInPool;
    }
}