using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class EmptyStatementTests
{
	[Test]
	public void ShouldParse()
	{
		// Arrange
		var text = "";

		var tokens = new Lexer(text).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var identifierRepository = new IdentifierRepository();

		var sut = new BasicParser(identifierRepository);

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<EmptyStatement>();
	}
}
