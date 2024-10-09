using System.Runtime.InteropServices;

namespace UnityNeRF.Editor
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Half
    {
        internal readonly ushort _value;

        internal Half(ushort value)
        {
            _value = value;
        }

        public static explicit operator Half(int value)
        {
            return (Half) ((float) value);
        }

        public static explicit operator Half(float value)
        {
            return new Half(UnityEngine.Mathf.FloatToHalf(value));
        }

        public static explicit operator int(Half value)
        {
            return (int) ((float) value);
        }

        public static explicit operator float(Half value)
        {
            return UnityEngine.Mathf.HalfToFloat(value._value);
        }
    }
}
