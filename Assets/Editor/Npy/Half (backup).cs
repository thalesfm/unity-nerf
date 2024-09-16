// #if !NET5_0_OR_GREATER

// using System;
// using System.Runtime.InteropServices;

// [StructLayout(LayoutKind.Sequential)]
// public readonly struct Half
// {
//     public const uint SignMask     = 0x8000;
//     public const uint ExponentMask = 0x7C00;
//     public const uint FractionMask = 0x03FF;

//     public const int SignShift     = 15;
//     public const int ExponentShift = 10;

//     public const int ExponentBias = 15;
//     public const uint MinBiasedExponent = 0x00;
//     public const uint MaxBiasedExponent = 0x1F;
//     public const uint MinFraction = 0; // TODO
//     public const uint MaxFraction = 0; // TODO

//     internal const ushort QNaNBits = 0x7E00;
//     internal const ushort PositiveInfinityBits = 0x7C00;
//     internal const ushort NegativeInfinityBits = 0xFC00;
//     internal const ushort ZeroBits = 0x0000;

//     public static readonly Half NaN = new(QNaNBits);
//     public static readonly Half PositiveInfinity = new(PositiveInfinityBits);
//     public static readonly Half NegativeInfinity = new(NegativeInfinityBits);
//     public static readonly Half Zero = new(ZeroBits);

//     private readonly ushort value;

//     internal Half(ushort value)
//     {
//         this.value = value;
//     }

//     private uint BiasedExponent { get => ExtractBiasedExponent(value); }
//     // private uint Exponent { get => ExtractExponent(value); }
//     private uint Fraction { get => ExtractFraction(value); }

//     public static bool IsNegative(Half value)
//     {
//         return (value.value & SignMask) != 0;
//     }

//     public static bool IsNaN(Half value)
//     {
//         return (value.BiasedExponent == MaxBiasedExponent) && (value.Fraction != 0);
//     }

//     public static bool IsPositiveInfinity(Half value)
//     {
//         return value.value == PositiveInfinity.value;
//     }

//     public static bool IsNegativeInfinity(Half value)
//     {
//         return value.value == NegativeInfinity.value;
//     }

//     private static uint ExtractBiasedExponent(ushort value)
//     {
//         return (value & ExponentMask) >> ExponentShift;
//     }

//     // private static uint ExtractExponent(ushort value)
//     // {
//     //     return ExtractBiasedExponent(value) - ExponentBias;
//     // }

//     private static uint ExtractFraction(ushort value)
//     {
//         return value & FractionMask;
//     }

//     // private static uint ExtractSignificand(ushort value)
//     // {
//     //     if (ExtractBiasedExponent(value) == 0)
//     //     {
//     //         return value;
//     //     }
//     //     else
//     //     {
//     //         return (1u << ExponentShift) | value;
//     //     }
//     // }

//     public override string ToString()
//     {
//         return ((float)this).ToString();
//     }

//     public static explicit operator float(Half value)
//     {
//         // TODO: Check if value is denormalized, NaN, or infinite
//         bool sign = IsNegative(value);
//         uint exponent = value.BiasedExponent;
//         uint fraction = value.Fraction;
//         return FloatHelper.Create(sign, exponent + 112, fraction << 13);
//     }

//     public static explicit operator Half(float value)
//     {
//         // int bits = BitConverter.SingleToInt32Bits(value);
//         throw new NotImplementedException();
//     }
// }

// internal static class FloatHelper
// {
//     private const uint SignMask     = 0x80000000;
//     private const uint ExponentMask = 0x7F800000;
//     private const uint FractionMask = 0x007FFFFF;

//     private const int SignShift = 31;
//     private const int ExponentShift = 23;

//     private const int  MinExponent = 0;
//     private const int  MaxExponent = 0;
//     private const uint MinFraction = 0;
//     private const uint MaxFraction = 0;
//     private const int  ExponentBias = 127;

//     // internal const ushort QNaNBits = 0x7E00;
//     // private static readonly uint PositiveInfinityBits = (uint)BitConverter.SingleToInt32Bits(float.PositiveInfinity);
//     // internal const ushort NegativeInfinityBits = 0xFC00;
//     // internal const ushort ZeroBits = 0x0000;

//     public static float Create(bool sign, uint bexpt, uint fraction)
//     {
//         uint value = ((sign ? 1u : 0) << SignShift) | (bexpt << ExponentShift) | fraction;
//         return BitConverter.Int32BitsToSingle((int)value);
//     }

//     public static float CreateChecked(bool sign, int exponent, uint fraction)
//     {
//         throw new NotImplementedException();
//     }
// }

// #endif