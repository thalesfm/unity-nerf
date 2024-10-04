// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Some routines inspired by the Stanford Bit Twiddling Hacks by Sean Eron Anderson:
// http://graphics.stanford.edu/~seander/bithacks.html

namespace System.Numerics
{
    /// <summary>
    /// Utility methods for intrinsic bit-twiddling operations.
    /// The methods use hardware intrinsics when available on the underlying platform,
    /// otherwise they use optimized software fallbacks.
    /// </summary>
    public static class BitOperations
    {
        // C# no-alloc optimization that directly wraps the data section of the dll (similar to string constants)
        // https://github.com/dotnet/roslyn/pull/24621

        private static ReadOnlySpan<byte> TrailingZeroCountDeBruijn => new byte[32]
        {
            00, 01, 28, 02, 29, 14, 24, 03,
            30, 22, 20, 15, 25, 17, 04, 08,
            31, 27, 13, 23, 21, 19, 16, 07,
            26, 12, 18, 06, 11, 05, 10, 09
        };

        private static ReadOnlySpan<byte> Log2DeBruijn => new byte[32]
        {
            00, 09, 01, 10, 13, 21, 02, 29,
            11, 14, 16, 18, 22, 25, 03, 30,
            08, 12, 20, 28, 15, 17, 24, 07,
            19, 27, 23, 06, 26, 05, 04, 31
        };

        /// <summary>
        /// Count the number of leading zero bits in a mask.
        /// Similar in behavior to the x86 instruction LZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // [CLSCompliant(false)]
        public static int LeadingZeroCount(uint value)
        {
            // if (Lzcnt.IsSupported)
            // {
            //     // LZCNT contract is 0->32
            //     return (int)Lzcnt.LeadingZeroCount(value);
            // }

            // if (ArmBase.IsSupported)
            // {
            //     return ArmBase.LeadingZeroCount(value);
            // }

            // if (WasmBase.IsSupported)
            // {
            //     return WasmBase.LeadingZeroCount(value);
            // }

            // Unguarded fallback contract is 0->31, BSR contract is 0->undefined
            if (value == 0)
            {
                return 32;
            }

            // if (X86Base.IsSupported)
            // {
            //     // LZCNT returns index starting from MSB, whereas BSR gives the index from LSB.
            //     // 31 ^ BSR here is equivalent to 31 - BSR since the BSR result is always between 0 and 31.
            //     // This saves an instruction, as subtraction from constant requires either MOV/SUB or NEG/ADD.
            //     return 31 ^ (int)X86Base.BitScanReverse(value);
            // }

            return 31 ^ Log2SoftwareFallback(value);
        }

        /// <summary>
        /// Count the number of leading zero bits in a mask.
        /// Similar in behavior to the x86 instruction LZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // [CLSCompliant(false)]
        public static int LeadingZeroCount(ulong value)
        {
            // if (Lzcnt.X64.IsSupported)
            // {
            //     // LZCNT contract is 0->64
            //     return (int)Lzcnt.X64.LeadingZeroCount(value);
            // }

            // if (ArmBase.Arm64.IsSupported)
            // {
            //     return ArmBase.Arm64.LeadingZeroCount(value);
            // }

            // if (WasmBase.IsSupported)
            // {
            //     return WasmBase.LeadingZeroCount(value);
            // }

            // if (X86Base.X64.IsSupported)
            // {
            //     // BSR contract is 0->undefined
            //     return value == 0 ? 64 : 63 ^ (int)X86Base.X64.BitScanReverse(value);
            // }

            uint hi = (uint)(value >> 32);

            if (hi == 0)
            {
                return 32 + LeadingZeroCount((uint)value);
            }

            return LeadingZeroCount(hi);
        }

        /// <summary>
        /// Count the number of leading zero bits in a mask.
        /// Similar in behavior to the x86 instruction LZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // [CLSCompliant(false)]
        public static int LeadingZeroCount(nuint value)
        {
#if TARGET_64BIT
            return LeadingZeroCount((ulong)value);
#else
            return LeadingZeroCount((uint)value);
#endif
        }

        /// <summary>
        /// Count the number of trailing zero bits in an integer value.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TrailingZeroCount(int value)
            => TrailingZeroCount((uint)value);
        
        /// <summary>
        /// Count the number of trailing zero bits in an integer value.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        // [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TrailingZeroCount(uint value)
        {
            // if (Bmi1.IsSupported)
            // {
            //     // TZCNT contract is 0->32
            //     return (int)Bmi1.TrailingZeroCount(value);
            // }

            // if (ArmBase.IsSupported)
            // {
            //     return ArmBase.LeadingZeroCount(ArmBase.ReverseElementBits(value));
            // }

            // if (WasmBase.IsSupported)
            // {
            //     return WasmBase.TrailingZeroCount(value);
            // }

            // Unguarded fallback contract is 0->0, BSF contract is 0->undefined
            if (value == 0)
            {
                return 32;
            }

            // if (X86Base.IsSupported)
            // {
            //     return (int)X86Base.BitScanForward(value);
            // }

            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            return Unsafe.AddByteOffset(
                // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_0111_1100_1011_0101_0011_0001u
                ref MemoryMarshal.GetReference(TrailingZeroCountDeBruijn),
                // uint|long -> IntPtr cast on 32-bit platforms does expensive overflow checks not needed here
                (IntPtr)(int)(((value & (uint)-(int)value) * 0x077CB531u) >> 27)); // Multi-cast mitigates redundant conv.u8
        }

        /// <summary>
        /// Count the number of trailing zero bits in a mask.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TrailingZeroCount(long value)
            => TrailingZeroCount((ulong)value);

        /// <summary>
        /// Count the number of trailing zero bits in a mask.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // [CLSCompliant(false)]
        public static int TrailingZeroCount(ulong value)
        {
            // if (Bmi1.X64.IsSupported)
            // {
            //     // TZCNT contract is 0->64
            //     return (int)Bmi1.X64.TrailingZeroCount(value);
            // }

            // if (ArmBase.Arm64.IsSupported)
            // {
            //     return ArmBase.Arm64.LeadingZeroCount(ArmBase.Arm64.ReverseElementBits(value));
            // }

            // if (WasmBase.IsSupported)
            // {
            //     return WasmBase.TrailingZeroCount(value);
            // }

            // if (X86Base.X64.IsSupported)
            // {
            //     // BSF contract is 0->undefined
            //     return value == 0 ? 64 : (int)X86Base.X64.BitScanForward(value);
            // }

            uint lo = (uint)value;

            if (lo == 0)
            {
                return 32 + TrailingZeroCount((uint)(value >> 32));
            }

            return TrailingZeroCount(lo);
        }

        /// <summary>
        /// Count the number of trailing zero bits in a mask.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TrailingZeroCount(nint value)
        {
#if TARGET_64BIT
            return TrailingZeroCount((ulong)(nuint)value);
#else
            return TrailingZeroCount((uint)(nuint)value);
#endif
        }

        /// <summary>
        /// Count the number of trailing zero bits in a mask.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // [CLSCompliant(false)]
        public static int TrailingZeroCount(nuint value)
        {
#if TARGET_64BIT
            return TrailingZeroCount((ulong)value);
#else
            return TrailingZeroCount((uint)value);
#endif
        }

        /// <summary>
        /// Returns the integer (floor) log of the specified value, base 2.
        /// Note that by convention, input value 0 returns 0 since Log(0) is undefined.
        /// Does not directly use any hardware intrinsics, nor does it incur branching.
        /// </summary>
        /// <param name="value">The value.</param>
        private static int Log2SoftwareFallback(uint value)
        {
            // No AggressiveInlining due to large method size
            // Has conventional contract 0->0 (Log(0) is undefined)

            // Fill trailing zeros with ones, eg 00010010 becomes 00011111
            value |= value >> 01;
            value |= value >> 02;
            value |= value >> 04;
            value |= value >> 08;
            value |= value >> 16;

            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            return Unsafe.AddByteOffset(
                // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_1100_0100_1010_1100_1101_1101u
                ref MemoryMarshal.GetReference(Log2DeBruijn),
                // uint|long -> IntPtr cast on 32-bit platforms does expensive overflow checks not needed here
                (IntPtr)(int)((value * 0x07C4ACDDu) >> 27));
        }
    }
}