using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class PrintStatementTests
{
	[TestCase("PRINT", false, false, 0, PrintCursorAction.NextLine)]
	[TestCase("PRINT ;", false, false, 1, PrintCursorAction.None)]
	[TestCase("PRINT 3", false, false, 1, PrintCursorAction.NextLine)]
	[TestCase("PRINT 3;", false, false, 1, PrintCursorAction.None)]
	[TestCase("PRINT 1, 2", false, false, 2, PrintCursorAction.NextLine)]
	[TestCase("PRINT \"a\", \"b\",", false, false, 2, PrintCursorAction.NextZone)]
	[TestCase("PRINT #b%, \"the slithy toves\"", true, false, 1, PrintCursorAction.NextLine)]
	[TestCase("PRINT USING \"000\"; x%", false, true, 1, PrintCursorAction.NextLine)]
	[TestCase("PRINT #3, USING \"000\"; x%,", true, true, 1, PrintCursorAction.NextZone)]
	public void ShouldParse(string statement, bool expectFileNumberExpression, bool expectUsingExpression, int expectedArgumentCount, PrintCursorAction expectedFinalCursorAction)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ref inType);

		// Assert
		result.Should().BeOfType<PrintStatement>();

		var printResult = (PrintStatement)result;

		if (expectFileNumberExpression)
			printResult.FileNumberExpression.Should().NotBeNull();
		else
			printResult.FileNumberExpression.Should().BeNull();

		if (expectUsingExpression)
			printResult.UsingExpression.Should().NotBeNull();
		else
			printResult.UsingExpression.Should().BeNull();

		printResult.Arguments.Should().HaveCount(expectedArgumentCount);

		var finalCursorAction = PrintCursorAction.NextLine;

		if (printResult.Arguments.Any())
			finalCursorAction = printResult.Arguments.Last().CursorAction;

		finalCursorAction.Should().Be(expectedFinalCursorAction);
	}
}
