using UnityEngine;

namespace Infrastructure.BroTweens
{
    internal class StandardEasing
    {
        private const float halfPi = Mathf.PI / 2f;
        private const float backEaseConst = 1.70158f;
        private const float defaultElasticEasePeriod = 0.3f;


        internal static float Evaluate(float t, Ease ease)
        {
            switch (ease)
            {
                case Ease.Default:
                    return t;
                case Ease.Linear:
                    return t;
                case Ease.InSine:
                    return 1 - Mathf.Cos(t * halfPi);
                case Ease.OutSine:
                    return Mathf.Sin(t * halfPi);
                case Ease.InOutSine:
                    return -0.5f * (Mathf.Cos(Mathf.PI * t) - 1);
                case Ease.InQuad:
                    return t * t;
                case Ease.OutQuad:
                    return -t * (t - 2);
                case Ease.InOutQuad:
                    t *= 2f;
                    if (t < 1)
                    {
                        return 0.5f * t * t;
                    }

                    return -0.5f * (--t * (t - 2) - 1);
                case Ease.InCubic:
                    return t * t * t;
                case Ease.OutCubic:
                    return (t -= 1) * t * t + 1;
                case Ease.InOutCubic:
                    t *= 2f;
                    if (t < 1)
                    {
                        return 0.5f * t * t * t;
                    }

                    return 0.5f * ((t -= 2) * t * t + 2);
                case Ease.InQuart:
                    return t * t * t * t;
                case Ease.OutQuart:
                    return -((t -= 1) * t * t * t - 1);
                case Ease.InOutQuart:
                    t *= 2f;
                    if (t < 1)
                    {
                        return 0.5f * t * t * t * t;
                    }

                    return -0.5f * ((t -= 2) * t * t * t - 2);
                case Ease.InQuint:
                    return t * t * t * t * t;
                case Ease.OutQuint:
                    return (t -= 1) * t * t * t * t + 1;
                case Ease.InOutQuint:
                    t *= 2f;
                    if (t < 1)
                    {
                        return 0.5f * t * t * t * t * t;
                    }

                    return 0.5f * ((t -= 2) * t * t * t * t + 2);
                case Ease.InExpo:
                    return t == 0 ? 0 : Mathf.Pow(2, 10 * (t - 1));
                case Ease.OutExpo:
                    if (t >= 1)
                    {
                        return 1f;
                    }

                    return -Mathf.Pow(2, -10 * t) + 1;
                case Ease.InOutExpo:
                    if (t <= 0f)
                    {
                        return 0f;
                    }

                    if (t >= 1f)
                    {
                        return 1f;
                    }

                    t *= 2f;
                    if (t < 1)
                    {
                        return 0.5f * Mathf.Pow(2, 10 * (t - 1));
                    }

                    return 0.5f * (-Mathf.Pow(2f, -10f * (t - 1f)) + 2f);

                case Ease.InCirc:
                    return -(Mathf.Sqrt(1 - t * t) - 1);
                case Ease.OutCirc:
                    return Mathf.Sqrt(1 - (t -= 1) * t);
                case Ease.InOutCirc:
                    t *= 2f;
                    if (t < 1)
                    {
                        return -0.5f * (Mathf.Sqrt(1 - t * t) - 1);
                    }

                    return 0.5f * (Mathf.Sqrt(1 - (t -= 2) * t) + 1);
                case Ease.InBack:
                    return t * t * ((backEaseConst + 1) * t - backEaseConst);
                case Ease.OutBack:
                    return (t -= 1) * t * ((backEaseConst + 1) * t + backEaseConst) + 1;
                case Ease.InOutBack:
                    t *= 2f;
                    const float c1 = backEaseConst * 1.525f;
                    if (t < 1)
                    {
                        return 0.5f * (t * t * ((c1 + 1) * t - c1));
                    }

                    return 0.5f * ((t -= 2) * t * ((c1 + 1) * t + c1) + 2);
                case Ease.InElastic:
                    return InElastic(t);
                case Ease.OutElastic:
                    return OutElastic(t);
                case Ease.InOutElastic:
                    if (t < 0.5f)
                    {
                        return InElastic(t * 2) * 0.5f;
                    }

                    return 0.5f + OutElastic((t - 0.5f) * 2f) * 0.5f;
                case Ease.InBounce:
                    return 1 - OutBounce(1 - t);
                case Ease.OutBounce:
                    return OutBounce(t);
                case Ease.InOutBounce:
                    return t < 0.5
                        ? (1 - OutBounce(1 - 2 * t)) / 2
                        : (1 + OutBounce(2 * t - 1)) / 2;
                case Ease.Custom:
                default:
                    Debug.Log($"Invalid ease type: {ease}.");
                    return t;
            }
        }


        private static float InElastic(float t) => 1 - OutElastic(1 - t);


        private static float OutElastic(float t)
        {
            const float decayFactor = 1f;
            float decay = Mathf.Pow(2, -10f * t * decayFactor);
            const float phase = defaultElasticEasePeriod / 4;
            const float twoPi = Mathf.PI * 2f;
            return t > 0.9999f ? 1 : decay * Mathf.Sin((t - phase) * twoPi / defaultElasticEasePeriod) + 1;
        }


        private static float OutBounce(float x)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;
            if (x < 1 / d1)
            {
                return n1 * x * x;
            }

            if (x < 2 / d1)
            {
                return n1 * (x -= 1.5f / d1) * x + 0.75f;
            }

            if (x < 2.5 / d1)
            {
                return n1 * (x -= 2.25f / d1) * x + 0.9375f;
            }

            return n1 * (x -= 2.625f / d1) * x + 0.984375f;
        }
    }
}