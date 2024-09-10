using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using UnityEngine;

// #if !NET7_0_OR_GREATER
// using NET_7_0_CompatExtensions;
// #endif

// TODO: Implement IDisposable
public class NpyReader // : IDisposable
{
    private static readonly byte[] MAGIC_PREFIX = { 147, 78, 85, 77, 80, 89 };
    private static readonly int MAGIC_LEN = MAGIC_PREFIX.Count() + 2;

    private BinaryReader reader;

    public NpyReader(BinaryReader reader)
    {
        this.reader = reader;
    }

    private static NpyVersion ReadMagic(BinaryReader reader)
    {
        byte[] magic = reader.ReadBytes(MAGIC_LEN);
        if (magic.Length == MAGIC_LEN && magic.AsSpan(0, 6).SequenceEqual(MAGIC_PREFIX))
        {
            byte major = magic[MAGIC_PREFIX.Count() + 0];
            byte minor = magic[MAGIC_PREFIX.Count() + 1];
            return new NpyVersion(major, minor);
        }
        else
        {
            // TODO: Improve error message
            throw new IOException("Failed parse NPY file");
        }
    }

    private static NpyHeader ReadArrayHeader(BinaryReader reader, NpyVersion version)
    {
        int headerLength = version switch
        {
            { Major: 1, Minor: 0 } => reader.ReadUInt16(), // Little-endian
            { Major: 2, Minor: 0 } => checked((int)reader.ReadUInt32()),
            _ => throw new IOException("Failed to parse NPY file: unsupported version number"),
        };

        byte[] headerBytes = reader.ReadBytes(headerLength);
        string headerStr = Encoding.ASCII.GetString(headerBytes);
        return NpyHeader.Parse(headerStr);
    }

    private static Array ReadArrayData(BinaryReader reader, NpyHeader header)
    {
        DType dtype = new DType(header.Descr);
        int length = header.Shape.Aggregate(1, (acc, x) => acc * x);
        byte[] data = reader.ReadBytes(dtype.ItemSize * length);

        if (dtype.Char == 'U')
        {
            // StreamReader readerUTF32 = new StreamReader(reader.BaseStream, Encoding.UTF32);
            // char[] buffer = new char[dtype.ItemSize / 4];
            string[] array = new string[length];

            for (int i = 0; i < length; ++i)
            {
                // readerUTF32.Read(buffer);
                // array[i] = new string(buffer);
                ReadOnlySpan<byte> bytes = data.AsSpan(i * dtype.ItemSize, dtype.ItemSize);
                array[i] = Encoding.UTF32.GetString(bytes);
            }
            return array;
        }
        else
        {
            Array array = Array.CreateInstance(dtype.Type, length);
            Buffer.BlockCopy(data, 0, array, 0, data.Length);
            return array;
        }
    }

    public NDArray<T> Read<T>()
    {
        throw new NotImplementedException();
    }

    public Array ReadArray()
    {
        NpyVersion version = ReadMagic(reader);
        NpyHeader header = ReadArrayHeader(reader, version);
        UnityEngine.Debug.Log($"NpyReader.ReadArray: reading array w/ header {header}");
        return ReadArrayData(reader, header);
    }

    public T[] ReadArray<T>()
    {
        return (T[])ReadArray();
    }
}
