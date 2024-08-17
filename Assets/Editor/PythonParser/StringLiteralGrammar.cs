using System;
using System.Collections.Generic;
using System.Linq;
using Sprache;
using static PythonParser.EscapeSeqGrammar;

namespace PythonParser
{
    [Flags]
    internal enum Prefix
    {
        None    = 0,
        Raw     = 1,
        Bytes   = 2,
        Format  = 3,
        Unicode = 4,
    }

    internal static class StringLiteralGrammar
    {
        private static Prefix ToPrefix(string s)
        {
            s = s.ToLower();
            var prefix = Prefix.None;
            if (s.Contains('r')) prefix |= Prefix.Raw;
            if (s.Contains('b')) prefix |= Prefix.Bytes;
            if (s.Contains('f')) prefix |= Prefix.Format;
            if (s.Contains('u')) prefix |= Prefix.Unicode;
            return prefix;
        }

        public static readonly Parser<Prefix> StringPrefix =
            Parse.Regex(@"(?i)r|u|f|fr|rf|").Select(ToPrefix);
        
        private static Parser<IEnumerable<char>> ShortStringItem(char quote) =>
            Parse.CharExcept(quote + "\\").Once().XOr(StringEscapeSeq);

        private static readonly Parser<string> ShortString =
            from prefix in StringPrefix
            from quote in Parse.Char('\'').Or(Parse.Char('\"'))
            from chars in ShortStringItem(quote).Many().Select(chars => chars.SelectMany(c => c)).Text()
            from close in Parse.Char(quote)
            select chars;

        // TODO: Raw strings

        // TODO: Long strings

        public static readonly Parser<string> StringLiteral = ShortString;
    }
}