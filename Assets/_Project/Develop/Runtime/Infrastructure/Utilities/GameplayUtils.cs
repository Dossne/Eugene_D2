using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Infrastructure.Utilities
{
    public static class GameplayUtils
    {
        /// <summary>
        /// Places points from the center: odd forward, even backward. Idx of the center is always 0.
        /// </summary>
        public static Vector3[] GetPositionsFromCenterToSides(float height, float length, int posCount)
        {
            float startPosZ = 0f;
            float posX = 0f;

            if (posCount <= 1)
                return new Vector3[] { new(posX, height, startPosZ) };

            Vector3[] localPositions = new Vector3 [posCount];
            float divisor = posCount % 2 == 0 ? posCount : posCount - 1;
            float stepDistance = length * 0.5f / divisor;

            for (int i = 0; i < posCount; i++)
            {
                float posZ = i % 2 == 0 ? startPosZ + i * stepDistance : startPosZ - (i + 1) * stepDistance;
                localPositions[i] = new Vector3(posX, height, posZ);
            }

            return localPositions;
        }


        /// <summary>
        /// Positions with offset along the X axis from left to right
        /// For posCount is even, then there will be a shift to the left by half the length for symmetry with no point in center
        /// Calculate in world space
        /// </summary>
        public static Vector3[] GetPositionsStraightLine(Transform transform, in float offsetX, in int posCount)
        {
            if (posCount <= 1)
                return new[] { transform.position };

            Vector3[] result = new Vector3 [posCount];

            float centerOffset = posCount % 2 == 0 ? posCount * 0.5f - 0.5f : posCount / 2; //do not fix loss fraction "posCount / 2"

            for (int i = 0; i < posCount; i++)
            {
                float xOffset = (i - centerOffset) * offsetX;
                result[i] = transform.TransformPoint(transform.localPosition + new Vector3(xOffset, 0, 0));
            }

            return result;
        }


        /// <summary>
        /// Positions from given transform to sides by angle
        /// </summary>
        public static Vector3[] GetTargetPositionsAngled(in Vector3 launchPos, in Vector3 forward, in float range, in float angle, in int posCount)
        {
            Vector3[] result = new Vector3 [posCount];

            for (int i = 0; i < posCount; i++)
            {
                float resultAngle = angle * (i / 2 + 1) * (i % 2 == 0 ? 1 : -1); //do not fix loss fraction "i / 2"
                result[i] = GetTargetPosByAngle(in launchPos, in forward, in range, in resultAngle);
            }

            return result;
        }


        public static Vector3 GetTargetPosByAngle(in Vector3 launchPos, in Vector3 forward, in float range, in float angle)
        {
            return launchPos + Quaternion.Euler(0, angle, 0) * forward * range;
        }


        /// <summary>
        /// Get random point between radiusMin and radiusMax.
        /// Calculating details see here https://www.desmos.com/calculator/k1yz5a2dbh
        /// </summary>
        public static Vector3 GetRandomPointInCircle2d(Vector2 zeroPosition, float radiusMin, float radiusMax)
        {
            float randomPointInRange = Random.Range(radiusMin, radiusMax);
            float randomPi = Random.Range(-Mathf.PI, Mathf.PI);
            float pointX = zeroPosition.x + Mathf.Cos(randomPi) * randomPointInRange;
            float pointY = zeroPosition.y + Mathf.Sin(randomPi) * randomPointInRange;
            return new Vector3(pointX, pointY, 0);
        }


        public static Vector3 GetRandomPointInCircle3d(Vector3 zeroPosition, float radiusMin, float radiusMax)
        {
            float randomPointInRange = Random.Range(radiusMin, radiusMax);
            float randomPi = Random.Range(-Mathf.PI, Mathf.PI);
            float pointX = zeroPosition.x + Mathf.Cos(randomPi) * randomPointInRange;
            float pointZ = zeroPosition.z + Mathf.Sin(randomPi) * randomPointInRange;
            return new Vector3(pointX, zeroPosition.y, pointZ);
        }


        /// <summary>
        /// Get random index by weight, more value of which gives better chance.
        /// </summary>
        /// <param name="weights">Array of weights</param>
        /// <param name="totalWeight">Total sum of all weights in array</param>
        /// <param name="resultIndex">Result index in array</param>
        /// <returns>Returns true if result found, otherwise false</returns>
        public static bool TryGetRandomByWeight(List<int> weights, in int totalWeight, out int resultIndex)
        {
            resultIndex = -1;

            if (weights == null || weights.Count == 0)
            {
                return false;
            }

            int random = Random.Range(1, totalWeight + 1); /*incl, excl*/
            int sum = 0;

            for (int i = 0; i < weights.Count; ++i)
            {
                if (weights[i] <= 0)
                {
                    continue;
                }

                sum += weights[i];

                if (sum >= random)
                {
                    resultIndex = i;
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Randomly swaps elements in source and return in result
        /// </summary>
        public static bool TryGetRandomUniqueElements<T>(List<T> source, int count, List<T> result)
        {
            if (count < 1 || count > source.Count)
                return false;

            int n = source.Count;

            for (int i = 0; i < count; i++)
            {
                int randomIndex = Random.Range(i, n);
                (source[i], source[randomIndex]) = (source[randomIndex], source[i]);
            }

            result.AddRange(source.GetRange(0, count));
            return true;
        }

        
        public static void ShuffleElements<T>(List<T> source)
        {
            int n = source.Count;

            for (int i = 0; i < source.Count; i++)
            {
                int randomIndex = Random.Range(i, n);
                (source[i], source[randomIndex]) = (source[randomIndex], source[i]);
            }
        }
        

        public static bool IsInAngle(in Vector3 sourceDir, in Vector3 sourcePos, in Vector3 targetPos, in float angle)
        {
            Vector3 directionToTarget = (targetPos - sourcePos).normalized;

            if (Vector3.Angle(sourceDir, directionToTarget) >= angle * 0.5f)
            {
                return false;
            }

            return true;
        }


        public static bool DistanceLess(float distance, Vector3 from, Vector3 to)
        {
            return (to - from).sqrMagnitude > distance * distance;
        }


        public static bool DistanceGreater(float distance, Vector3 from, Vector3 to)
        {
            return (to - from).sqrMagnitude < distance * distance;
        }


        public static bool DistanceEqual(float distance, Vector3 from, Vector3 to)
        {
            return Mathf.Approximately((to - from).sqrMagnitude, distance * distance);
        }


        public static bool DistanceLessOrEqual(float distance, Vector3 from, Vector3 to)
        {
            return (to - from).sqrMagnitude >= distance * distance;
        }


        public static bool DistanceGreaterOrEqual(float distance, Vector3 from, Vector3 to)
        {
            return (to - from).sqrMagnitude <= distance * distance;
        }
        
        public static Vector3 GetClosestPoint(List<Collider> colliders, Vector3 point)
        {
            if (colliders == null || colliders.Count == 0)
                return point;

            float minDistance = float.MaxValue;
            Vector3 closestPoint = point;

            foreach (var collider in colliders)
            {
                if (collider == null)
                    continue;

                Vector3 pointOnCollider = GetClosestPoint(collider, point);
                
                float distance = Vector3.Distance(point, pointOnCollider);

                if (distance <= 0.01f)
                    return point;

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = pointOnCollider;
                }
            }

            return closestPoint;
        }


        public static Vector3 GetClosestPoint(Collider collider, Vector3 point)
        {
            if (collider is MeshCollider meshCollider && !meshCollider.convex)
            {
                return collider.bounds.ClosestPoint(point);
            }

            return collider.ClosestPoint(point);
        }
    }
}