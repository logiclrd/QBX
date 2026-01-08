using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class TextViewportStatementTests
{
	[TestCase("VIEW PRINT", false)]
	[TestCase("VIEW PRINT 3 TO 5", true)]
	[TestCase("VIEW PRINT a% + b% TO c% * d% MOD 3", true)]
	public void ShouldParse(string statement, bool expectRangeExpressions)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<TextViewportStatement>();

		var viewportResult = (TextViewportStatement)result;

		if (!expectRangeExpressions)
		{
			viewportResult.TopExpression.Should().BeNull();
			viewportResult.BottomExpression.Should().BeNull();
		}
		else
		{
			viewportResult.TopExpression.Should().NotBeNull();
			viewportResult.BottomExpression.Should().NotBeNull();
		}
	}
}
