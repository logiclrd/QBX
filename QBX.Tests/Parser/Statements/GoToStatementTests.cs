using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class GoToStatementTests
{
	[TestCase("GOTO 100", "100", null)]
	[TestCase("GOTO OneHundred", null, "OneHundred")]
	public void ShouldParse(string statement, string? expectedLineNumber, string? expectedLabel)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens);

		// Assert
		result.Should().BeOfType<GoToStatement>();

		var gotoResult = (GoToStatement)result;

		gotoResult.TargetLineNumber.Should().Be(expectedLineNumber);
		gotoResult.TargetLabel.Should().Be(expectedLabel);
	}
}
