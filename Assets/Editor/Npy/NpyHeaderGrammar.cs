using System.Collections.Generic;
using System.Linq;
using Sprache;
using static PythonGrammar;

public static class NpyHeaderGrammar
{
    private static readonly Parser<string> ArrayDTypeDescr = PythonString;

    private static readonly Parser<object> HeaderValue =
        ArrayDTypeDescr
        .Or(PythonBool.Select(b => b as object))
        .Or(PythonTuple(PythonInteger));

    private static readonly Parser<KeyValuePair<string, object>> HeaderItem =
        from key in PythonString
        from colon in Parse.Char(':').Token()
        from value in HeaderValue
        select new KeyValuePair<string, object>(key, value);
    
    public static Parser<Dictionary<string, object>> HeaderDictionary =
        from lbrace in Parse.Char('{')
        from dictItems in HeaderItem.Token().DelimitedBy(Parse.Char(','))
        from trailingComma in Parse.Char(',').Token().Optional()
        from rbrace in Parse.Char('}')
        select new Dictionary<string, object>(dictItems);
}