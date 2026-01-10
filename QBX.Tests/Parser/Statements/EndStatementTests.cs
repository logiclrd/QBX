using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class EndStatementTests
{
	[TestCase("END", false)]
	[TestCase("END 0", true)]
	[TestCase("END a% + b%", true)]
	public void ShouldParse(string statement, bool expectExitCodeExpression)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<EndStatement>();

		var endResult = (EndStatement)result;

		if (expectExitCodeExpression)
			endResult.ExitCodeExpression.Should().NotBeNull();
		else
			endResult.ExitCodeExpression.Should().BeNull();
	}
}
