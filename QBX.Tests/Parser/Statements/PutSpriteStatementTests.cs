using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class PutSpriteStatementTests
{
	[TestCase("PUT (1, 2), a",
		false, // step
		typeof(LiteralExpression), "1", // x
		typeof(LiteralExpression), "2", // y
		typeof(IdentifierExpression), "a", // source
		null)] // action verb
	[TestCase("PUT STEP(x%, y%), colourtable%(i%)",
		true, // step
		typeof(IdentifierExpression), "x%", // x
		typeof(IdentifierExpression), "y%", // y
		typeof(CallOrIndexExpression), "(", // source
		null)] // action verb
	[TestCase("PUT (1, 2), structure.fieldname, PSET",
		false, // step
		typeof(LiteralExpression), "1", // x
		typeof(LiteralExpression), "2", // y
		typeof(BinaryExpression), ".", // source
		PutSpriteAction.PixelSet)]
	[TestCase("PUT STEP(a% + b%, -d%), a(b + c), PRESET",
		true, // step
		typeof(BinaryExpression), "+", // x
		typeof(UnaryExpression), "-", // y
		typeof(CallOrIndexExpression), "(", // source
		PutSpriteAction.PixelSetInverted)]
	[TestCase("PUT (1, 2), structure.fieldname, AND",
		false, // step
		typeof(LiteralExpression), "1", // x
		typeof(LiteralExpression), "2", // y
		typeof(BinaryExpression), ".", // source
		PutSpriteAction.And)]
	[TestCase("PUT STEP(a% + b%, -d%), a(b + c), OR",
		true, // step
		typeof(BinaryExpression), "+", // x
		typeof(UnaryExpression), "-", // y
		typeof(CallOrIndexExpression), "(", // source
		PutSpriteAction.Or)]
	[TestCase("PUT STEP(a% + b%, -d%), a(b + c), XOR",
		true, // step
		typeof(BinaryExpression), "+", // x
		typeof(UnaryExpression), "-", // y
		typeof(CallOrIndexExpression), "(", // source
		PutSpriteAction.ExclusiveOr)]
	public void ShouldParse(string statement,
		bool expectedStep,
		Type? expectedXExpressionType, string expectedXExpressionTokenValue,
		Type? expectedYExpressionType, string expectedYExpressionTokenValue,
		Type? expectedSourceExpressionType, string expectedSourceExpressionTokenValue,
		PutSpriteAction? expectedActionVerb)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<PutSpriteStatement>();

		var getSpriteResult = (PutSpriteStatement)result;

		getSpriteResult.Step.Should().Be(expectedStep);

		if (expectedXExpressionType == null)
		{
			getSpriteResult.XExpression.Should().BeNull();
			getSpriteResult.YExpression.Should().BeNull();
		}
		else
		{
			getSpriteResult.XExpression.Should().BeOfType(expectedXExpressionType)
				.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedXExpressionTokenValue);
			getSpriteResult.YExpression.Should().BeOfType(expectedYExpressionType)
				.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedYExpressionTokenValue);
		}

		if (expectedSourceExpressionType == null)
			getSpriteResult.SourceExpression.Should().BeNull();
		else
		{
			getSpriteResult.SourceExpression.Should().BeOfType(expectedSourceExpressionType)
				.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedSourceExpressionTokenValue);
		}

		getSpriteResult.ActionVerb.Should().Be(expectedActionVerb);
	}
}
