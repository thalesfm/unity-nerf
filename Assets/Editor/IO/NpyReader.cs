using System;
using System.IO;
using System.Linq;
using System.Text;
using NumSharp;
using Sprache;

namespace UnityNeRF.Editor.IO
{

public sealed class NpyReader : IDisposable
{
    private static readonly byte[] MagicPrefix = { 147, 78, 85, 77, 80, 89 };
    private static readonly int MagicLength = MagicPrefix.Count() + 2;

    private readonly BinaryReader reader;

    public NpyReader(Stream stream)
    {
        reader = new BinaryReader(stream);
    }

    public NpyReader(Stream stream, bool leaveOpen)
    {
        reader = new BinaryReader(stream, Encoding.Default, leaveOpen);
    }

    public void Dispose()
    {
        reader.Dispose();
    }

    public NDArray Read()
    {
        (int, int) version = ReadMagic(reader);
        NpyFileHeader header = ReadArrayHeader(reader, version);
        Array data = ReadArrayData(reader, header);
        Type dtype = data.GetType().GetElementType();
        NDArray array = np.array(data, dtype, copy: false);

        if (header.FortranOrder)
            throw new NotSupportedException("Fortran order not supported");

        return array.reshape(header.Shape);
    }

    public Array ReadArray()
    {
        (int, int) version = ReadMagic(reader);
        NpyFileHeader header = ReadArrayHeader(reader, version);
        return ReadArrayData(reader, header);
    }

    public T[] ReadArray<T>()
    {
        return (T[])ReadArray();
    }

    private static (int major, int minor) ReadMagic(BinaryReader reader)
    {
        byte[] magic = reader.ReadBytes(MagicLength);

        if (magic.Length != MagicLength || !magic.AsSpan(0, 6).SequenceEqual(MagicPrefix))
            throw new InvalidDataException("Failed parse magic string");
        
        byte major = magic[MagicPrefix.Count() + 0];
        byte minor = magic[MagicPrefix.Count() + 1];
        return new (major, minor);
    }

    private static NpyFileHeader ReadArrayHeader(BinaryReader reader, (int major, int minor) version)
    {
        int headerLength = version switch
        {
            (1, 0) => reader.ReadUInt16(),
            (2, 0) => checked((int)reader.ReadUInt32()),
            _ => throw new NotSupportedException($"Version {version} not supported"),
        };

        byte[] headerBytes = reader.ReadBytes(headerLength);
        string headerStr = Encoding.ASCII.GetString(headerBytes);
        return NpyFileHeader.Parse(headerStr);
    }

    private static Array ReadArrayData(BinaryReader reader, NpyFileHeader header)
    {
        Type dtype = ParseTypestr(header.Descr, out int size, out char byteorder);
        int length = header.Shape.Aggregate(1, (acc, x) => acc * x);

        if ((byteorder == '<' && !BitConverter.IsLittleEndian) ||
            (byteorder == '>' &&  BitConverter.IsLittleEndian))
            throw new NotSupportedException("Byte order doesn't match system endianness");
        
        if (dtype.IsPrimitive)
            return ReadArrayOfPrimitive(reader, dtype, size, length);
        if (dtype == typeof(Half))
            return ReadArrayOfHalf(reader, length);
        if (dtype == typeof(string))
            return ReadArrayOfString(reader, size, length);
        
        throw new NotSupportedException($"Type {dtype} not supported");
    }

    private static Half[] ReadArrayOfHalf(BinaryReader reader, int length)
    {
        ushort[] buffer = (ushort[])ReadArrayOfPrimitive(reader, typeof(ushort), sizeof(ushort), length);
        return buffer.Select(value => new Half(value)).ToArray();
    }

    private static Array ReadArrayOfPrimitive(BinaryReader reader, Type type, int size, int length)
    {
        byte[] buffer = reader.ReadBytes(size * length);
        Array array = Array.CreateInstance(type, length);
        Buffer.BlockCopy(buffer, 0, array, 0, size * length);
        return array;
    }

    private static string[] ReadArrayOfString(BinaryReader reader, int size, int length)
    {
        byte[] data = reader.ReadBytes(size * length);
        string[] array = new string[length];

        for (int i = 0; i < length; ++i)
        {
            ReadOnlySpan<byte> bytes = data.AsSpan(i * size, size);
            array[i] = Encoding.UTF32.GetString(bytes);
        }

        return array;
    }

    private static Type ParseTypestr(string typestr, out int size, out char endian)
    {   
        if (!TryParseTypestr(typestr, out Type type, out size, out endian))
            throw new FormatException($"Failed to parse type string \"{typestr}\"");
        
        return type;
    }

    private static bool TryParseTypestr(string typestr, out Type type, out int size, out char byteorder)
    {
        if (typestr is null)
            goto Fail;
        
        if (typestr.Length == 0)
            goto Fail;
        
        switch (typestr[0]) {
        case '>':
        case '<':
        case '=':
            byteorder = typestr[0];
            typestr = typestr[1..];
            break;
        case '|':
            byteorder = '=';
            typestr = typestr[1..];
            break;
        default:
            byteorder = '=';
            break;
        }

        if (typestr.Length == 0)
            goto Fail;

        if (!int.TryParse(typestr[1..], out size))
            goto Fail;

        char kind = typestr[0];
        switch (kind, size) {
        case ('b', 1):
            type = typeof(bool);
            return true;
        case ('i', 1):
            type = typeof(sbyte);
            return true;
        case ('i', 2):
            type = typeof(short);
            return true;
        case ('i', 4):
            type = typeof(int);
            return true;
        case ('i', 8):
            type = typeof(long);
            return true;
        case ('u', 1):
            type = typeof(byte);
            return true;
        case ('u', 2):
            type = typeof(ushort);
            return true;
        case ('u', 4):
            type = typeof(uint);
            return true;
        case ('u', 8):
            type = typeof(ulong);
            return true;
        case ('f', 2):
            type = typeof(Half);
            return true;
        case ('f', 4):
            type = typeof(float);
            return true;
        case ('f', 8):
            type = typeof(double);
            return true;
        case ('U', _):
            type = typeof(string);
            size = 4 * size;
            return true;
        };

    Fail:
        type = null;
        size = 0;
        byteorder = '\0';
        return false;
    }
}

} // namespace UnityNeRF.Editor.IO