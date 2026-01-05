using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class LineStatementTests
{
	[TestCase("LINE (1, 2)-(3, 4)",
		false, // from step
		typeof(LiteralExpression), "1", // x
		typeof(LiteralExpression), "2", // y
		false, // to step
		typeof(LiteralExpression), "3", // x
		typeof(LiteralExpression), "4", // y
		null, "", // colour
		LineDrawStyle.Line,
		null, "")] // style
	[TestCase("LINE STEP(x%, y%)-(3, 4), colourtable%(i%)",
		true, // from step
		typeof(IdentifierExpression), "x%", // x
		typeof(IdentifierExpression), "y%", // y
		false, // to step
		typeof(LiteralExpression), "3", // x
		typeof(LiteralExpression), "4", // y
		typeof(CallOrIndexExpression), "", // colour
		LineDrawStyle.Line,
		null, "")] // style
	[TestCase("LINE (1, 2)-STEP(x%, y%), , B",
		false, // from step
		typeof(LiteralExpression), "1", // x
		typeof(LiteralExpression), "2", // y
		true, // to step
		typeof(IdentifierExpression), "x%", // x
		typeof(IdentifierExpression), "y%", // y
		null, "", // colour
		LineDrawStyle.Box,
		null, "")] // style
	[TestCase("LINE STEP(a% + b%, -d%)-STEP(x%, y%), , BF, 13",
		true, // from step
		typeof(BinaryExpression), "", // x
		typeof(UnaryExpression), "", // y
		true, // to step
		typeof(IdentifierExpression), "x%", // x
		typeof(IdentifierExpression), "y%", // y
		null, "", // colour
		LineDrawStyle.FilledBox,
		typeof(LiteralExpression), "13")] // style
	public void ShouldParse(string statement,
		bool expectedFromStep,
		Type? expectedFromXExpressionType, string expectedFromXExpressionTokenValue,
		Type? expectedFromYExpressionType, string expectedFromYExpressionTokenValue,
		bool expectedToStep,
		Type expectedToXExpressionType, string expectedToXExpressionTokenValue,
		Type expectedToYExpressionType, string expectedToYExpressionTokenValue,
		Type? expectedColourExpressionType, string expectedColourExpressionTokenValue,
		LineDrawStyle expectedDrawStyle,
		Type? expectedStyleExpressionType, string expectedStyleExpressionTokenValue)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens);

		// Assert
		result.Should().BeOfType<LineStatement>();

		var lineResult = (LineStatement)result;

		lineResult.FromStep.Should().Be(expectedFromStep);

		if (expectedFromXExpressionType == null)
		{
			lineResult.FromXExpression.Should().BeNull();
			lineResult.FromYExpression.Should().BeNull();
		}
		else
		{
			lineResult.FromXExpression.Should().BeOfType(expectedFromXExpressionType)
				.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedFromXExpressionTokenValue);
			lineResult.FromYExpression.Should().BeOfType(expectedFromYExpressionType)
				.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedFromYExpressionTokenValue);
		}

		lineResult.ToStep.Should().Be(expectedToStep);

		lineResult.ToXExpression.Should().BeOfType(expectedToXExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedToXExpressionTokenValue);
		lineResult.ToYExpression.Should().BeOfType(expectedToYExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedToYExpressionTokenValue);

		if (expectedColourExpressionType == null)
			lineResult.ColourExpression.Should().BeNull();
		else
		{
			lineResult.ColourExpression.Should().BeOfType(expectedColourExpressionType)
				.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedColourExpressionTokenValue);
		}

		lineResult.DrawStyle.Should().Be(expectedDrawStyle);

		if (expectedStyleExpressionType == null)
			lineResult.StyleExpression.Should().BeNull();
		else
		{
			lineResult.StyleExpression.Should().BeOfType(expectedStyleExpressionType)
				.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedStyleExpressionTokenValue);
		}
	}
}
