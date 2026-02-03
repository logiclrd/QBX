using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class ClearStatementTests
{
	[TestCase("CLEAR", false, false, false)]
	[TestCase("CLEAR 1", true, false, false)]
	[TestCase("CLEAR , 2", false, true, false)]
	[TestCase("CLEAR , , 3", false, false, true)]
	[TestCase("CLEAR 1, 2", true, true, false)]
	[TestCase("CLEAR 1, , 3", true, false, true)]
	[TestCase("CLEAR , 2, 3", false, true, true)]
	[TestCase("CLEAR 1, 2, 3", true, true, true)]
	public void ShouldParse(string statement, bool arg1, bool arg2, bool arg3)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<ClearStatement>();

		var colorResult = (ClearStatement)result;

		if (arg1)
			colorResult.StringSpaceExpression.Should().NotBeNull();
		else
			colorResult.StringSpaceExpression.Should().BeNull();

		if (arg2)
			colorResult.MaximumMemoryAddressExpression.Should().NotBeNull();
		else
			colorResult.MaximumMemoryAddressExpression.Should().BeNull();

		if (arg3)
			colorResult.StackSpaceExpression.Should().NotBeNull();
		else
			colorResult.StackSpaceExpression.Should().BeNull();
	}

	[TestCase(false, false, false, "CLEAR")]
	[TestCase(true, false, false, "CLEAR 1")]
	[TestCase(false, true, false, "CLEAR , 2")]
	[TestCase(false, false, true, "CLEAR , , 3")]
	[TestCase(true, true, false, "CLEAR 1, 2")]
	[TestCase(true, false, true, "CLEAR 1, , 3")]
	[TestCase(false, true, true, "CLEAR , 2, 3")]
	[TestCase(true, true, true, "CLEAR 1, 2, 3")]
	public void ShouldFormat(bool setArg1, bool setArg2, bool setArg3, string expectedStatement)
	{
		// Arrange
		var sut = new ClearStatement();

		if (setArg1)
			sut.StringSpaceExpression = new LiteralExpression(new Token(Token.CreateDummyLine(), 0, TokenType.Number, "1"));
		if (setArg2)
			sut.MaximumMemoryAddressExpression = new LiteralExpression(new Token(Token.CreateDummyLine(), 0, TokenType.Number, "2"));
		if (setArg3)
			sut.StackSpaceExpression = new LiteralExpression(new Token(Token.CreateDummyLine(), 0, TokenType.Number, "3"));

		var buffer = new StringWriter();

		// Act
		sut.Render(buffer);

		// Assert
		buffer.ToString().Should().Be(expectedStatement);
	}
}
