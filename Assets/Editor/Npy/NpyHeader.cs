using System.Collections.Generic;
using Sprache;

public readonly struct NpyHeader
{
    public readonly DType DType;
    public readonly bool FortranOrder;
    public readonly int[] Shape;

    public NpyHeader(DType dtype, bool fortranOrder, int[] shape)
    {
        DType = dtype;
        FortranOrder = fortranOrder;
        Shape = shape;
    }

    public static NpyHeader Parse(string s)
    {
        // IResult<Dictionary> result = NpyHeaderGrammar.HeaderDictionary.TryParse(s);
        Dictionary<string, object> dict = NpyHeaderGrammar.HeaderDictionary.Parse(s);
        string descr = dict["descr"] as string;
        DType dtype = DType.Parse(descr);
        bool fortranOrder = (bool) dict["fortran_order"]; // WARN: unsafe!
        int[] shape = dict["shape"] as int[];
        return new NpyHeader(dtype, fortranOrder, shape);
    }
}