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

		sourceCode = CollapseLineContinuations(sourceCode);

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

	static string CollapseLineContinuations(string code)
	{
		using (var reader = new StringReader(code))
		using (var writer = new StringWriter() { NewLine = "\r\n" })
		{
			while (true)
			{
				string? line = reader.ReadLine();

				if (line == null)
					break;

				if (line.EndsWith(" _"))
					writer.Write(line.Remove(line.Length - 2));
				else
					writer.WriteLine(line);
			}

			return writer.ToString();
		}
	}
}
