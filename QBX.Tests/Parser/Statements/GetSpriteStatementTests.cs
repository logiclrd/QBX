using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class GetSpriteStatementTests
{
	[TestCase("GET (1, 2)-(3, 4), a",
		false, // from step
		typeof(LiteralExpression), "1", // x
		typeof(LiteralExpression), "2", // y
		false, // to step
		typeof(LiteralExpression), "3", // x
		typeof(LiteralExpression), "4", // y
		typeof(IdentifierExpression), "a")] // target
	[TestCase("GET STEP(x%, y%)-(3, 4), colourtable%(i%)",
		true, // from step
		typeof(IdentifierExpression), "x%", // x
		typeof(IdentifierExpression), "y%", // y
		false, // to step
		typeof(LiteralExpression), "3", // x
		typeof(LiteralExpression), "4", // y
		typeof(CallOrIndexExpression), "(")] // target
	[TestCase("GET (1, 2)-STEP(x%, y%), structure.fieldname",
		false, // from step
		typeof(LiteralExpression), "1", // x
		typeof(LiteralExpression), "2", // y
		true, // to step
		typeof(IdentifierExpression), "x%", // x
		typeof(IdentifierExpression), "y%", // y
		typeof(BinaryExpression), ".")] // target
	[TestCase("GET STEP(a% + b%, -d%)-STEP(x%, y%), a(b + c)",
		true, // from step
		typeof(BinaryExpression), "+", // x
		typeof(UnaryExpression), "-", // y
		true, // to step
		typeof(IdentifierExpression), "x%", // x
		typeof(IdentifierExpression), "y%", // y
		typeof(CallOrIndexExpression), "(")] // target
	public void ShouldParse(string statement,
		bool expectedFromStep,
		Type? expectedFromXExpressionType, string expectedFromXExpressionTokenValue,
		Type? expectedFromYExpressionType, string expectedFromYExpressionTokenValue,
		bool expectedToStep,
		Type expectedToXExpressionType, string expectedToXExpressionTokenValue,
		Type expectedToYExpressionType, string expectedToYExpressionTokenValue,
		Type? expectedTargetExpressionType, string expectedTargetExpressionTokenValue)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<GetSpriteStatement>();

		var getSpriteResult = (GetSpriteStatement)result;

		getSpriteResult.FromStep.Should().Be(expectedFromStep);

		if (expectedFromXExpressionType == null)
		{
			getSpriteResult.FromXExpression.Should().BeNull();
			getSpriteResult.FromYExpression.Should().BeNull();
		}
		else
		{
			getSpriteResult.FromXExpression.Should().BeOfType(expectedFromXExpressionType)
				.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedFromXExpressionTokenValue);
			getSpriteResult.FromYExpression.Should().BeOfType(expectedFromYExpressionType)
				.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedFromYExpressionTokenValue);
		}

		getSpriteResult.ToStep.Should().Be(expectedToStep);

		getSpriteResult.ToXExpression.Should().BeOfType(expectedToXExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedToXExpressionTokenValue);
		getSpriteResult.ToYExpression.Should().BeOfType(expectedToYExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedToYExpressionTokenValue);

		if (expectedTargetExpressionType == null)
			getSpriteResult.TargetExpression.Should().BeNull();
		else
		{
			getSpriteResult.TargetExpression.Should().BeOfType(expectedTargetExpressionType)
				.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedTargetExpressionTokenValue);
		}
	}
}
