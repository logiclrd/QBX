using System.Runtime.CompilerServices;

using QBX.CodeModel;
using QBX.LexicalAnalysis;
using QBX.Parser;
using QBX.Tests.Utility;

namespace QBX.Tests;

public class CompilationUnitReadWriteEndToEndTests
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

	[TestCaseSource(nameof(EnumerateSamples))]
	public void TestParseAndFormat(string path)
	{
		// Arrange
		var sourceCode = File.ReadAllText(path);

		using (var reader = new StringReader(sourceCode))
		{
			var formattedBuffer = new StringWriter() { NewLine = "\r\n" };

			// Act
			var parsed = CompilationUnit.Read(reader, path, tabSize: 8, ignoreErrors: true);

			parsed.PrepareForWrite();
			parsed.Write(formattedBuffer);

			var formatted = formattedBuffer.ToString();

			// Assert

			// Ignore trailing whitespace and newlines
			formatted = formatted.RemoveTrailingWhitespace();
			sourceCode = sourceCode.RemoveTrailingWhitespace();

			formatted.Should().Be(sourceCode);
		}
	}
}
