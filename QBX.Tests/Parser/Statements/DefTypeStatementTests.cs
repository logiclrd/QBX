using QBX.CodeModel;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class DefTypeStatementTests
{
	[TestCase(DataType.INTEGER, "DEFINT", 'A', 'Z')]
	[TestCase(DataType.LONG, "DEFLNG", 'B', 'Z')]
	[TestCase(DataType.SINGLE, "DEFSNG", 'D', 'Z')]
	[TestCase(DataType.DOUBLE, "DEFDBL", 'F', 'Z')]
	[TestCase(DataType.STRING, "DEFSTR", 'R', 'Z')]
	[TestCase(DataType.CURRENCY, "DEFCUR", 'T', 'Z')]
	public void ShouldParse(DataType dataType, string statement, char rangeStart, char rangeEnd)
	{
		// Arrange
		var text = $"{statement} {rangeStart}-{rangeEnd}";

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
		defTypeResult.RangeStart.Should().Be(rangeStart);
		defTypeResult.RangeEnd.Should().Be(rangeEnd);
	}
}
