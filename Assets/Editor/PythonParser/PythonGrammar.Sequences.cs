using System;
using System.Collections.Generic;
using System.Linq;
using Sprache;
using static PythonParser.ParseHelpers;

namespace PythonParser
{
    public static partial class PythonGrammar
    {
        public static readonly Parser<IEnumerable<PyObject>> ExpressionList =
            from exprs in Expression.Token().DelimitedBy(Parse.Char(','))
            from comma in Parse.Char(',').Optional()
            select exprs;
        
        public static readonly Parser<PyObject> Tuple =
            from lparen in Parse.Char('(')
            from elements in Parse.DelimitedBy(Expression.Token(), Parse.Char(','), 1, null)
            from trailComma in Parse.Char(',').Repeat(elements.Count() == 0 ? 1 : 0, 1)
            from rparen in Parse.Char(')')
            select new PythonTuple<PyObject>(elements.ToArray());
        
        public static readonly Parser<PythonList<PyObject>> ListDisplay =
            from lbracket in Parse.Char('[')
            from elements in ExpressionList
            from rbracket in Parse.Char(']')
            select new PythonList<PyObject>(elements.ToArray());
    }
}