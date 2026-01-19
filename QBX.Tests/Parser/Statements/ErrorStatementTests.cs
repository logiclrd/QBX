using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class ErrorStatementTests
{
	[TestCase("ERROR 0")]
	[TestCase("ERROR a% + b%")]
	public void ShouldParse(string statement)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<ErrorStatement>();

		var errorResult = (ErrorStatement)result;

		errorResult.ErrorNumberExpression.Should().NotBeNull();
	}
}
