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

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ref inType);

		// Assert
		result.Should().BeOfType<EndStatement>();

		var endResult = (EndStatement)result;

		if (expectExitCodeExpression)
			endResult.Expression.Should().NotBeNull();
		else
			endResult.Expression.Should().BeNull();
	}
}
