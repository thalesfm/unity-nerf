using System;
using System.IO;
using System.Linq;
using System.Text;

// #if !NET5_0_OR_GREATER
// using Half = HalfCompat;
// #endif

// #if !NET7_0_OR_GREATER
// using NET_7_0_CompatExtensions;
// #endif

#if !NET8_0_OR_GREATER
using BinaryPrimitives = Compat.System.Buffers.Binary.BinaryPrimitives;
#endif

// TODO: Implement IDisposable
public class NpyReader // : IDisposable
{
    private static readonly byte[] MagicPrefix = { 147, 78, 85, 77, 80, 89 };
    private static readonly int MagicLength = MagicPrefix.Count() + 2;

    private BinaryReader reader;

    public NpyReader(BinaryReader reader)
    {
        this.reader = reader;
    }

    public NDArray<T> Read<T>()
    {
        throw new NotImplementedException();
    }

    public Array ReadArray()
    {
        Version version = ReadMagic(reader);
        NpyHeader header = ReadArrayHeader(reader, version);
        UnityEngine.Debug.Log($"NpyReader.ReadArray: reading array w/ header {header}");
        return ReadArrayData(reader, header);
    }

    public T[] ReadArray<T>()
    {
        return (T[])ReadArray();
    }

    private static Version ReadMagic(BinaryReader reader)
    {
        byte[] magic = reader.ReadBytes(MagicLength);
        if (magic.Length == MagicLength && magic.AsSpan(0, 6).SequenceEqual(MagicPrefix))
        {
            byte major = magic[MagicPrefix.Count() + 0];
            byte minor = magic[MagicPrefix.Count() + 1];
            return new Version(major, minor);
        }
        else
        {
            throw new IOException("Failed parse NPY file");
        }
    }

    private static NpyHeader ReadArrayHeader(BinaryReader reader, Version version)
    {
        int headerLength = version switch
        {
            { Major: 1, Minor: 0 } => reader.ReadUInt16(),
            { Major: 2, Minor: 0 } => checked((int)reader.ReadUInt32()),
            _ => throw new IOException("Failed to parse NPY file: unsupported version number"),
        };

        byte[] headerBytes = reader.ReadBytes(headerLength);
        string headerStr = Encoding.ASCII.GetString(headerBytes); // FIXME: Header could be UTF-8
        return NpyHeader.Parse(headerStr);
    }

    private static string DescrToTypestr(string descr)
    {
        DType dtype = new DType(descr);
        return dtype.Str;
    }

    private static Array ReadArrayData(BinaryReader reader, NpyHeader header)
    {
        string typestr = DescrToTypestr(header.Descr);
        int length = header.Shape.Aggregate(1, (acc, x) => acc * x);
        reader = typestr[0] switch
        {
            '<' => reader,
            '>' => new BinaryReaderBigEndian(reader.BaseStream),
            '|' => reader,
            _   => throw new Exception(),
        };
        
        switch (typestr[1..])
        {
            case "b1":
                return reader.ReadBooleanArray(length);
            case "i1":
                return reader.ReadSByteArray(length);
            case "i2":
                return reader.ReadInt16Array(length);
            case "i4":
                return reader.ReadInt32Array(length);
            case "i8":
                return reader.ReadInt64Array(length);
            case "S1":
            case "u1":
                return reader.ReadBytes(length);
            case "u2":
                return reader.ReadUInt16Array(length);
            case "u4":
                return reader.ReadUInt32Array(length);
            case "u8":
                return reader.ReadUInt64Array(length);
            case "f2":
                return reader.ReadHalfArray(length);
            case "f4":
                return reader.ReadSingleArray(length);
            case "f8":
                return reader.ReadDoubleArray(length);
            case "f16":
                throw new NotSupportedException();
            // Complex
            case var s when s.StartsWith("c"):
                throw new NotSupportedException();
            // Timedelta
            case var s when s.StartsWith("m"):
                throw new NotSupportedException();
            // Datetime
            case var s when s.StartsWith("M"):
                throw new NotSupportedException();
            // Object
            case var s when s.StartsWith("O"):
                throw new NotSupportedException();
            // Zero-terminated byte string
            case var s when s.StartsWith("S"):
                throw new NotSupportedException();
            // Unicode string
            case var s when s.StartsWith("U"):
                return ReadStringArray(reader, header);
            // Void
            case var s when s.StartsWith("V"):
                throw new NotSupportedException();
            default:
                throw new ArgumentException();
        };
    }

    // FIXME: Reverse endianness if necessary
    private static string[] ReadStringArray(BinaryReader reader, NpyHeader header)
    {
        DType dtype = new DType(header.Descr);
        int length = header.Shape.Aggregate(1, (acc, x) => acc * x);
        byte[] data = reader.ReadBytes(dtype.ItemSize * length);

        string[] array = new string[length];
        for (int i = 0; i < length; ++i)
        {
            ReadOnlySpan<byte> bytes = data.AsSpan(i * dtype.ItemSize, dtype.ItemSize);
            array[i] = Encoding.UTF32.GetString(bytes);
        }
        return array;
    }

    public readonly struct Version
    {
        public readonly int Major;
        public readonly int Minor;

        public Version(int major, int minor)
        {
            Major = major;
            Minor = minor;
        }
    }
}
