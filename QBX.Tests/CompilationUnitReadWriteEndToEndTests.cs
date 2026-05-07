using QBX.CodeModel;
using QBX.Tests.Utility;

namespace QBX.Tests;

public class CompilationUnitReadWriteEndToEndTests
{
	public static IEnumerable<string> EnumerateSamples() => SamplesHelper.EnumerateSamples();

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
