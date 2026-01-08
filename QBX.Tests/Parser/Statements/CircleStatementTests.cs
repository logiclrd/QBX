using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class CircleStatementTests
{
	[TestCase("CIRCLE (1, 2), 3",
		false, // step
		typeof(LiteralExpression), "1", // x
		typeof(LiteralExpression), "2", // y
		typeof(LiteralExpression), "3", // radius
		null, "", // colour
		null, "", // start
		null, "", // end
		null, "")] // aspect
	[TestCase("CIRCLE STEP(a%, 2), 3",
		true, // step
		typeof(IdentifierExpression), "a%", // x
		typeof(LiteralExpression), "2", // y
		typeof(LiteralExpression), "3", // radius
		null, "", // colour
		null, "", // start
		null, "", // end
		null, "")] // aspect
	[TestCase("CIRCLE (1, 2), 3, 4",
		false, // step
		typeof(LiteralExpression), "1", // x
		typeof(LiteralExpression), "2", // y
		typeof(LiteralExpression), "3", // radius
		typeof(LiteralExpression), "4", // colour
		null, "", // start
		null, "", // end
		null, "")] // aspect
	[TestCase("CIRCLE (1, 2), 3, , 5",
		false, // step
		typeof(LiteralExpression), "1", // x
		typeof(LiteralExpression), "2", // y
		typeof(LiteralExpression), "3", // radius
		null, "", // colour
		typeof(LiteralExpression), "5", // start
		null, "", // end
		null, "")] // aspect
	[TestCase("CIRCLE (1, 2), 3, , , 6",
		false, // step
		typeof(LiteralExpression), "1", // x
		typeof(LiteralExpression), "2", // y
		typeof(LiteralExpression), "3", // radius
		null, "", // colour
		null, "", // start
		typeof(LiteralExpression), "6", // end
		null, "")] // aspect
	[TestCase("CIRCLE (1, 2), 3, , , , 7",
		false, // step
		typeof(LiteralExpression), "1", // x
		typeof(LiteralExpression), "2", // y
		typeof(LiteralExpression), "3", // radius
		null, "", // colour
		null, "", // start
		null, "", // end
		typeof(LiteralExpression), "7")] // aspect
	[TestCase("CIRCLE STEP(x%, y%), r1% + r2%, c%, getstart%(a), 6.2831, aspect!",
		true, // step
		typeof(IdentifierExpression), "x%", // x
		typeof(IdentifierExpression), "y%", // y
		typeof(BinaryExpression), "+", // radius
		typeof(IdentifierExpression), "c%", // colour
		typeof(CallOrIndexExpression), "(", // start
		typeof(LiteralExpression), "6.2831", // end
		typeof(IdentifierExpression), "aspect!")] // aspect
	public void ShouldParse(string statement,
		bool expectedStep,
		Type expectedXExpressionType, string expectedXExpressionTokenValue,
		Type expectedYExpressionType, string expectedYExpressionTokenValue,
		Type expectedRadiusExpressionType, string expectedRadiusExpressionTokenValue,
		Type? expectedColourExpressionType, string expectedColourExpressionTokenValue,
		Type? expectedStartExpressionType, string expectedStartExpressionTokenValue,
		Type? expectedEndExpressionType, string expectedEndExpressionTokenValue,
		Type? expectedAspectExpressionType, string expectedAspectExpressionTokenValue)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<CircleStatement>();

		var circleResult = (CircleStatement)result;

		circleResult.Step.Should().Be(expectedStep);

		circleResult.XExpression.Should().BeOfType(expectedXExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedXExpressionTokenValue);
		circleResult.YExpression.Should().BeOfType(expectedYExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedYExpressionTokenValue);
		circleResult.RadiusExpression.Should().BeOfType(expectedRadiusExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedRadiusExpressionTokenValue);

		if (expectedColourExpressionType == null)
			circleResult.ColourExpression.Should().BeNull();
		else
		{
			circleResult.ColourExpression.Should().BeOfType(expectedColourExpressionType)
				.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedColourExpressionTokenValue);
		}

		if (expectedStartExpressionType == null)
			circleResult.StartExpression.Should().BeNull();
		else
		{
			circleResult.StartExpression.Should().BeOfType(expectedStartExpressionType)
				.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedStartExpressionTokenValue);
		}

		if (expectedEndExpressionType == null)
			circleResult.EndExpression.Should().BeNull();
		else
		{
			circleResult.EndExpression.Should().BeOfType(expectedEndExpressionType)
				.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedEndExpressionTokenValue);
		}

		if (expectedAspectExpressionType == null)
			circleResult.AspectExpression.Should().BeNull();
		else
		{
			circleResult.AspectExpression.Should().BeOfType(expectedAspectExpressionType)
				.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedAspectExpressionTokenValue);
		}
	}
}
