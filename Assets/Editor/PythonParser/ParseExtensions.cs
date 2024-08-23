using Sprache;
using UnityEngine;

namespace PythonParser
{
    internal static class ParserExtensions
    {
        public static Parser<U> And<T, U>(this Parser<T> first, Parser<U> second)
        {
            return first.Then(_ => second);
        }

        public static Parser<T> ThenFail<T>(this Parser<T> parser)
        {
            return parser.Where(_ => false);
        }
    }
}