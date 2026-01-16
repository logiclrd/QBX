using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class SoftKeyConfigStatementTests
{
	public void ShouldParse()
	{
		// Arrange
		var tokens = new Lexer("KEY a%, b$").ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<SoftKeyConfigStatement>();

		var keyResult = (SoftKeyConfigStatement)result;

		keyResult.KeyExpression.Should().BeOfType<IdentifierExpression>()
			.Which.Token!.Value.Should().Be("a%");
		keyResult.MacroExpression.Should().BeOfType<IdentifierExpression>()
			.Which.Token!.Value.Should().Be("b$");
	}
}
