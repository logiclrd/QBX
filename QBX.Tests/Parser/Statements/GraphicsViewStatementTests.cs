using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class GraphicsViewportStatementTests
{
	[TestCase("VIEW",
		false,
		null, null, null, null, null, null, null, null,
		null, null, null, null)]
	[TestCase("VIEW (1, 2)-(3, 4)",
		false,
		typeof(LiteralExpression), "1",
		typeof(LiteralExpression), "2",
		typeof(LiteralExpression), "3",
		typeof(LiteralExpression), "4",
		null, null, null, null)]
	[TestCase("VIEW SCREEN (a, b)-(c + d, e(f)), g(h) + i, 3",
		true,
		typeof(IdentifierExpression), "a",
		typeof(IdentifierExpression), "b",
		typeof(BinaryExpression), "",
		typeof(CallOrIndexExpression), null,
		typeof(BinaryExpression), "",
		typeof(LiteralExpression), "3")]
	public void ShouldParse(
		string statement,
		bool expectAbsoluteCoordinates,
		Type? expectFromXType, string? expectFromXValue,
		Type? expectFromYType, string? expectFromYValue,
		Type? expectToXType, string? expectToXValue,
		Type? expectToYType, string? expectToYValue,
		Type? expectFillColourType, string? expectFillColourValue,
		Type? expectBorderColourType, string? expectBorderColourValue)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ref inType);

		// Assert
		result.Should().BeOfType<GraphicsViewportStatement>();

		var viewportResult = (GraphicsViewportStatement)result;

		viewportResult.AbsoluteCoordinates.Should().Be(expectAbsoluteCoordinates);

		[CustomAssertion]
		void ExpectExpression(Expression? value, Type? expectType, string? expectValue)
		{
			if (expectType == null)
				value.Should().BeNull();
			else
			{
				value.Should().BeOfType(expectType).And.BeAssignableTo<Expression>()
					.Which.Token!.Value.Should().Be(expectValue);
			}
		}

		ExpectExpression(viewportResult.FromXExpression, expectFromXType, expectFromXValue);
		ExpectExpression(viewportResult.FromYExpression, expectFromYType, expectFromYValue);
		ExpectExpression(viewportResult.ToXExpression, expectToXType, expectToXValue);
		ExpectExpression(viewportResult.ToYExpression, expectToYType, expectToYValue);
		ExpectExpression(viewportResult.FillColourExpression, expectFillColourType, expectFillColourValue);
		ExpectExpression(viewportResult.BorderColourExpression, expectBorderColourType, expectBorderColourValue);
	}
}
