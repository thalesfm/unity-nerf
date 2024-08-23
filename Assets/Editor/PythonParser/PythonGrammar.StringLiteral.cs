using System;
using System.Collections.Generic;
using System.Linq;
using Sprache;
using static PythonParser.ParseHelpers;

namespace PythonParser
{
    public static partial class PythonGrammar
    {
        [Flags]
        private enum StringFlags
        {
            None    = 0,
            Raw     = 1,
            Bytes   = 2,
            Format  = 3,
            Unicode = 4,
        }
        
        private static StringFlags ToStringFlags(string s)
        {
            s = s.ToLower();
            var prefix = StringFlags.None;
            if (s.Contains('r')) prefix |= StringFlags.Raw;
            if (s.Contains('b')) prefix |= StringFlags.Bytes;
            if (s.Contains('f')) prefix |= StringFlags.Format;
            if (s.Contains('u')) prefix |= StringFlags.Unicode;
            return prefix;
        }

        private static readonly Parser<StringFlags> StringPrefix =
            Parse.Regex(@"(?i)r|f|u|fr|rf|").Select(ToStringFlags);
        
        private static readonly Parser<StringFlags> BytesPrefix =
            Parse.Regex(@"(?i)b|br|rb").Select(ToStringFlags);
        
        private static Parser<IEnumerable<char>> GeneralEscapeSequence(Parser<char> charParser) =>
            Parse.Char('\\').And(charParser).Then(c => c switch
            {
                '\n' => Parse.Return(Enumerable.Empty<char>()), // Backslash and newline ignored
                '\\' => Parse.Return('\\').Once(),              // Backslash   
                '\'' => Parse.Return('\'').Once(),              // Single quote
                '\"' => Parse.Return('\"').Once(),              // Double quote
                'a'  => Parse.Return('\a').Once(),              // ASCII Bell (BEL)
                'b'  => Parse.Return('\b').Once(),              // ASCII Backspace (BS)
                'f'  => Parse.Return('\f').Once(),              // ASCII Formfeed (FF)
                'n'  => Parse.Return('\n').Once(),              // ASCII Linefeed (LF)
                'r'  => Parse.Return('\r').Once(),              // ASCII Carriage Return (CR)
                't'  => Parse.Return('\t').Once(),              // ASCII Horizontal Tab (TAB)
                'v'  => Parse.Return('\v').Once(),              // ASCII Vertical Tab (VT)
                var o when IsOctDigit(o) =>                     // Character with octal value ooo
                    OctDigit.Repeat(0, 2).Select(oo => {
                        var ooo = Enumerable.Prepend(oo, o).ToString();
                        return Convert.ToChar(Convert.ToByte(ooo));
                    }).Once(),
                _ => Parse.Return(c).Where(_ => false).Once(),
            });
        
        private static readonly Parser<IEnumerable<char>> StringOnlyEscapeSequence =
            Parse.Char('\\').And(Parse.AnyChar).Then(c => c switch
            {
                // Character with hex value hh
                'x' => HexDigit.Repeat(2).Text().Select(hh =>
                    Convert.ToChar(Convert.ToByte(hh, 16))).Once(),
                // Character with 16-bit hex value xxxx
                'u' => HexDigit.Repeat(4).Text().Select(xxxx =>
                    Convert.ToChar(Convert.ToInt16(xxxx, 16))).Once(),
                // Character with 32-bit hex value xxxxxxxx
                'U' => HexDigit.Repeat(8).Text().Select(xxxxxxxx =>
                    Convert.ToChar(Convert.ToInt32(xxxxxxxx, 16))).Once(),
                _ => Parse.Return(c).Where(_ => false).Once(),
            });
        
        private static readonly Parser<IEnumerable<char>> StringEscapeSequence =
            GeneralEscapeSequence(Parse.AnyChar).Or(StringOnlyEscapeSequence);
        
        private static readonly Parser<IEnumerable<byte>> BytesEscapeSequence =
            GeneralEscapeSequence(AsciiChar).Select(chars => chars.Select(Convert.ToByte));
        
        private static Parser<IEnumerable<char>> ShortStringItem(char quote) =>
            Parse.CharExcept(quote + "\\").Once().XOr(StringEscapeSequence);
        
        private static Parser<string> TripleQuotes =
            Parse.String("\'\'\'").Or(Parse.String("\"\"\"")).Text();

        private static Parser<string> ShortString(StringFlags flags) =>
            from quote in Parse.Char('\'').Or(Parse.Char('\"'))
            from content in ShortStringItem(quote).Many().Select(chars => chars.SelectMany(c => c)).Text()
            from closeQuote in Parse.Char(quote)
            select content;

        private static Parser<string> LongString(StringFlags flags) =>
            from quotes in TripleQuotes
            from unexpected in Unexpected<string>("long string literals not supported")
            select unexpected;

        public static readonly Parser<PyString> StringLiteral =
            from prefix in StringPrefix
            where !(prefix.HasFlag(StringFlags.Format) || prefix.HasFlag(StringFlags.Raw))
            from tripleQuotes in TripleQuotes.Preview()
            from content in tripleQuotes.IsDefined ? LongString(prefix) : ShortString(prefix)
            select new PyString(content);

        public static readonly Parser<PyObject> BytesLiteral =
            from prefix in BytesPrefix
            from unexpected in Unexpected<PyObject>("bytes literals not supported")
            select unexpected;
    }
}