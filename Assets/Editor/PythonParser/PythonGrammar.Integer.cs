using System;
using System.Linq;
using Sprache;
using static PythonParser.ParseHelpers;

namespace PythonParser
{
    public static partial class PythonGrammar
    {
        private static readonly Parser<char> NonZeroDigit = Parse.Digit.Except(Parse.Char('0'));

        private static readonly Parser<long> DecIntegerZero =
            from digit0 in Parse.Char('0')
            from digits in Parse.Char('0').Or(Parse.Char('_')).Many()
            select 0L;
        
        private static readonly Parser<long> DecIntegerNonZero =
            from digit0 in NonZeroDigit
            from digits in Parse.Digit.Or(Parse.Char('_')).Many().Text()
            select Convert.ToInt64(digit0 + digits);
        
        private static readonly Parser<long> DecInteger = DecIntegerZero.Or(DecIntegerNonZero);
        
        private static readonly Parser<long> BinInteger =
            from prefix in Parse.IgnoreCase("0b")
            from digits in BinDigit.Or(Parse.Char('_')).Many().Text()
            select Convert.ToInt64(digits, 2);

        private static readonly Parser<long> OctInteger =
            from prefix in Parse.IgnoreCase("0o")
            from digits in OctDigit.Or(Parse.Char('_')).Many().Text()
            select Convert.ToInt64(digits, 8);

        private static readonly Parser<long> HexInteger =
            from prefix in Parse.IgnoreCase("0x")
            from digits in HexDigit.Or(Parse.Char('_')).Many().Text()
            select Convert.ToInt64(digits, 16);

        public static readonly Parser<PyInt> Integer =
            from value in BinInteger.Or(OctInteger).Or(HexInteger).Or(DecInteger)
            select new PyInt(value);
    }
}