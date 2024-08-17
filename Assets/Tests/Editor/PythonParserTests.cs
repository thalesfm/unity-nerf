using NUnit.Framework;
using Sprache;
using PythonParser;

public class PythonParserTests
{
    [Test]
    public void StringLiteralParserTest()
    {
        string input;
        string output;

        input = @"""Hello, World!""";
        output = PythonGrammar.StringLiteral.Parse(input).Value;
        Assert.AreEqual("Hello, World!", output);

        input = @"'single quotes'";
        output = PythonGrammar.StringLiteral.Parse(input).Value;
        Assert.AreEqual("single quotes", output);

        input = @"'\\\'\""\n'";
        output = PythonGrammar.StringLiteral.Parse(input).Value;
        Assert.AreEqual("\\\'\"\n", output);
    }
}
