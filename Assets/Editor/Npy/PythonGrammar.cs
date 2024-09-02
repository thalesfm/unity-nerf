using System.Collections.Generic;
using System.Linq;
using Sprache;

public static class PythonGrammar
{
    public static readonly Parser<bool> PythonBool =
        Parse.String("True").Return(true)
        .Or(Parse.String("False").Return(false));

    public static readonly Parser<int> PythonInteger =
        Parse.DecimalInvariant.Select(s => int.Parse(s)).Named("integer");

    public static readonly Parser<string> PythonString =
        from quote in Parse.Char('\'').Or(Parse.Char('\"'))
        from content in Parse.CharExcept(quote + "\\").Many().Text()
        from closeQuote in Parse.Char(quote)
        select content;
    
    public static Parser<T[]> PythonTuple<T>(Parser<T> elemParser) =>
        from lparen in Parse.Char('(')
        from elements in Parse.DelimitedBy(elemParser.Token(), Parse.Char(','), 1, null)
        from trailComma in Parse.Char(',').Repeat(elements.Count() > 1 ? 0 : 1, 1)
        from rparen in Parse.Char(')')
        select elements.ToArray();

    // public static Parser<T[]> PythonList<T>(Parser<T> elemParser) =>
    //     from lbracket in Parse.Char('[')
    //     from elements in Parse.DelimitedBy(elemParser.Token(), Parse.Char(','))
    //     from trailComma in Parse.Char(',').Optional()
    //     from rbracket in Parse.Char(']')
    //     select elements.ToArray();
}