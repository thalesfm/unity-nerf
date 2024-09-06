using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sprache;

internal readonly struct NpyHeader
{
    public readonly string Descr;
    public readonly bool FortranOrder;
    public readonly int[] Shape;

    public NpyHeader(string descr, bool fortranOrder, int[] shape)
    {
        Descr = descr;
        FortranOrder = fortranOrder;
        Shape = shape;
    }

    public static NpyHeader Parse(string s)
    {
        Dictionary<string, object> dict = NpyHeaderParser.Parse(s);
        var descr = (string) dict["descr"];
        var fortranOrder = (bool) dict["fortran_order"];
        var shape = (int[]) dict["shape"];
        return new NpyHeader(descr, fortranOrder, shape);
    }

    public override string ToString()
    {
        string shape = string.Join(", ", Shape.Select(x => x.ToString()));
        return $"{{'descr': {Descr}, 'fortran_order': {FortranOrder}, 'shape': ({shape})}}";
    }
}