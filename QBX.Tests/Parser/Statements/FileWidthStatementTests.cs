using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class FileWidthStatementTests
{
	[TestCase("WIDTH #1, 45", typeof(LiteralExpression), "1", typeof(LiteralExpression), "45")]
	[TestCase("WIDTH #a% + b%, c%", typeof(BinaryExpression), "+", typeof(IdentifierExpression), "c%")]
	public void ShouldParse(
		string statement,
		Type expectedFileNumberExpressionType, string expectedFileNumberExpressionTokenValue,
		Type expectedWidthExpressionType, string expectedWidthExpressionTokenValue)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var identifierRepository = new IdentifierRepository();

		var sut = new BasicParser(identifierRepository);

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<FileWidthStatement>();

		var fileWidthResult = (FileWidthStatement)result;

		fileWidthResult.FileNumberExpression.Should().BeOfType(expectedFileNumberExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedFileNumberExpressionTokenValue);
		fileWidthResult.WidthExpression.Should().BeOfType(expectedWidthExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedWidthExpressionTokenValue);
	}
}
