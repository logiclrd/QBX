using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class WEndStatementTests
{
	[Test]
	public void ShouldParse()
	{
		// Arrange
		var tokens = new Lexer("WEND").ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ref inType);

		// Assert
		result.Should().BeOfType<WEndStatement>();
	}
}
