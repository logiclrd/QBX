using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class PaintStatementTests
{
	[TestCase("PAINT (1, 2), 3",
		false, // step
		typeof(LiteralExpression), "1", // x
		typeof(LiteralExpression), "2", // y
		typeof(LiteralExpression), "3", // paint
		null, "", // border colour
		null, "")] // background
	[TestCase("PAINT STEP(a%, 2), 3",
		true, // step
		typeof(IdentifierExpression), "a%", // x
		typeof(LiteralExpression), "2", // y
		typeof(LiteralExpression), "3", // paint
		null, "", // border colour
		null, "")] // background
	[TestCase("PAINT (1, 2), , 4",
		false, // step
		typeof(LiteralExpression), "1", // x
		typeof(LiteralExpression), "2", // y
		null, "", // paint
		typeof(LiteralExpression), "4", // border colour
		null, "")] // background
	[TestCase("PAINT (1, 2), 3, , 5",
		false, // step
		typeof(LiteralExpression), "1", // x
		typeof(LiteralExpression), "2", // y
		typeof(LiteralExpression), "3", // paint
		null, "", // border colour
		typeof(LiteralExpression), "5")] // background
	[TestCase("PAINT STEP(x%, y%), r1% + r2%, c%, getstart%(a)",
		true, // step
		typeof(IdentifierExpression), "x%", // x
		typeof(IdentifierExpression), "y%", // y
		typeof(BinaryExpression), "+", // paint
		typeof(IdentifierExpression), "c%", // border colour
		typeof(CallOrIndexExpression), "(")] // background
	public void ShouldParse(string statement,
		bool expectedStep,
		Type expectedXExpressionType, string expectedXExpressionTokenValue,
		Type expectedYExpressionType, string expectedYExpressionTokenValue,
		Type? expectedPaintExpressionType, string expectedPaintExpressionTokenValue,
		Type? expectedBorderColourExpressionType, string expectedBorderColourExpressionTokenValue,
		Type? expectedBackgroundExpressionType, string expectedBackgroundExpressionTokenValue)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<PaintStatement>();

		var paintResult = (PaintStatement)result;

		paintResult.Step.Should().Be(expectedStep);

		paintResult.XExpression.Should().BeOfType(expectedXExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedXExpressionTokenValue);
		paintResult.YExpression.Should().BeOfType(expectedYExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedYExpressionTokenValue);

		if (expectedPaintExpressionType == null)
			paintResult.PaintExpression.Should().BeNull();
		else
		{
			paintResult.PaintExpression.Should().BeOfType(expectedPaintExpressionType)
				.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedPaintExpressionTokenValue);
		}

		if (expectedBorderColourExpressionType == null)
			paintResult.BorderColourExpression.Should().BeNull();
		else
		{
			paintResult.BorderColourExpression.Should().BeOfType(expectedBorderColourExpressionType)
				.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedBorderColourExpressionTokenValue);
		}

		if (expectedBackgroundExpressionType == null)
			paintResult.BackgroundExpression.Should().BeNull();
		else
		{
			paintResult.BackgroundExpression.Should().BeOfType(expectedBackgroundExpressionType)
				.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedBackgroundExpressionTokenValue);
		}
	}
}
