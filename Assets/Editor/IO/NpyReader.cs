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

        public NpyReader(Stream stream) : this(stream, false)
        {
        }

        public NpyReader(Stream stream, bool leaveOpen)
        {
            reader = new BinaryReader(stream, Encoding.Default, leaveOpen);
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        public Array ReadArray(out int[] shape)
        {
            (int, int) version = ReadMagic(reader);
            NpyFileHeader header = ReadArrayHeader(reader, version);
            Type type = ParseArrayDescr(header.Descr, out int itemsize, out char byteorder);

            if (header.FortranOrder)
                throw new NotSupportedException("Fortran ordered arrays not supported");

            if ((byteorder == '<' && !BitConverter.IsLittleEndian) ||
                (byteorder == '>' &&  BitConverter.IsLittleEndian))
                throw new NotSupportedException("Byte order doesn't match system endianness");
            
            shape = header.Shape;
            return ReadArrayData(reader, type, itemsize, header.Shape);
        }

        public Array ReadArray() => ReadArray(out int[] shape);

        public T[] ReadArray<T>() => ReadArray<T>(out int[] shape);

        public T[] ReadArray<T>(out int[] shape) => (T[])ReadArray(out shape);

        private T ReadScalar<T>()
        {
            Array array = ReadArray();

            if (array.Length != 1)
                throw new Exception("Array not a scalar");

            if (array.GetType().GetElementType() != typeof(T))
                throw new Exception("Type mismatch");

            return (T)array.GetValue(0);
        }

        public Int16 ReadInt16() => ReadScalar<Int16>();
        public Int32 ReadInt32() => ReadScalar<Int32>();
        public Int64 ReadInt64() => ReadScalar<Int64>();

        public UInt16 ReadUInt16() => ReadScalar<UInt16>();
        public UInt32 ReadUInt32() => ReadScalar<UInt32>();
        public UInt64 ReadUInt64() => ReadScalar<UInt64>();

        public Half ReadHalf() => ReadScalar<Half>();
        public float ReadSingle() => ReadScalar<float>();
        public double ReadDouble() => ReadScalar<double>();

        public string ReadString() => ReadScalar<string>();

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

        private static Array ReadArrayData(BinaryReader reader, Type type, int itemsize, int[] shape)
        {
            int length = shape.Aggregate(1, (count, x) => count * x);
            return ReadArrayData(reader, type, itemsize, length);
        }

        private static Array ReadArrayData(BinaryReader reader, Type type, int itemsize, int length)
        {   
            byte[] buffer = reader.ReadBytes(itemsize * length);
            
            if (type.IsPrimitive)
            {
                Array array = Array.CreateInstance(type, length);
                Buffer.BlockCopy(buffer, 0, array, 0, buffer.Length);
                return array;
            }
            if (type == typeof(Half))
            {
                ushort[] array = new ushort[length];
                Buffer.BlockCopy(buffer, 0, array, 0, buffer.Length);
                return array.Select(value => new Half(value)).ToArray();
            }
            if (type == typeof(string))
            {
                string[] array = new string[length];
                for (int i = 0; i < length; ++i)
                {
                    ReadOnlySpan<byte> bytes = buffer.AsSpan(i * itemsize, itemsize);
                    array.SetValue(Encoding.UTF32.GetString(bytes), i);
                }
                return array;
            }
            
            throw new NotSupportedException($"Type {type} not supported");
        }

        private static Type ParseArrayDescr(string typestr, out int itemsize, out char byteorder)
        {   
            if (!TryParseArrayDescr(typestr, out Type type, out itemsize, out byteorder))
                throw new FormatException($"Failed to parse type string \"{typestr}\"");
            
            return type;
        }

        private static bool TryParseArrayDescr(string typestr, out Type type, out int itemsize, out char byteorder)
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

            if (!int.TryParse(typestr[1..], out itemsize))
                goto Fail;

            char kind = typestr[0];
            switch (kind, itemsize) {
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
                itemsize = 4 * itemsize;
                return true;
            };

        Fail:
            type = null;
            itemsize = 0;
            byteorder = '\0';
            return false;
        }
    }
} // namespace UnityNeRF.Editor.IO