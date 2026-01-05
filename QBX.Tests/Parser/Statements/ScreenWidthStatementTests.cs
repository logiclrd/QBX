using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class WidthStatementTests
{
	[TestCase("WIDTH 80", true, false)]
	[TestCase("WIDTH , 50", false, true)]
	[TestCase("WIDTH 80, 50", true, true)]
	public void ShouldParse(string statement, bool expectWidth, bool expectHeight)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens);

		// Assert
		if (result is UnresolvedWidthStatement unresolved)
			result = unresolved.ResolveToScreenWidth();

		result.Should().BeOfType<ScreenWidthStatement>();

		var widthResult = (ScreenWidthStatement)result;

		if (expectWidth)
			widthResult.WidthExpression.Should().BeOfType<LiteralExpression>().Which.Token!.Value.Should().Be("80");
		else
			widthResult.WidthExpression.Should().BeNull();

		if (expectHeight)
			widthResult.HeightExpression.Should().BeOfType<LiteralExpression>().Which.Token!.Value.Should().Be("50");
		else
			widthResult.HeightExpression.Should().BeNull();
	}
}
