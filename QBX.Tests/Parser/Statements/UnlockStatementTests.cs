using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class UnlockStatementTests
{
	[TestCase("UNLOCK 1", false, false)]
	[TestCase("UNLOCK #2, 32", true, false)]
	[TestCase("UNLOCK #fn%, start& TO end&", true, true)]
	public void ShouldParse(string statement, bool expectRangeStart, bool expectRangeEnd)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ref inType);

		// Assert
		result.Should().BeOfType<UnlockStatement>();

		var unlockResult = (UnlockStatement)result;

		unlockResult.FileNumberExpression.Should().NotBeNull();

		if (expectRangeStart)
			unlockResult.RangeStartExpression.Should().NotBeNull();
		else
			unlockResult.RangeStartExpression.Should().BeNull();

		if (expectRangeEnd)
			unlockResult.RangeEndExpression.Should().NotBeNull();
		else
			unlockResult.RangeEndExpression.Should().BeNull();
	}
}
