using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sprache;

internal readonly struct NpyFileHeader
{
    public readonly string Descr;
    public readonly bool FortranOrder;
    public readonly int[] Shape;

    public NpyFileHeader(string descr, bool fortranOrder, int[] shape)
    {
        Descr = descr;
        FortranOrder = fortranOrder;
        Shape = shape;
    }

    public static NpyFileHeader Parse(string s)
    {
        Dictionary<string, object> dict = NpyHeaderParser.Parse(s);
        var descr = (string) dict["descr"];
        var fortranOrder = (bool) dict["fortran_order"];
        var shape = (int[]) dict["shape"];
        return new NpyFileHeader(descr, fortranOrder, shape);
    }

    public override string ToString()
    {
        string shape = string.Join(", ", Shape.Select(x => x.ToString()));
        return $"{{'descr': {Descr}, 'fortran_order': {FortranOrder}, 'shape': ({shape})}}";
    }
}