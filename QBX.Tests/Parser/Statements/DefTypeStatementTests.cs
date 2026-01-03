using QBX.CodeModel;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class DefTypeStatementTests
{
	[TestCase(DataType.INTEGER, "DEFINT", 'A', null)]
	[TestCase(DataType.INTEGER, "DEFINT", 'A', 'Z')]
	[TestCase(DataType.LONG, "DEFLNG", 'B', null)]
	[TestCase(DataType.LONG, "DEFLNG", 'B', 'Z')]
	[TestCase(DataType.SINGLE, "DEFSNG", 'D', null)]
	[TestCase(DataType.SINGLE, "DEFSNG", 'D', 'Z')]
	[TestCase(DataType.DOUBLE, "DEFDBL", 'F', null)]
	[TestCase(DataType.DOUBLE, "DEFDBL", 'F', 'Z')]
	[TestCase(DataType.STRING, "DEFSTR", 'R', null)]
	[TestCase(DataType.STRING, "DEFSTR", 'R', 'Z')]
	[TestCase(DataType.CURRENCY, "DEFCUR", 'T', null)]
	[TestCase(DataType.CURRENCY, "DEFCUR", 'T', 'Z')]
	public void ShouldParse(DataType dataType, string statement, char rangeStart, char? rangeEnd)
	{
		// Arrange
		var text = rangeEnd.HasValue
			? $"{statement} {rangeStart}-{rangeEnd}"
			: $"{statement} {rangeStart}";

		var tokens = new Lexer(text).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ref inType);

		// Assert
		result.Should().BeOfType<DefTypeStatement>();

		var defTypeResult = (DefTypeStatement)result;

		defTypeResult.DataType.Should().Be(dataType);
		defTypeResult.Ranges.Should().HaveCount(1);
		defTypeResult.Ranges[0].Start.Should().Be(rangeStart);
		defTypeResult.Ranges[0].End.Should().Be(rangeEnd);
	}

	[Test]
	public void ShouldParseMultiple()
	{
		// Arrange
		var text = "DEFINT A-C, F, H-I, Z";

		var tokens = new Lexer(text).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ref inType);

		// Assert
		result.Should().BeOfType<DefTypeStatement>();

		var defTypeResult = (DefTypeStatement)result;

		defTypeResult.DataType.Should().Be(DataType.INTEGER);
		defTypeResult.Ranges.Should().HaveCount(4);
		defTypeResult.Ranges[0].Start.Should().Be('A');
		defTypeResult.Ranges[0].End.Should().Be('C');
		defTypeResult.Ranges[1].Start.Should().Be('F');
		defTypeResult.Ranges[1].End.Should().BeNull();
		defTypeResult.Ranges[2].Start.Should().Be('H');
		defTypeResult.Ranges[2].End.Should().Be('I');
		defTypeResult.Ranges[3].Start.Should().Be('Z');
		defTypeResult.Ranges[3].End.Should().BeNull();
	}
}
