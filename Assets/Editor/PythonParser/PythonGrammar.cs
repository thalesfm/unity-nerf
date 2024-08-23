using System;
using System.Collections.Generic;
using System.Linq;
using Sprache;
using static PythonParser.ParseHelpers;

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
        
        // TODO: Other identifiers
        public static readonly Parser<PyObject> Identifier = True.Or(False).Or<PyObject>(None);

        // TODO
        // public static readonly Parser<PyObject> ImagNumber = null;
        
        // WARN: Initialization order not specified!
        public static readonly Parser<PyObject> Literal =
            StringLiteral.Or(BytesLiteral).Or(Integer).Or(FloatNumber);
            // .Or(ImagNumber);

        public static readonly Parser<PyObject> Enclosure = Unexpected<PyObject>("not implemented");

        public static readonly Parser<PyObject> Atom = Identifier.Or(Literal).Or(Enclosure);

        public static readonly Parser<PyObject> Expression = Atom;

        // TODO: list
        // TODO: tuple
        // TODO: dict
    }
}