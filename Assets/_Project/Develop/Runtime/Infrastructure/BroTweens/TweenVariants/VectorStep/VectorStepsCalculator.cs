using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.BroTweens
{
    public static class VectorStepsCalculator
    {
        public static void GetRandomShakes(List<VectorStep> outputResult, in Vector3 inputDefault, in float duration, Vector3 inputStrength, in int countPerSec, in float randomness, in bool fadeOut,
                                           in bool isFullRandomness)
        {
            outputResult.Clear();
            float strength = inputStrength.magnitude;

            int shakeCount = (int)( (double)countPerSec * (double)duration);

            if (shakeCount < 2)
                shakeCount = 2;

            float perStepStrength = strength / (float)shakeCount;
            float rawTotalDuration = 0.0f;

            for (int i = 0; i < shakeCount; ++i)
            {
                float normalizedIdx = (float)(i + 1) / (float)shakeCount;
                float segmentDuration = fadeOut ? duration * normalizedIdx : duration / (float)shakeCount;
                rawTotalDuration += segmentDuration;

                var result = new VectorStep
                {
                    duration = segmentDuration
                };

                outputResult.Add(result);
            }

            float normalizationFactor = duration / rawTotalDuration;

            for (int i = 0; i < shakeCount; ++i)
            {
                VectorStep item = outputResult[i];
                item.duration *= normalizationFactor;
                outputResult[i] = item;
            }

            float degrees = Random.Range(0.0f, 360f);

            for (int i = 0; i < shakeCount; ++i)
            {
                if (i < shakeCount - 1)
                {
                    float fromRandom = isFullRandomness ? -randomness : 0.0f;

                    if (i > 0)
                        degrees = degrees - 180f + Random.Range(fromRandom, randomness);

                    Quaternion quaternion = Quaternion.AngleAxis(Random.Range(fromRandom, randomness), Vector3.up);

                    Vector3 vector = quaternion * Vector3FromAngle(degrees, strength);
                    vector.x = Vector3.ClampMagnitude(vector, inputStrength.x).x;
                    vector.y = Vector3.ClampMagnitude(vector, inputStrength.y).y;
                    vector.z = Vector3.ClampMagnitude(vector, inputStrength.z).z;
                    vector = vector.normalized * strength;

                    VectorStep item = outputResult[i];
                    item.target = inputDefault + vector;
                    outputResult[i] = item;

                    if (fadeOut)
                        strength -= perStepStrength;

                    inputStrength = Vector3.ClampMagnitude(inputStrength, strength);
                }
                else
                {
                    VectorStep item = outputResult[i];
                    item.target = inputDefault;
                    outputResult[i] = item;
                }
            }
        }

        public static void GetRandomPunches(List<VectorStep> outputResult, in Vector3 inputDefault, in Vector3 direction, in float duration, in int countPerSec, float elasticity)
        {
            outputResult.Clear();

            if ((double)elasticity > 1.0)
                elasticity = 1f;
            else if ((double)elasticity < 0.0)
                elasticity = 0.0f;

            float magnitude = direction.magnitude;
            int shakeCount = (int)((double)countPerSec * (double)duration);

            if (shakeCount < 2)
                shakeCount = 2;

            float perStepStrength = magnitude / (float)shakeCount;

            float rawTotalDuration = 0.0f;

            for (int i = 0; i < shakeCount; ++i)
            {
                float normalizedIdx = (float)(i + 1) / (float)shakeCount;
                float segmentDuration = duration * normalizedIdx;
                rawTotalDuration += segmentDuration;
                var result = new VectorStep
                {
                    duration = segmentDuration
                };

                outputResult.Add(result);
            }

            float normalizationFactor = duration / rawTotalDuration;

            for (int i = 0; i < shakeCount; ++i)
            {
                VectorStep item = outputResult[i];
                item.duration *= normalizationFactor;
                outputResult[i] = item;
            }

            for (int i = 0; i < shakeCount; ++i)
            {
                Vector3 vector;
                if (i < shakeCount - 1)
                {
                    vector = i != 0
                        ? (i % 2 == 0 ? Vector3.ClampMagnitude(direction, magnitude) : -Vector3.ClampMagnitude(direction, magnitude * elasticity))
                        : direction;

                    magnitude -= perStepStrength;
                }
                else
                {
                    vector = Vector3.zero;
                }

                VectorStep item = outputResult[i];
                item.target = inputDefault + vector;
                outputResult[i] = item;
            }
        }

        private static Vector3 Vector3FromAngle(float degrees, float magnitude)
        {
            float f = degrees * (Mathf.PI / 180f);
            return new Vector3(magnitude * Mathf.Cos(f), magnitude * Mathf.Sin(f), 0.0f);
        }
    }
}