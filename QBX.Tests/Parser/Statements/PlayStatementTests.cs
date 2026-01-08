using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class PlayStatementTests
{
	[TestCase("PLAY \"ABCDE\"")]
	[TestCase("PLAY song$")]
	[TestCase("PLAY part1$ + part2$")]
	public void ShouldParse(string statement)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<PlayStatement>();
	}
}
