using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityNeRF
{
    internal static class Vector3Extensions
    {
        public static Vector3 Clamp(this Vector3 value, float min, float max)
        {
            float x = Mathf.Clamp(value.x, min, max);
            float y = Mathf.Clamp(value.y, min, max);
            float z = Mathf.Clamp(value.z, min, max);

            return new Vector3(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Clamp01(this Vector3 value)
        {
            return value.Clamp(0.0f, 1.0f);
        }

        public static Vector3 Floor(this Vector3 value)
        {
            float x = Mathf.Floor(value.x);
            float y = Mathf.Floor(value.y);
            float z = Mathf.Floor(value.z);

            return new Vector3(x, y, z);
        }

        public static Vector3Int FloorToInt(this Vector3 value)
        {
            int x = Mathf.FloorToInt(value.x);
            int y = Mathf.FloorToInt(value.y);
            int z = Mathf.FloorToInt(value.z);

            return new Vector3Int(x, y, z);
        }
    }
}