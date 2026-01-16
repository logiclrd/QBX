using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class SoftKeyControlStatementTests
{
	[TestCase("KEY ON", true)]
	[TestCase("KEY OFF", false)]
	public void ShouldParse(string statement, bool expectedEnable)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<SoftKeyControlStatement>();

		var keyResult = (SoftKeyControlStatement)result;

		keyResult.Enable.Should().Be(expectedEnable);
	}
}
