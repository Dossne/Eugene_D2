using System;
using UnityEngine;

namespace Features.Character
{
    [Serializable]
    public struct PhysicsState
    {
        public Vector3 val;
        public float time;
    }

    [Serializable]
    public class PhysicsInterpolationBuffer
    {
        private const int Min = 2;
        private const int Max = 4; // 4=>8=>16

        [TriInspector.ShowInInspector] private PhysicsState[] buffer = new PhysicsState[Max];
        [TriInspector.ShowInInspector] private int length;
        [TriInspector.ShowInInspector] private int startIdx;


        public void AddValue(Vector3 val)
        {
            float time = Time.time;

            int index = (startIdx + length) % Max;
            buffer[index] = new PhysicsState { val = val, time = time };

            if (length < Max)
                length++;
            else
                startIdx = (startIdx + 1) % Max;
        }


        public bool HaveValues()
        {
            return length >= Min;
        }


        public Vector3 GetValue()
        {
            if (length == 1)
                return buffer[startIdx].val;

            float renderTime = Time.time - Time.fixedDeltaTime;

            for (int i = 0; i < length - 1; i++)
            {
                int aIndex = (startIdx + i) % Max;
                int bIndex = (startIdx + i + 1) % Max;

                PhysicsState a = buffer[aIndex];
                PhysicsState b = buffer[bIndex];

                if (renderTime <= b.time)
                {
                    float t = Mathf.InverseLerp(a.time, b.time, renderTime);
                    return Vector3.Lerp(a.val, b.val, t);
                }
            }

            int last = (startIdx + length - 1) % Max;
            return buffer[last].val;
        }


        public void Clear()
        {
            Array.Clear(buffer, 0, buffer.Length);
            startIdx = 0;
            length = 0;
        }
    }
}