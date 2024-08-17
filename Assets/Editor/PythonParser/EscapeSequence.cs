using System;
using System.Collections.Generic;
using System.Linq;
using Sprache;

namespace PythonParser
{
    internal static class EscapeSeqGrammar
    {
        private static bool IsOctDigit(char c) => '0' <= c && c <= '7';

        private static bool IsHexDigit(char c) =>
            char.IsDigit(c) || ('a' <= char.ToLower(c) && char.ToLower(c) <= 'f');

        private static readonly Parser<char> OctDigit = Parse.Char(IsOctDigit, "octal digit");

        private static readonly Parser<char> HexDigit = Parse.Char(IsHexDigit, "hexadecimal digit");

        private static readonly Parser<char> ByteOctEscapeSeq =
            from prefix in Parse.String(@"\o")
            from digits in OctDigit.Repeat(1, 3).Text()
            select Convert.ToChar(Convert.ToByte(digits, 8));
        
        private static readonly Parser<char> ByteHexEscapeSeq = 
            from prefix in Parse.String(@"\x")
            from digits in HexDigit.Repeat(2).Text()
            select Convert.ToChar(Convert.ToByte(digits, 16));
        
        private static readonly Parser<char> Int16HexEscapeSeq = 
            from prefix in Parse.String(@"\u")
            from digits in HexDigit.Repeat(4).Text()
            select Convert.ToChar(Convert.ToInt16(digits, 16));
        
        private static readonly Parser<char> Int32HexEscapeSeq = 
            from prefix in Parse.String(@"\U")
            from digits in HexDigit.Repeat(8).Text()
            select Convert.ToChar(Convert.ToInt32(digits, 16));

        private static readonly Parser<IEnumerable<char>> UnrecognizedEscapeSeq =
            Parse.Char('\\').Then(_ => Parse.AnyChar).Select(c => new char[] {'\\', c});
        
        private static readonly Parser<IEnumerable<char>> GeneralEscapeSeq = 
            Parse.String("\\\n").Return(Enumerable.Empty<char>()) // Backslash and newline ignored
            .Or(Parse.String(@"\\").Return('\\').Once())          // Backslash
            .Or(Parse.String(@"\'").Return('\'').Once())          // Single quote
            .Or(Parse.String(@"\""").Return('\"').Once())         // Double quote
            .Or(Parse.String(@"\a").Return('\a').Once())          // ASCII Bell (BEL)
            .Or(Parse.String(@"\b").Return('\b').Once())          // ASCII Backspace (BS)
            .Or(Parse.String(@"\f").Return('\f').Once())          // ASCII Formfeed (FF)
            .Or(Parse.String(@"\n").Return('\n').Once())          // ASCII Linefeed (LF)
            .Or(Parse.String(@"\r").Return('\r').Once())          // ASCII Carriage Return (CR)
            .Or(Parse.String(@"\t").Return('\t').Once())          // ASCII Horizontal Tab (TAB)
            .Or(Parse.String(@"\v").Return('\v').Once())          // ASCII Vertical Tab (VT)
            .Or(ByteOctEscapeSeq.Once())                          // Character with octal value
            .Or(ByteHexEscapeSeq.Once());                         // Character with hex value
        
        public static readonly Parser<IEnumerable<char>> StringEscapeSeq = GeneralEscapeSeq
            // Omitted: Character named "name" in the Unicode database
            .Or(Int16HexEscapeSeq.Once())                         // Character with 16-bit hex value
            .Or(Int32HexEscapeSeq.Once())                         // Character with 32-bit hex value
            .Or(UnrecognizedEscapeSeq);
        
        public static readonly Parser<IEnumerable<byte>> BytesEscapeSeq = GeneralEscapeSeq
             .Or(UnrecognizedEscapeSeq).Select(chars => chars.Select(Convert.ToByte));
    }
}