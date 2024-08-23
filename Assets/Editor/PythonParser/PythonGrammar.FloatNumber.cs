using System.Collections.Generic;
using System.Linq;
using Sprache;

namespace PythonParser
{
    public static partial class PythonGrammar
    {
        private static readonly Parser<IEnumerable<char>> DigitPart =
            from digit0 in Parse.Digit
            from digits in Parse.Digit.Or(Parse.Char('_')).Many()
            select Enumerable.Prepend(digits.Where(c => c != '_'), digit0);

        private static readonly Parser<string> Significand =
            from wholePart in DigitPart.Text().Optional()
            from dot in Parse.String(".").Text().Optional()
            from fractPart in DigitPart.Text().Optional()
            where wholePart.IsDefined || fractPart.IsDefined
            select wholePart.GetOrDefault() + dot.GetOrDefault() + fractPart.GetOrDefault();
        
        private static readonly Parser<string> Exponent =
            from e in Parse.IgnoreCase('e')
            from sign in Parse.String("+").Or(Parse.String("-")).Text().Optional()
            from digits in DigitPart.Text()
            select e + sign.GetOrDefault() + digits;
        
        public static readonly Parser<PyFloat> FloatNumber =
            from significand in Significand
            from exponent in Exponent.Optional()
            where significand.Contains('.') || exponent.IsDefined
            select new PyFloat(double.Parse(significand + exponent.GetOrDefault()));
    }
}