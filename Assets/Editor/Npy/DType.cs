using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sprache;

// TODO: Handle strings

internal readonly struct DType
{
    // public readonly int Num;
    // public readonly string Name;

    public char Kind { get => Str[1]; }
    public char ByteOrder { get => Str[0]; }

    public char Char
    {
        get => (Kind, ItemSize) switch
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

    // See: https://numpy.org/doc/stable/reference/generated/numpy.typename.html
    // and: https://docs.python.org/3/library/array.html
    public Type Type
    {
        get => Char switch {
            '?' => typeof(bool),
            'b' => typeof(sbyte),
            'B' => typeof(byte),
            'U' => typeof(string),
            'h' => typeof(short),
            'H' => typeof(ushort),
            'i' => typeof(int),
            'I' => typeof(uint),
            'l' => typeof(int),
            'L' => typeof(uint),
            'q' => typeof(long),
            'e' => typeof(Half), // typeof(short),
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
    
    public readonly string Str;
    public readonly int ItemSize;

    public DType(Type type)
    {
        throw new NotImplementedException();
    }

    public DType(string desc)
    {
        Str = TypeStr.Parse(desc);
        ItemSize = Str.Length > 2 ? int.Parse(Str[2..]) : 1;
        if (Kind == 'U') ItemSize *= 4;
    }

    public override string ToString() => $"dtype({Str})";
}