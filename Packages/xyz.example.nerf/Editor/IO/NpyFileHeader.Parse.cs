using System;
using System.Collections.Generic;
using System.Linq;
using Sprache;
using static Sprache.Parse;

namespace UnityNeRF.Editor.IO
{
    internal readonly partial struct NpyFileHeader
    {
        public static NpyFileHeader Parse(string s)
        {
            if (s is null)
                throw new ArgumentNullException();
            
            if (!TryParse(s, out NpyFileHeader header))
                throw new FormatException("Failed to parse header");
            
            return header;
        }

        public static bool TryParse(string s, out NpyFileHeader header)
        {
            if (s is null)
                goto Fail;
            
            var result = ParseHelper.Header.Token().End().TryParse(s);

            if (!result.WasSuccessful)
                goto Fail;
            
            Dictionary<string, object> dict = result.Value;
            
            if (!dict.ContainsKey("descr") || dict["descr"] is not string)
                goto Fail;
            if (!dict.ContainsKey("fortran_order") || dict["fortran_order"] is not bool)
                goto Fail;
            if (!dict.ContainsKey("shape") || dict["shape"] is not int[])
                goto Fail;
            
            var descr = (string) dict["descr"];
            var fortranOrder = (bool) dict["fortran_order"];
            var shape = (int[]) dict["shape"];
            header = new NpyFileHeader(descr, fortranOrder, shape);
            return true;

        Fail:
            header = default;
            return false;
        }

        private class ParseHelper
        {
            public static readonly Parser<Dictionary<string, object>> Header =
                from lbrace in Char('{')
                from dictItems in HeaderItem.Token().DelimitedBy(Char(','))
                from trailingComma in Char(',').Token().Optional()
                from rbrace in Char('}')
                select new Dictionary<string, object>(dictItems);
            
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
        }
    }
} // namespace UnityNeRF.Editor.IO
