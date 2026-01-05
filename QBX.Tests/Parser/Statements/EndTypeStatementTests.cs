using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class EndTypeStatementTests
{
	[Test]
	public void ShouldParse()
	{
		// Arrange
		var tokens = new Lexer("END TYPE").ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens);

		// Assert
		result.Should().BeOfType<EndTypeStatement>();
	}
}
