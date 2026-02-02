using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class BeepStatementTests
{
	public void ShouldParse(string statement, bool expectExitCodeExpression)
	{
		// Arrange
		var tokens = new Lexer("BEEP").ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<BeepStatement>();
	}
}
