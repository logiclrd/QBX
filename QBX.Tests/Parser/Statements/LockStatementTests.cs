using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class LockStatementTests
{
	[TestCase("LOCK 1", false, false)]
	[TestCase("LOCK #2, 32", true, false)]
	[TestCase("LOCK #fn%, startPos& TO endPos&", true, true)]
	public void ShouldParse(string statement, bool expectRangeStart, bool expectRangeEnd)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var identifierRepository = new IdentifierRepository();

		var sut = new BasicParser(identifierRepository);

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<LockStatement>();

		var lockResult = (LockStatement)result;

		lockResult.FileNumberExpression.Should().NotBeNull();

		if (expectRangeStart)
			lockResult.RangeStartExpression.Should().NotBeNull();
		else
			lockResult.RangeStartExpression.Should().BeNull();

		if (expectRangeEnd)
			lockResult.RangeEndExpression.Should().NotBeNull();
		else
			lockResult.RangeEndExpression.Should().BeNull();
	}
}
