using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class DefSegStatementTests
{
	[TestCase("DEF SEG", false)]
	[TestCase("DEF SEG = &HA000", true)]
	[TestCase("DEF SEG = ComputeSegment%(42)", true)]
	public void ShouldParse(string statement, bool expectSegmentExpression)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<DefSegStatement>();

		var defSegResult = (DefSegStatement)result;

		if (expectSegmentExpression)
			defSegResult.SegmentExpression.Should().NotBeNull();
		else
			defSegResult.SegmentExpression.Should().BeNull();
	}
}
