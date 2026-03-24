using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Infrastructure.Utilities
{

    [Serializable]
    public struct SerializableVector3
    {
        private const float Min = 0.001f;

        [JsonConverter(typeof(Float2Json))] public float x;
        [JsonConverter(typeof(Float2Json))] public float y;
        [JsonConverter(typeof(Float2Json))] public float z;

        public SerializableVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public SerializableVector3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public SerializableVector3(Quaternion r)
        {
            Vector3 v = r.eulerAngles;
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public static SerializableVector3 operator *(SerializableVector3 left, float right)
        {
            return new(left.x * right, left.y * right, left.z * right);
        }

        //Newtonsoft hooks
        public bool ShouldSerializex() => Math.Abs(x) >= Min;
        public bool ShouldSerializey() => Math.Abs(y) >= Min;
        public bool ShouldSerializez() => Math.Abs(z) >= Min;

        //convert methods
        public Vector3 ToVector3() => new(x, y, z);
        public Quaternion ToQuaternion() => Quaternion.Euler(ToVector3());
        public bool HasValue() => Math.Abs(x) >= Min || Math.Abs(y) >= Min || Math.Abs(z) >= Min;
    }
}