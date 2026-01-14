using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class ExitScopeStatementTests
{
	[TestCase("EXIT DEF", ScopeType.Def)]
	[TestCase("EXIT FUNCTION", ScopeType.Function)]
	[TestCase("EXIT SUB", ScopeType.Sub)]
	[TestCase("EXIT DO", ScopeType.Do)]
	[TestCase("EXIT FOR", ScopeType.For)]
	public void ShouldParse(string statement, ScopeType expectedScopeType)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<ExitScopeStatement>();

		var exitScopeResult = (ExitScopeStatement)result;

		exitScopeResult.ScopeType.Should().Be(expectedScopeType);
	}
}
