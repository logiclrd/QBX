using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class EndScopeStatementTests
{
	[TestCase("END SUB", ScopeType.Sub)]
	[TestCase("END FUNCTION", ScopeType.Function)]
	public void ShouldParse(string statement, ScopeType expectedScopeType)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ref inType);

		// Assert
		result.Should().BeOfType<EndScopeStatement>();

		var endScopeResult = (EndScopeStatement)result;

		endScopeResult.ScopeType.Should().Be(expectedScopeType);
	}
}
