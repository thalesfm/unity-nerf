using System;
using System.Collections.Generic;
using System.Linq;
using Sprache;
// using static Python.Parser.StringLiteralGrammar;

namespace PythonParser
{
    public static partial class PythonGrammar
    {
        public static readonly Parser<PyBool> True =
            Parse.String("True").Return(new PyBool(true));

        public static readonly Parser<PyBool> False =
            Parse.String("False").Return(new PyBool(false));

        public static readonly Parser<PyBool> BoolLiteral = True.Or(False);

        public static readonly Parser<PyNone> None  =
            Parse.String("None").Return(new PyNone());
        
        public static readonly Parser<PyString> StringLiteral =
            StringLiteralGrammar.StringLiteral.Select(s => new PyString(s));
        
        // TODO:
        // BytesLiteral
        // Int, Float, etc.
        // Lists, tuples
        // Dictionary
    }
}