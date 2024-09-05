using System.Collections.Generic;
using NUnit.Framework;
using Sprache;
using UnityEditor;
using System.Data.SqlTypes;
using System.Numerics;
using System.Linq;

public class NpyHeaderGrammarTests
{
    [TestCase("\"double quoted string\"", "double quoted string")]
    [TestCase("'single quoted string'", "single quoted string")]
    public static void TestPythonStringParser(string input, string expected)
    {
        string output = NpyHeaderGrammar.PythonString.Parse(input);
        Assert.That(output, Is.EqualTo(expected));
    }

    [TestCase(@"'\\\'\""\n'")]
    public static void TestStringLiteralParserWasNotSuccessful(string input)
    {
        IResult<string> result = NpyHeaderGrammar.PythonString.TryParse(input);
        Assert.That(result.WasSuccessful, Is.False);
    }

    [TestCase("3", 3)]
    [TestCase("7", 7)]
    [TestCase("2147483647", 2147483647)]
    public static void TestPythonIntegerParser(string input, long expected)
    {
        long output = NpyHeaderGrammar.PythonInteger.End().Parse(input);
        Assert.That(output, Is.EqualTo(expected));
    }

    [TestCase("(1, 2, 3)", new int[] {1, 2, 3})]
    [TestCase("(1 ,2,3,)", new int[] {1, 2, 3})]
    [TestCase("(1,)", new int[] {1})]
    [TestCase("()", new int[0])]
    public static void TestPythonTupleParser(string input, IEnumerable<int> expected)
    {
        IEnumerable<int> output = NpyHeaderGrammar.PythonTuple<int>(NpyHeaderGrammar.PythonInteger).End().Parse(input);
        Assert.That(output.SequenceEqual(expected));
    }
}
