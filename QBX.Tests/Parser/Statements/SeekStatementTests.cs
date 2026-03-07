using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class SeekStatementTests
{
	[TestCase("SEEK #1, 45", true, typeof(LiteralExpression), "1", typeof(LiteralExpression), "45")]
	[TestCase("SEEK #a% + b%, EXP(c!)", true, typeof(BinaryExpression), "+", typeof(KeywordFunctionExpression), "EXP")]
	[TestCase("SEEK filenumber(3 + 4), n&", false, typeof(CallOrIndexExpression), "(", typeof(IdentifierExpression), "n&")]
	public void ShouldParse(
		string statement,
		bool expectedIncludeNumberSign,
		Type expectedFileNumberExpressionType, string expectedFileNumberExpressionTokenValue,
		Type expectedPositionExpressionType, string expectedPositionExpressionTokenValue)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<SeekStatement>();

		var seekResult = (SeekStatement)result;

		seekResult.IncludeNumberSign.Should().Be(expectedIncludeNumberSign);

		seekResult.FileNumberExpression.Should().BeOfType(expectedFileNumberExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedFileNumberExpressionTokenValue);
		seekResult.PositionExpression.Should().BeOfType(expectedPositionExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedPositionExpressionTokenValue);
	}
}
