using System.Collections.Generic;
using NUnit.Framework;
using Sprache;
using PythonParser;
using UnityEditor;
using System.Data.SqlTypes;
using System.Numerics;

public class PythonParserTests
{
    [TestCase("\"double quoted string\"", "double quoted string")]
    [TestCase("'single quoted string'", "single quoted string")]
    [TestCase(@"'\\\'\""\n'", "\\\'\"\n")]
    public static void TestStringLiteralParser(string input, string expected)
    {
        string output = PythonGrammar.StringLiteral.Parse(input).Value;
        Assert.That(output, Is.EqualTo(expected));
    }

    [TestCase("'''this is a long string'''")]
    [TestCase("r'this is a raw string'")]
    [TestCase("f'this is an f-string'")]
    public static void TestStringLiteralParserWasNotSuccessful(string input)
    {
        IResult<PyObject> result = PythonGrammar.StringLiteral.TryParse(input);
        Assert.That(result.WasSuccessful, Is.False);
    }

    [TestCase("b'abcdef'")]
    public void TestBytesLiteralParserWasNotSuccessful(string input)
    {
        IResult<object> result = PythonGrammar.BytesLiteral.TryParse(input);
        Assert.That(result.WasSuccessful, Is.False);
    }

    [TestCase("3", 3)]
    [TestCase("7", 7)]
    [TestCase("2147483647", 2147483647)]
    // [TestCase("79228162514264337593543950336", ...)]
    [TestCase("0o177", 127)]
    [TestCase("0o377", 255)]
    [TestCase("0b10110111", 0b10110111)]
    [TestCase("0xdeadbeef", 0xdeadbeef)]
    // [TestCase("100_000_000_000", ...)]
    public static void TestIntegerLiteralParser(string input, long expected)
    {
        long output = PythonGrammar.Integer.End().Parse(input).Value;
        Assert.That(output, Is.EqualTo(expected));
    }

    [TestCase("3.14", 3.14)]
    [TestCase("10.", 10.0)]
    [TestCase(".001", .001)]
    [TestCase("1e100", 1e100)]
    [TestCase("3.14e-10", 3.14e-10)]
    [TestCase("0e0", 0.0)]
    [TestCase("3.14_15_93", 3.141593)]
    public static void TestFloatParser(string input, double expected)
    {
        double output = PythonGrammar.FloatNumber.End().Parse(input).Value;
        Assert.That(output, Is.EqualTo(expected));
    }

    [TestCase("10")]
    public void TestFloatParserWasNotSuccessful(string input)
    {
        IResult<PyObject> result = PythonGrammar.FloatNumber.TryParse(input);
        Assert.That(result.WasSuccessful, Is.False);
    }
}
