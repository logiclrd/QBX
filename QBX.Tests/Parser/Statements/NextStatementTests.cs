using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class NextStatementTests
{
	[TestCase("NEXT", new string[0])]
	[TestCase("NEXT a%", new string[] { "a%" })]
	[TestCase("NEXT k, j, i", new string[] { "k", "j", "i" })]
	public void ShouldParse(string statement, string[] expectedVariables)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ref inType);

		// Assert
		result.Should().BeOfType<NextStatement>();

		var nextResult = (NextStatement)result;

		nextResult.CounterExpressions.Should().HaveCount(expectedVariables.Length);

		for (int i = 0; i < expectedVariables.Length; i++)
		{
			nextResult.CounterExpressions[i].Should().BeOfType<IdentifierExpression>()
				.Which.Token!.Value.Should().Be(expectedVariables[i]);
		}
	}
}
