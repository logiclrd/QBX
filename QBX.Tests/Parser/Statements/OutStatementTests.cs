using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class OutStatementTests
{
	[TestCase("OUT 100, 5",
		typeof(LiteralExpression), "100",
		typeof(LiteralExpression), "5")]
	[TestCase("OUT porttable(action), a% + r%",
		typeof(CallOrIndexExpression), "(",
		typeof(BinaryExpression), "+")]
	public void ShouldParse(
		string statement,
		Type expectedPortExpressionType, string expectedPortExpressionValue,
		Type expectedDataExpressionType, string expectedDataExpressionValue)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<OutStatement>();

		var outResult = (OutStatement)result;

		outResult.PortExpression.Should().BeOfType(expectedPortExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedPortExpressionValue);
		outResult.DataExpression.Should().BeOfType(expectedDataExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedDataExpressionValue);
	}
}
