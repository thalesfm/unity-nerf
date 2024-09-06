using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sprache;
using static Sprache.Parse;

internal static class NpyHeaderParser
{
    private static readonly Parser<bool> PythonBool =
            String("True").Return(true)
        .Or(String("False").Return(false));

    private static readonly Parser<int> PythonInteger =
        Digit.AtLeastOnce().Text().Select(s => int.Parse(s));

    private static readonly Parser<string> PythonString =
        from quote in Char('\'').Or(Char('\"'))
        from content in CharExcept(quote + "\\").Many().Text()
        from closeQuote in Char(quote)
        select content;

    private static Parser<T[]> PythonTuple<T>(Parser<T> elemParser) =>
        from lparen in Char('(')
        from elements in elemParser.Token().DelimitedBy(Char(',')).Optional()
           .Select(opt => opt.GetOrElse(Enumerable.Empty<T>()))
        from trailComma in Char(',').Repeat(elements.Count() == 1 ? 1 : 0, 1)
        from rparen in Char(')')
        select elements.ToArray();

    private static readonly Parser<object> HeaderValue =
            PythonString
        .Or(PythonBool.Select(b => (object)b))
        .Or(PythonTuple(PythonInteger));

    private static readonly Parser<KeyValuePair<string, object>> HeaderItem =
        from key in PythonString
        from colon in Char(':').Token()
        from value in HeaderValue
        select KeyValuePair.Create(key, value);

    private static readonly Parser<Dictionary<string, object>> Header =
        from lbrace in Char('{')
        from dictItems in HeaderItem.Token().DelimitedBy(Char(','))
        from trailingComma in Char(',').Token().Optional()
        from rbrace in Char('}')
        select new Dictionary<string, object>(dictItems);

    public static Dictionary<string, object> Parse(string s) => Header.Token().End().Parse(s);
}