using System.Runtime.CompilerServices;

using QBX.LexicalAnalysis;
using QBX.Parser;
using QBX.Tests.Utility;

namespace QBX.Tests;

public class ParserCodeModelEndToEndTests
{
	static string GetSamplesPath([CallerFilePath] string testFilePath = "")
	{
		string testProjectPath = Path.GetDirectoryName(testFilePath)!;
		string solutionPath = Path.GetDirectoryName(testProjectPath)!;

		return Path.Combine(solutionPath, "Samples");
	}

	public static IEnumerable<string> EnumerateSamples()
	{
		string samplesPath = GetSamplesPath();

		return Directory.EnumerateFiles(samplesPath, "*.bas");
	}

	[TestCaseSource(nameof(EnumerateSamples))]
	public void TestParseAndFormat(string path)
	{
		// Arrange
		var sourceCode = File.ReadAllText(path);

		var lexer = new Lexer(sourceCode);

		var parser = new BasicParser();

		var formattedBuffer = new StringWriter();

		// Act
		var parsed = parser.Parse(lexer);

		parsed.Render(formattedBuffer);

		var formatted = formattedBuffer.ToString();

		// Assert

		// Ignore trailing whitespace and newlines
		formatted = formatted.RemoveTrailingWhitespace();
		sourceCode = sourceCode.RemoveTrailingWhitespace();

		formatted.Should().Be(sourceCode);
	}
}
