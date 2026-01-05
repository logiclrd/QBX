using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class IfStatementTests
{
	[Test]
	public void ShouldParseSimple()
	{
		// Arrange
		string statement = "IF monitor$ = \"M\" THEN";

		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens);

		// Assert
		result.Should().BeOfType<IfStatement>();

		var ifResult = (IfStatement)result;

		ifResult.ConditionExpression.Should().BeOfType<BinaryExpression>()
			.Which.Operator.Should().Be(Operator.Equals);
		ifResult.ThenBody.Should().BeNull();
		ifResult.ElseBody.Should().BeNull();
	}

	[Test]
	public void ShouldParseThenBody()
	{
		// Arrange
		string statement = "IF monitor$ = \"M\" THEN PRINT 2";

		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens);

		// Assert
		result.Should().BeOfType<IfStatement>();

		var ifResult = (IfStatement)result;

		ifResult.ConditionExpression.Should().BeOfType<BinaryExpression>()
			.Which.Operator.Should().Be(Operator.Equals);
		ifResult.ThenBody.Should().HaveCount(1);
		ifResult.ElseBody.Should().BeNull();
	}

	[Test]
	public void ShouldParseThenAndElseBodies()
	{
		// Arrange
		string statement = "IF monitor$ = \"M\" THEN PRINT 2 ELSE PRINT 3";

		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens);

		// Assert
		result.Should().BeOfType<IfStatement>();

		var ifResult = (IfStatement)result;

		ifResult.ConditionExpression.Should().BeOfType<BinaryExpression>()
			.Which.Operator.Should().Be(Operator.Equals);
		ifResult.ThenBody.Should().HaveCount(1);
		ifResult.ElseBody.Should().HaveCount(1);
	}

	[Test]
	public void ShouldParseCompoundThenAndElseBodies()
	{
		// Arrange
		string statement = "IF monitor$ = \"M\" THEN PRINT 2: a = 0: GOTO retry ELSE PRINT 3: END";

		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens);

		// Assert
		result.Should().BeOfType<IfStatement>();

		var ifResult = (IfStatement)result;

		ifResult.ConditionExpression.Should().BeOfType<BinaryExpression>()
			.Which.Operator.Should().Be(Operator.Equals);
		ifResult.ThenBody.Should().HaveCount(3);
		ifResult.ElseBody.Should().HaveCount(2);
	}

	[Test]
	public void ShouldParseNestedIfInThenBody()
	{
		// Arrange
		string statement = "IF monitor$ = \"M\" THEN IF snake$ = \"yes\" THEN PRINT 2: a = 0: GOTO retry ELSE PRINT 3: END";

		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens);

		// Assert
		result.Should().BeOfType<IfStatement>();

		var ifResult = (IfStatement)result;

		ifResult.ConditionExpression.Should().BeOfType<BinaryExpression>()
			.Which.Operator.Should().Be(Operator.Equals);
		ifResult.ThenBody.Should().HaveCount(1);
		ifResult.ElseBody.Should().BeNull();

		ifResult.ThenBody[0].Should().BeOfType<IfStatement>();

		var nestedIfResult = (IfStatement)ifResult.ThenBody[0];

		nestedIfResult.ThenBody.Should().HaveCount(3);
		nestedIfResult.ElseBody.Should().HaveCount(2);
	}

	[Test]
	public void ShouldParseNestedIfInElseBody()
	{
		// Arrange
		string statement = "IF monitor$ = \"M\" THEN PRINT 2: a = 0: GOTO retry ELSE IF snake$ = \"yes\" THEN PRINT 3: END";

		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens);

		// Assert
		result.Should().BeOfType<IfStatement>();

		var ifResult = (IfStatement)result;

		ifResult.ConditionExpression.Should().BeOfType<BinaryExpression>()
			.Which.Operator.Should().Be(Operator.Equals);
		ifResult.ThenBody.Should().HaveCount(3);
		ifResult.ElseBody.Should().HaveCount(1);

		ifResult.ElseBody[0].Should().BeOfType<IfStatement>();

		var nestedIfResult = (IfStatement)ifResult.ElseBody[0];

		nestedIfResult.ThenBody.Should().HaveCount(2);
		nestedIfResult.ElseBody.Should().BeNull();
	}
}
