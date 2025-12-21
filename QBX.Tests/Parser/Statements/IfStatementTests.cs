using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class IfStatementTests
{
	[Test]
	public void ShouldParse()
	{
		// Arrange
		string statement = "IF monitor$ = \"M\" THEN";

		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, colonAfter: false, ref inType);

		// Assert
		result.Should().BeOfType<IfStatement>();

		var ifResult = (IfStatement)result;

		ifResult.ConditionExpression.Should().BeOfType<BinaryExpression>()
			.Which.Operator.Should().Be(Operator.Equals);
	}
}
