using QBX.LexicalAnalysis;
using QBX.Parser;
using QBX.Tests.Utility;

namespace QBX.Tests;

public class ParserCodeModelEndToEndTests
{
	public static IEnumerable<string> EnumerateSamples() => SamplesHelper.EnumerateSamples();

	[TestCaseSource(nameof(EnumerateSamples))]
	public void TestParseAndFormat(string path)
	{
		// Arrange
		var sourceCode = File.ReadAllText(path);

		var lexer = new Lexer(sourceCode);

		var formattedBuffer = new StringWriter() { NewLine = "\r\n" };

		// Act
		var parsed = BasicParser.Parse(lexer);

		parsed.Render(formattedBuffer);

		var formatted = formattedBuffer.ToString();

		// Assert

		// Ignore trailing whitespace and newlines
		formatted = formatted.RemoveTrailingWhitespace();
		sourceCode = sourceCode.RemoveTrailingWhitespace();

		formatted.Should().Be(sourceCode);
	}
}
