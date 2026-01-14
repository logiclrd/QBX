using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class ResumeStatementTests
{
	[TestCase("RESUME", true, false, null, null)]
	[TestCase("RESUME 0", true, false, "0", null)]
	[TestCase("RESUME 350", false, false, "350", null)]
	[TestCase("RESUME Label", false, false, null, "Label")]
	[TestCase("RESUME NEXT", false, true, null, null)]
	public void ShouldParse(string statement, bool expectedSameStatement, bool expectedNextStatement, string? expectedTargetLineNumber, string? expectedTargetLabel)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<ResumeStatement>();

		var resumeResult = (ResumeStatement)result;

		resumeResult.SameStatement.Should().Be(expectedSameStatement);
		resumeResult.NextStatement.Should().Be(expectedNextStatement);
		resumeResult.TargetLineNumber.Should().Be(expectedTargetLineNumber);
		resumeResult.TargetLabel.Should().Be(expectedTargetLabel);
	}
}
