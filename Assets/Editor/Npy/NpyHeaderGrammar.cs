using System.Collections.Generic;
using System.Linq;
using Sprache;

public static class NpyHeaderGrammar
{
    public static readonly Parser<bool> PythonBool =
        Parse.String("True").Return(true)
        .Or(Parse.String("False").Return(false));

    public static readonly Parser<int> PythonInteger =
        Parse.Digit.AtLeastOnce().Text().Select(s => int.Parse(s));

    public static readonly Parser<string> PythonString =
        from quote in Parse.Char('\'').Or(Parse.Char('\"'))
        from content in Parse.CharExcept(quote + "\\").Many().Text()
        from closeQuote in Parse.Char(quote)
        select content;
    
    public static Parser<T[]> PythonTuple<T>(Parser<T> elemParser) =>
        from lparen in Parse.Char('(')
        from elements in elemParser.Token().DelimitedBy(Parse.Char(',')).Optional()
           .Select(opt => opt.GetOrElse(Enumerable.Empty<T>()))
        from trailComma in Parse.Char(',').Repeat(elements.Count() == 1 ? 1 : 0, 1)
        from rparen in Parse.Char(')')
        select elements.ToArray();

    // public static Parser<T[]> PythonList<T>(Parser<T> elemParser) =>
    //     from lbracket in Parse.Char('[')
    //     from elements in Parse.DelimitedBy(elemParser.Token(), Parse.Char(','))
    //     from trailComma in Parse.Char(',').Optional()
    //     from rbracket in Parse.Char(']')
    //     select elements.ToArray();

    public static readonly Parser<object> HeaderValue = PythonString
         .Or(PythonBool.Select(b => b as object))
         .Or(PythonTuple(PythonInteger));

    public static readonly Parser<KeyValuePair<string, object>> HeaderItem =
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