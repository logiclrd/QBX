using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class PageCopyStatementTests
{
	[Test]
	public void ShouldParse()
	{
		// Arrange
		string statement = "PCOPY 1, 2";

		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ref inType);

		// Assert
		result.Should().BeOfType<PageCopyStatement>();

		var pageCopyResult = (PageCopyStatement)result;

		pageCopyResult.SourcePageExpression.Should().BeOfType<LiteralExpression>().Which.Token!.Value.Should().Be("1");
		pageCopyResult.DestinationPageExpression.Should().BeOfType<LiteralExpression>().Which.Token!.Value.Should().Be("2");
	}
}
