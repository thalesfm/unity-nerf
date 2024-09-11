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

    private static NpyVersion ReadMagic(BinaryReader reader)
    {
        byte[] magic = reader.ReadBytes(MagicLength);
        if (magic.Length == MagicLength && magic.AsSpan(0, 6).SequenceEqual(MagicPrefix))
        {
            byte major = magic[MagicPrefix.Count() + 0];
            byte minor = magic[MagicPrefix.Count() + 1];
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
        string headerStr = Encoding.ASCII.GetString(headerBytes); // FIXME: Header could be UTF-8
        return NpyHeader.Parse(headerStr);
    }

    private static Array ReadArrayData(BinaryReader reader, NpyHeader header)
    {
        DType dtype = new DType(header.Descr);
        int length = header.Shape.Aggregate(1, (acc, x) => acc * x);
        byte[] data = reader.ReadBytes(dtype.ItemSize * length);

        if (dtype.Char == 'U')
        {
            string[] array = new string[length];

            for (int i = 0; i < length; ++i)
            {
                ReadOnlySpan<byte> bytes = data.AsSpan(i * dtype.ItemSize, dtype.ItemSize);
                array[i] = Encoding.UTF32.GetString(bytes);
            }
            return array;
        }
        else if (dtype.Char == 'e')
        {
            ushort[] array = new ushort[length];
            Buffer.BlockCopy(data, 0, array, 0, data.Length);
            // return array;
            return array.Select(BitConverterCompat.UInt16BitsToHalf).ToArray();
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
