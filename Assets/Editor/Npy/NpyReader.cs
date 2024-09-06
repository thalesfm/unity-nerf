using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Codice.CM.SEIDInfo;
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

    private static object ReadArrayData(BinaryReader reader, NpyHeader header)
    {
        DType dtype = new DType(header.Descr);
        Debug.Log($"Got dtype {dtype}");

        if (header.Shape.Count() == 0)
        {
            // UnityEngine.Debug.Log("Reading scalar");
            return dtype.Read(reader);
        }
        else
        {
            // UnityEngine.Debug.Log($"Reading array w/ shape {header.Shape}");
            Array data = Array.CreateInstance(dtype.Type, header.Shape);
            // Span<T> span = new Span<float>(data);
            foreach (int[] indices in Indices(header.Shape))
            {
                object value = dtype.Read(reader);
                string indicesStr = string.Join(", ", indices.Select(x => x.ToString()));
                // Debug.Log($"ReadArrayData: read {value} (a {value.GetType()}) for index ({indicesStr})");
                data.SetValue(value, indices);
            }
            return data;
        }

        static IEnumerable<int[]> Indices(int[] dimensions)
        {
            if (dimensions.Length == 0)
            {
                yield return new int[0];
            }
            else
            {
                for (int i = 0; i < dimensions[0]; ++i)
                {
                    foreach (int[] indices in Indices(dimensions[1..]))
                    {
                        yield return indices.Prepend(i).ToArray();
                    }
                }           
            }
        }
    }

    public object ReadArray()
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
