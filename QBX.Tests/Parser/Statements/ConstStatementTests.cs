using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class ConstStatementTests
{
	[TestCase("CONST TRUE = -1", "TRUE")]
	[TestCase("CONST FALSE = NOT TRUE", "FALSE")]
	[TestCase("CONST MAXSNAKELENGTH = 1000", "MAXSNAKELENGTH")]
	public void ShouldParse(string definition, string constantName)
	{
		// Arrange
		var tokens = new Lexer(definition).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var identifierRepository = new IdentifierRepository();

		var sut = new BasicParser(identifierRepository);

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<ConstStatement>();

		var constResult = (ConstStatement)result;

		constResult.Definitions.Count.Should().Be(1);
		constResult.Definitions[0].Identifier.Value.Should().Be(constantName);
	}
}
