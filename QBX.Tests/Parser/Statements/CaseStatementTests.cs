using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class CaseStatementTests
{
	[TestCase("CASE 1", 1)]
	[TestCase("CASE \"test\"", 1)]
	[TestCase("CASE NEXTLEVEL", 1)]
	[TestCase("CASE 2, a%, b% + c%", 3)]
	public void ShouldParse(string statement, int expectedExpressionCount)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, colonAfter: false, ref inType);

		// Assert
		result.Should().BeOfType<CaseStatement>();

		var caseResult = (CaseStatement)result;

		caseResult.Expressions.Expressions.Should().HaveCount(expectedExpressionCount);
	}
}
