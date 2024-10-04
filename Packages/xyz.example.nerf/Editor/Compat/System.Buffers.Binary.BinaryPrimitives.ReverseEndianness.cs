// Adapted from https://github.com/dotnet/runtime/blob/5535e31a712343a63f5d7d796cd874e563e5ac14/src/libraries/System.Private.CoreLib/src/System/Buffers/Binary/BinaryPrimitives.ReverseEndianness.cs#L191C13-L191C81
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if UNITY_64 || UNITY_EDITOR_64
    #define TARGET_64BIT
#endif

using System;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// namespace System.Buffers.Binary
namespace Compat.System.Buffers.Binary
{
    public static partial class BinaryPrimitives
    {
        public static void ReverseEndianness(ReadOnlySpan<ushort> source, Span<ushort> destination) =>
            ReverseEndianness(MemoryMarshal.Cast<ushort, short>(source), MemoryMarshal.Cast<ushort, short>(destination), new Int16EndiannessReverser());

        public static void ReverseEndianness(ReadOnlySpan<short> source, Span<short> destination) =>
            ReverseEndianness(source, destination, new Int16EndiannessReverser());

        public static void ReverseEndianness(ReadOnlySpan<uint> source, Span<uint> destination) =>
            ReverseEndianness(MemoryMarshal.Cast<uint, int>(source), MemoryMarshal.Cast<uint, int>(destination), new Int32EndiannessReverser());

        public static void ReverseEndianness(ReadOnlySpan<int> source, Span<int> destination) =>
            ReverseEndianness(source, destination, new Int32EndiannessReverser());

        public static void ReverseEndianness(ReadOnlySpan<ulong> source, Span<ulong> destination) =>
            ReverseEndianness(MemoryMarshal.Cast<ulong, long>(source), MemoryMarshal.Cast<ulong, long>(destination), new Int64EndiannessReverser());

        public static void ReverseEndianness(ReadOnlySpan<long> source, Span<long> destination) =>
            ReverseEndianness(source, destination, new Int64EndiannessReverser());

        public static void ReverseEndianness(ReadOnlySpan<nuint> source, Span<nuint> destination) =>
#if TARGET_64BIT
            ReverseEndianness(MemoryMarshal.Cast<nuint, long>(source), MemoryMarshal.Cast<nuint, long>(destination), new Int64EndiannessReverser());
#else
            ReverseEndianness(MemoryMarshal.Cast<nuint, int>(source), MemoryMarshal.Cast<nuint, int>(destination), new Int32EndiannessReverser());
#endif

        public static void ReverseEndianness(ReadOnlySpan<nint> source, Span<nint> destination) =>
#if TARGET_64BIT
            ReverseEndianness(MemoryMarshal.Cast<nint, long>(source), MemoryMarshal.Cast<nint, long>(destination), new Int64EndiannessReverser());
#else
            ReverseEndianness(MemoryMarshal.Cast<nint, int>(source), MemoryMarshal.Cast<nint, int>(destination), new Int32EndiannessReverser());
#endif

        private readonly struct Int16EndiannessReverser : IEndiannessReverser<short>
        {
            public short Reverse(short value) =>
                global::System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);
        }

        private readonly struct Int32EndiannessReverser : IEndiannessReverser<int>
        {
            public int Reverse(int value) =>
                global::System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);
        }

        private readonly struct Int64EndiannessReverser : IEndiannessReverser<long>
        {
            public long Reverse(long value) =>
                global::System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);
        }

        private static void ReverseEndianness<T, TReverser>(ReadOnlySpan<T> source, Span<T> destination, TReverser reverser)
            where T : struct
            where TReverser : IEndiannessReverser<T>
        {
            if (destination.Length < source.Length)
            {
                ThrowDestinationTooSmall();
            }

            ref T sourceRef = ref MemoryMarshal.GetReference(source);
            ref T destRef = ref MemoryMarshal.GetReference(destination);

            if (Unsafe.AreSame(ref sourceRef, ref destRef) ||
                !source.Overlaps(destination, out int elementOffset) ||
                elementOffset < 0)
            {
                // Either there's no overlap between the source and the destination, or there's overlap but the
                // destination starts at or before the source.  That means we can safely iterate from beginning
                // to end of the source and not have to worry about writing into the destination and clobbering
                // source data we haven't yet read.

                for (int i = 0; i < source.Length; i++)
                {
                    Unsafe.Add(ref destRef, i) = reverser.Reverse(Unsafe.Add(ref sourceRef, i));
                }
            }
            else
            {
                // There's overlap between the source and the destination, and the source starts before the destination.
                // That means if we were to iterate from beginning to end, reading from the source and writing to the
                // destination, we'd overwrite source elements not yet read.  To avoid that, we iterate from end to beginning.

                for (int i = source.Length; i > 0; i--)
                {
                    Unsafe.Add(ref destRef, i) = reverser.Reverse(Unsafe.Add(ref sourceRef, i));
                }
            }
        }

        private interface IEndiannessReverser<T> where T : struct
        {
            abstract T Reverse(T value);
        }

        [DoesNotReturn]
        private static void ThrowDestinationTooSmall() =>
            throw new ArgumentException("Not enough space available in the buffer.", "destination");
    }
}