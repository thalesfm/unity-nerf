using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sprache;

// TODO: Handle strings

internal readonly struct DType
{
    // public readonly Type Type;
    public readonly char Kind;
    public readonly char Char;
    // public readonly int Num;
    // public readonly string Str;
    // public readonly string Name;
    public readonly int ItemSize;
    public readonly char ByteOrder;

    // See: https://numpy.org/doc/stable/reference/generated/numpy.typename.html
    // and: https://docs.python.org/3/library/array.html
    public Type Type
    {
        get => Char switch {
            '?' => typeof(bool),
            'b' => typeof(sbyte),
            'B' => typeof(byte),
            // 'u' => typeof(string),
            'U' => typeof(string),
            'h' => typeof(short),
            'H' => typeof(ushort),
            'i' => typeof(int),
            'I' => typeof(uint),
            'l' => typeof(int),
            'L' => typeof(uint),
            'q' => typeof(long),
            'e' => typeof(short), // HACK: Should be Half
            'Q' => typeof(ulong),
            'f' => typeof(float),
            'd' => typeof(double),
            'O' => typeof(object),
            _   => throw new NotImplementedException(),
        };
    }

    private static readonly Parser<string> TypeStr =
        from byteorder in Parse.Chars("><=").Optional().Select(o => o.GetOrElse('='))
        from typekind in Parse.Chars("?bBiufcmMOSaUV") // 'biufcmMOSUV'
        from itemsize in Parse.Digit.Many().Text() // Select(s => int.Parse(s.ToString()))
        select $"{byteorder}{typekind}{itemsize}";

    public DType(Type type)
    {
        throw new NotImplementedException();
    }

    public DType(string _typestr)
    {
        string typestr = TypeStr.Parse(_typestr);
        ByteOrder = typestr[0];
        Kind = typestr[1];

        ItemSize = typestr.Length > 2 ? int.Parse(typestr[2..]) : 1;
        if (Kind == 'U') ItemSize *= 4;

        Char = (Kind, ItemSize) switch
        {
            ('?', 1) => throw new NotSupportedException(),
            ('b', 1) => 'b',
            ('B', 1) => 'B',
            ('i', 1) => 'b',
            ('i', 2) => 'h',
            ('i', 4) => 'l',
            ('i', 8) => 'q',
            ('u', 1) => 'B',
            ('u', 2) => 'H',
            ('u', 4) => 'L',
            ('u', 8) => 'Q',
            ('f', 2) => 'e',
            ('f', 4) => 'f',
            ('f', 8) => 'd',
            ('c', _) => throw new NotSupportedException(),
            ('m', _) => throw new NotSupportedException(),
            ('M', _) => throw new NotSupportedException(),
            ('O', _) => throw new NotSupportedException(),
            ('S', _) => throw new NotSupportedException(),
            ('a', _) => throw new NotSupportedException(),
            ('U', _) => 'U',
            ('V', _) => throw new NotSupportedException(),
            _        => throw new ArgumentException(),
        };
    }

    public override string ToString() => $"dtype({ByteOrder}{Kind}{ItemSize})";

    public object Read(BinaryReader reader)
    {
        // byte[] bytes = reader.ReadBytes(ItemSize);
        // BitConverter.IsLittleEndian;
        bool IsLittleEndian = true; // TODO
        return (IsLittleEndian, Char) switch
        {
            (_, 'B') => (object) reader.ReadByte(),
            // 'D'
            // 'G
            // 'F'
            (true, 'I') => (object) reader.ReadUInt32(),
            // (false, 'I') => BinaryPrimitives.ReadInt32BigEndian(bytes),
            (true, 'H') => (object) reader.ReadUInt16(),
            (true, 'L') => (object) reader.ReadUInt32(),
            (true, 'Q') => (object) reader.ReadUInt64(),
            // 'S'
            // 'U'
            // 'u' => throw new NotSupportedException(),
            // 'V'
            (true, 'd') => reader.ReadDouble(),
            (true, 'b') => reader.ReadSByte(),
            (true, 'f') => reader.ReadSingle(),
            (true, 'e') => throw new NotImplementedException(),
            (true, 'i') => reader.ReadInt32(),
            (true, 'h') => reader.ReadInt16(),
            (true, 'l') => reader.ReadInt32(),
            (true, 'q') => reader.ReadInt64(),
            (true, '?') => throw new NotSupportedException(),
            (true, 'O') => throw new NotSupportedException(),
            (true, 'g') => throw new NotSupportedException(),
            _           => throw new ArgumentException(),
        };
    }
}