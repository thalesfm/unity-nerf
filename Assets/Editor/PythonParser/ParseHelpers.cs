using Sprache;

namespace PythonParser
{
    internal static class ParseHelpers
    {
        public static bool IsBinDigit(char c) => c == '0' || c == '1';

        public static bool IsOctDigit(char c) => '0' <= c && c <= '7';

        public static bool IsHexDigit(char c) => char.IsDigit(c) || ('a' <= c && c <= 'f') || ('A' <= c && c <= 'F');
        
        public static bool IsAscii(char c) => '\x0000' <= c && c <= '\x007f';

        public static readonly Parser<char> BinDigit = Parse.Char(IsBinDigit, "binary digit");
        
        public static readonly Parser<char> OctDigit = Parse.Char(IsOctDigit, "octal digit");

        public static readonly Parser<char> HexDigit = Parse.Char(IsHexDigit, "hexadecimal digit");

        public static readonly Parser<char> AsciiChar = Parse.Char(IsAscii, "ASCII character");

        public static Parser<T> Unexpected<T>(string message) => delegate (IInput i)
        {
            return Result.Failure<T>(i, message, new string[0]);
        };
    }
}