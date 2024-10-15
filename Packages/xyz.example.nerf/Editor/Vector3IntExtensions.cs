using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityNeRF.Editor
{
    internal static class Vector3IntExtensions
    {
        public static Vector3Int Clamp(this Vector3Int value, int min, int max)
        {
            int x = Mathf.Clamp(value.x, min, max);
            int y = Mathf.Clamp(value.y, min, max);
            int z = Mathf.Clamp(value.z, min, max);

            return new Vector3Int(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int Clamp01(this Vector3Int value)
        {
            return value.Clamp(0, 1);
        }
    }
}
