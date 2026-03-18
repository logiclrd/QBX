using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class WaitStatementTests
{
	[TestCase("WAIT 100, 5",
		typeof(LiteralExpression), "100",
		typeof(LiteralExpression), "5",
		null, null)]
	[TestCase("WAIT porttable(action), a% + r%, 32",
		typeof(CallOrIndexExpression), "(",
		typeof(BinaryExpression), "+",
		typeof(LiteralExpression), "32")]
	public void ShouldParse(
		string statement,
		Type expectedPortExpressionType, string expectedPortExpressionValue,
		Type expectedAndExpressionType, string expectedAndExpressionValue,
		Type? expectedXOrExpressionType, string? expectedXOrExpressionValue)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var identifierRepository = new IdentifierRepository();

		var sut = new BasicParser(identifierRepository);

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<WaitStatement>();

		var waitResult = (WaitStatement)result;

		waitResult.PortExpression.Should().BeOfType(expectedPortExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedPortExpressionValue);
		waitResult.AndExpression.Should().BeOfType(expectedAndExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedAndExpressionValue);

		if (expectedXOrExpressionType is null)
			waitResult.XOrExpression.Should().BeNull();
		else
		{
			waitResult.XOrExpression.Should().BeOfType(expectedXOrExpressionType)
				.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedXOrExpressionValue);
		}
	}
}
