using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

using static QBX.Tests.Utility.IdentifierHelpers;

namespace QBX.Tests.Parser.Statements;

public class RestoreStatementTests
{
	[TestCase("RESTORE", null, null)]
	[TestCase("RESTORE 100", "100", null)]
	[TestCase("RESTORE OneHundred", null, "OneHundred")]
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
		result.Should().BeOfType<RestoreStatement>();

		var restoreResult = (RestoreStatement)result;

		restoreResult.TargetLabel.Should().Be(ID(expectedLabel));
		restoreResult.TargetLineNumber.Should().Be(ID(expectedLineNumber));
	}
}
