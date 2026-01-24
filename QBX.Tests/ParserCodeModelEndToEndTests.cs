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

		return Directory.EnumerateFiles(samplesPath, "*.BAS");
	}

	static string NormalizeNewLinesForNonWindowsPlatforms(string code)
	{
		if (Environment.NewLine == "\r\n")
			return code;

		var reader = new StringReader(code);
		var writer = new StringWriter() { NewLine = "\r\n" };

		while (reader.ReadLine() is string line)
			writer.WriteLine(line);

		return writer.ToString();
	}

	[TestCaseSource(nameof(EnumerateSamples))]
	public void TestParseAndFormat(string path)
	{
		// Arrange
		var sourceCode = File.ReadAllText(path);

		sourceCode = NormalizeNewLinesForNonWindowsPlatforms(sourceCode);

		var lexer = new Lexer(sourceCode);

		var parser = new BasicParser();

		var formattedBuffer = new StringWriter() { NewLine = "\r\n" };

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
