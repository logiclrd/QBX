using QBX.CodeModel;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace QBX.Tests.CodeModel;

// This tests a specific behaviour to do with DEFtype statements preceding SUBs and FUNCTIONs.
// Their representation differs on disk and in-memory, and QuickBASIC actually rewrites the
// statements when saving and loading.
//
// On disk, there is only one DEFtype configuration, and it is updated continuously across the
// file. DEFtype statements apply starting on the line they're encountered going forward,
// regardless of whether that line is in the main module, preceding a SUB or FUNCTION or
// in a SUB or FUNCTION.
//
// In memory, every SUB and FUNCTION starts at a baseline of DEFSNG A-Z, and specifies the
// DEFtype statements needed to achieve the desired configuration at the start of the SUB
// or FUNCTION directly before the opening line (and before any comments that precede the
// SUB or FUNCTION as well).

internal class DefTypesTests
{
	static Stream OpenOnDiskReference()
		=> typeof(DefTypesTests).Assembly.GetManifestResourceStream("QBX.Tests.CodeModel.DEFTYPES.BAS-ondisk") ?? throw new Exception("Internal error: Failed to load test data");
	static Stream OpenInMemoryReference()
		=> typeof(DefTypesTests).Assembly.GetManifestResourceStream("QBX.Tests.CodeModel.DEFTYPES.BAS-inmemory") ?? throw new Exception("Internal error: Failed to load test data");

	[Test]
	public void ShouldTranslateCorrectlyOnLoad()
	{
		// Arrange
		var parser = new BasicParser();

		using (var inMemoryStream = OpenInMemoryReference())
		using (var onDiskStream = OpenOnDiskReference())
		{
			var reader = new StreamReader(inMemoryStream);

			var expectedLoadedUnit = parser.Parse(new Lexer(reader));

			reader = new StreamReader(onDiskStream);

			string unitName = "Untitled";

			// Act
			var result = CompilationUnit.Read(reader, unitName, parser);

			// Assert
			var expectedWriter = new StringWriter();
			var actualWriter = new StringWriter();

			expectedLoadedUnit.Render(expectedWriter);
			result.Render(actualWriter);

			string expectedText = expectedWriter.ToString();
			string actualText = actualWriter.ToString();

			actualText.Should().Be(expectedText);
		}
	}

	[Test]
	public void ShouldTranslateCorrectlyOnSave()
	{
		// Arrange
		var parser = new BasicParser();

		using (var inMemoryStream = OpenInMemoryReference())
		using (var onDiskStream = OpenOnDiskReference())
		{
			var reader = new StreamReader(onDiskStream);

			string expectedOnDiskText = reader.ReadToEnd();

			reader = new StreamReader(inMemoryStream);

			var unit = parser.Parse(new Lexer(reader));

			var writer = new StringWriter();

			// Act
			unit.Write(writer);

			// Assert
			string actualText = writer.ToString();

			string actualTextTrimmed = actualText.TrimEnd('\r', '\n');
			string expectedTextTrimmed = expectedOnDiskText.TrimEnd('\r', '\n');

			actualTextTrimmed.Should().Be(expectedTextTrimmed);
		}
	}

	[Test]
	public void ShouldRoundTrip()
	{
		// Arrange
		using (var stream = OpenOnDiskReference())
		{
			var reader = new StreamReader(stream, leaveOpen: true);

			var expectedText = reader.ReadToEnd();

			stream.Position = 0;
			reader = new StreamReader(stream);

			var unitName = "test";

			var parser = new BasicParser();

			var writer = new StringWriter();

			// Act
			var unit = CompilationUnit.Read(reader, unitName, parser);
			unit.Write(writer);

			// Assert
			string actualTextTrimmed = writer.ToString().TrimEnd('\r', '\n');
			string expectedTextTrimmed = expectedText.TrimEnd('\r', '\n');

			actualTextTrimmed.Should().Be(expectedTextTrimmed);
		}
	}
}
