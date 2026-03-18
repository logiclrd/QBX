using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

using static QBX.Tests.Utility.IdentifierHelpers;

namespace QBX.Tests.Parser.Statements;

public class GoSubStatementTests
{
	[TestCase("GOSUB 100", "100", null)]
	[TestCase("GOSUB OneHundred", null, "OneHundred")]
	public void ShouldParse(string statement, string? expectedLineNumber, string? expectedLabel)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var identifierRepository = new IdentifierRepository();

		var sut = new BasicParser(identifierRepository);

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<GoSubStatement>();

		var goSubResult = (GoSubStatement)result;

		goSubResult.TargetLineNumber.Should().Be(ID(expectedLineNumber));
		goSubResult.TargetLabel.Should().Be(ID(expectedLabel));
	}
}
