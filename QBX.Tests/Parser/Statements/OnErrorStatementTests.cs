using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class OnErrorStatementTests
{
	[TestCase("ON ERROR GOTO 0", false, OnErrorAction.DoNotHandle, null, null)]
	[TestCase("ON ERROR RESUME NEXT", false, OnErrorAction.ResumeNext, null, null)]
	[TestCase("ON ERROR GOTO 350", false, OnErrorAction.GoToHandler, "350", null)]
	[TestCase("ON ERROR GOTO ErrorHandler", false, OnErrorAction.GoToHandler, null, "ErrorHandler")]
	[TestCase("ON LOCAL ERROR GOTO 0", true, OnErrorAction.DoNotHandle, null, null)]
	[TestCase("ON LOCAL ERROR RESUME NEXT", true, OnErrorAction.ResumeNext, null, null)]
	[TestCase("ON LOCAL ERROR GOTO 350", true, OnErrorAction.GoToHandler, "350", null)]
	[TestCase("ON LOCAL ERROR GOTO ErrorHandler", true, OnErrorAction.GoToHandler, null, "ErrorHandler")]
	public void ShouldParse(string statement, bool expectedLocalOnly, OnErrorAction expectedAction, string? expectedTargetLineNumber, string? expectedTargetLabel)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<OnErrorStatement>();

		var onErrorResult = (OnErrorStatement)result;

		onErrorResult.LocalHandler.Should().Be(expectedLocalOnly);
		onErrorResult.Action.Should().Be(expectedAction);
		onErrorResult.TargetLineNumber.Should().Be(expectedTargetLineNumber);
		onErrorResult.TargetLabel.Should().Be(expectedTargetLabel);
	}
}
