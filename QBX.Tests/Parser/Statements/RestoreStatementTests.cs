using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class RestoreStatementTests
{
	[TestCase("RESTORE", null, null)]
	[TestCase("RESTORE 100", "100", null)]
	[TestCase("RESTORE OneHundred", null, "OneHundred")]
	public void ShouldParse(string statement, string? expectedLineNumber, string? expectedLabel)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens);

		// Assert
		result.Should().BeOfType<RestoreStatement>();

		var restoreResult = (RestoreStatement)result;

		restoreResult.TargetLabel.Should().Be(expectedLabel);
		restoreResult.TargetLineNumber.Should().Be(expectedLineNumber);
	}
}
