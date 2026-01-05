using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class PaletteStatementTests
{
	[TestCase("PALETTE", null, null, null)]
	[TestCase("PALETTE i%, col&(i%)", null, typeof(IdentifierExpression), typeof(CallOrIndexExpression))]
	[TestCase("PALETTE USING col&", typeof(IdentifierExpression), null, null)]
	[TestCase("PALETTE USING col&(x% + y%)", typeof(CallOrIndexExpression), null, null)]
	public void ShouldParse(string statement, Type? expectedArrayExpressionType, Type? expectedAttributeExpressionType, Type? expectedColourExpressionType)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens);

		// Assert
		result.Should().BeOfType<PaletteStatement>();

		var paletteResult = (PaletteStatement)result;

		if (expectedArrayExpressionType == null)
			paletteResult.ArrayExpression.Should().BeNull();
		else
			paletteResult.ArrayExpression.Should().BeOfType(expectedArrayExpressionType);

		if (expectedAttributeExpressionType == null)
			paletteResult.AttributeExpression.Should().BeNull();
		else
			paletteResult.AttributeExpression.Should().BeOfType(expectedAttributeExpressionType);

		if (expectedColourExpressionType == null)
			paletteResult.ColourExpression.Should().BeNull();
		else
			paletteResult.ColourExpression.Should().BeOfType(expectedColourExpressionType);
	}
}
