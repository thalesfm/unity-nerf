using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.MethodImplOptions;
using System.Text;

internal class BinaryReaderBigEndian : BinaryReader, IDisposable
{
    public BinaryReaderBigEndian(Stream input) : base(input) { }

    public BinaryReaderBigEndian(Stream input, Encoding encoding) : base(input, encoding) { }

    public BinaryReaderBigEndian(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen) { }

    public new int Read7BitEncodedInt() => throw new NotImplementedException();

    public override decimal ReadDecimal() => throw new NotImplementedException();

    [MethodImpl(AggressiveInlining)]
    public override double ReadDouble()
    {
        long value = BinaryPrimitives.ReverseEndianness(base.ReadInt64());
        return BitConverter.Int64BitsToDouble(value);
    }

    // public override Half ReadHalf() => throw new NotImplementedException();

    [MethodImpl(AggressiveInlining)]
    public override short ReadInt16() => BinaryPrimitives.ReverseEndianness(base.ReadInt16());

    [MethodImpl(AggressiveInlining)]
    public override int ReadInt32() => BinaryPrimitives.ReverseEndianness(base.ReadInt32());

    [MethodImpl(AggressiveInlining)]
    public override long ReadInt64() => BinaryPrimitives.ReverseEndianness(base.ReadInt64());

    [MethodImpl(AggressiveInlining)]
    public override float ReadSingle()
    {
        int value = BinaryPrimitives.ReverseEndianness(base.ReadInt32());
        return BitConverter.Int32BitsToSingle(value);
    }

    // TODO
    [MethodImpl(AggressiveInlining)]
    public override string ReadString() => throw new NotImplementedException();

    [MethodImpl(AggressiveInlining)]
    public override ushort ReadUInt16() => BinaryPrimitives.ReverseEndianness(base.ReadUInt16());

    [MethodImpl(AggressiveInlining)]
    public override uint ReadUInt32() => BinaryPrimitives.ReverseEndianness(base.ReadUInt32());

    [MethodImpl(AggressiveInlining)]
    public override ulong ReadUInt64() => BinaryPrimitives.ReverseEndianness(base.ReadUInt64());
}