using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class CloseStatementTests
{
	[TestCase("CLOSE", 0)]
	[TestCase("CLOSE 1", 1)]
	[TestCase("CLOSE #1, 2", 2)]
	[TestCase("CLOSE #1, #2, #a% + b%", 3)]
	public void ShouldParse(string statement, int argCount)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<CloseStatement>();

		var closeResult = (CloseStatement)result;

		closeResult.FileNumberExpressions.Should().HaveCount(argCount);
	}
}
