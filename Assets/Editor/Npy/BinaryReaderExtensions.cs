using System;
using System.IO;
using System.Linq;

// namespace Eek {

public static class BinaryReaderExtensions
{
    private delegate object ReadFunc(BinaryReader reader);

    public static bool[] ReadBooleanArray(this BinaryReader reader, int count)
    {
        throw new NotImplementedException();
    }

    public static double[] ReadDoubleArray(this BinaryReader reader, int count)
    {
        static object read(BinaryReader reader) => reader.ReadDouble();
        return ReadArray<double>(reader, count, read);
    }

    public static Half[] ReadHalfArray(this BinaryReader reader, int count)
    {
        ushort[] array = reader.ReadUInt16Array(count);
        return array.Select(BitConverterCompat.UInt16BitsToHalf).ToArray();
    }

    public static short[] ReadInt16Array(this BinaryReader reader, int count)
    {
        static object read(BinaryReader reader) => reader.ReadInt16();
        return ReadArray<short>(reader, count, read);
    }

    public static int[] ReadInt32Array(this BinaryReader reader, int count)
    {
        static object read(BinaryReader reader) => reader.ReadInt32();
        return ReadArray<int>(reader, count, read);
    }

    public static long[] ReadInt64Array(this BinaryReader reader, int count)
    {
        static object read(BinaryReader reader) => reader.ReadInt64();
        return ReadArray<long>(reader, count, read);
    }

    public static sbyte[] ReadSByteArray(this BinaryReader reader, int count)
    {
        static object read(BinaryReader reader) => reader.ReadSByte();
        return ReadArray<sbyte>(reader, count, read);
    }

    public static float[] ReadSingleArray(this BinaryReader reader, int count)
    {
        static object read(BinaryReader reader) => reader.ReadSingle();
        return ReadArray<float>(reader, count, read);
    }

    public static ushort[] ReadUInt16Array(this BinaryReader reader, int count)
    {
        static object read(BinaryReader reader) => reader.ReadUInt16();
        return ReadArray<ushort>(reader, count, read);
    }

    public static uint[] ReadUInt32Array(this BinaryReader reader, int count)
    {
        static object read(BinaryReader reader) => reader.ReadUInt32();
        return ReadArray<uint>(reader, count, read);
    }

    public static ulong[] ReadUInt64Array(this BinaryReader reader, int count)
    {
        static object read(BinaryReader reader) => reader.ReadUInt64();
        return ReadArray<ulong>(reader, count, read);
    }

    private static T[] ReadArray<T>(BinaryReader reader, int count, ReadFunc read)
    {
        T[] array = new T[count];
        for (int i = 0; i < count; i++)
        {
            T value = (T) read(reader);
            array.SetValue(value, i);
        }
        return array;
    }
}

// }