using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class GoSubStatementTests
{
	[TestCase("GOSUB 100", "100", null)]
	[TestCase("GOSUB OneHundred", null, "OneHundred")]
	public void ShouldParse(string statement, string? expectedLineNumber, string? expectedLabel)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, colonAfter: false, ref inType);

		// Assert
		result.Should().BeOfType<GoSubStatement>();

		var goSubResult = (GoSubStatement)result;

		goSubResult.TargetLineNumber.Should().Be(expectedLineNumber);
		goSubResult.TargetLabel.Should().Be(expectedLabel);
	}
}

/*
namespace QBX.Tests.Parser.Statements;

public class GoSubStatement : TargetLineStatement
{
	public override StatementType Type => StatementType.GoSub;

	protected override string StatementName => "GOSUB";
}

*/
