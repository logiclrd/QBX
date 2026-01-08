using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class ElseIfStatementTests
{
	[Test]
	public void ShouldParse()
	{
		// Arrange
		string statement = "ELSEIF monitor$ = \"M\" THEN";

		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<ElseIfStatement>();

		var ifResult = (ElseIfStatement)result;

		ifResult.ConditionExpression.Should().BeOfType<BinaryExpression>()
			.Which.Operator.Should().Be(Operator.Equals);
	}
}
