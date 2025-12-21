using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class ReadStatementTests
{
	[TestCase("READ a", 1)]
	[TestCase("READ b, c", 2)]
	[TestCase("READ d(e)", 1)]
	[TestCase("READ f.g", 1)]
	[TestCase("READ h(i).j", 1)]
	[TestCase("READ k.l(m)", 1)]
	[TestCase("READ d(e), f.g, h(i).j, k.l(m)", 4)]
	public void ShouldParse(string statement, int expectedTargetCount)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ref inType);

		// Assert
		result.Should().BeOfType<ReadStatement>();

		var readResult = (ReadStatement)result;

		readResult.Targets.Should().HaveCount(expectedTargetCount);
	}
}
