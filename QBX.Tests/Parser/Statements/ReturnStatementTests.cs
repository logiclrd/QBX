using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

using static QBX.Tests.Utility.IdentifierHelpers;

namespace QBX.Tests.Parser.Statements;

public class ReturnStatementTests
{
	[TestCase("RETURN", null, null)]
	[TestCase("RETURN 100", "100", null)]
	[TestCase("RETURN OneHundred", null, "OneHundred")]
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
		result.Should().BeOfType<ReturnStatement>();

		var returnResult = (ReturnStatement)result;

		returnResult.TargetLabel.Should().Be(ID(expectedLabel));
		returnResult.TargetLineNumber.Should().Be(ID(expectedLineNumber));
	}
}
