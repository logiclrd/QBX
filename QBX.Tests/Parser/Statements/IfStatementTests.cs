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

		var identifierRepository = new IdentifierRepository();

		var sut = new BasicParser(identifierRepository);

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

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

		var identifierRepository = new IdentifierRepository();

		var sut = new BasicParser(identifierRepository);

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

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

		var identifierRepository = new IdentifierRepository();

		var sut = new BasicParser(identifierRepository);

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

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

		var identifierRepository = new IdentifierRepository();

		var sut = new BasicParser(identifierRepository);

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

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

		var identifierRepository = new IdentifierRepository();

		var sut = new BasicParser(identifierRepository);

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

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

		var identifierRepository = new IdentifierRepository();

		var sut = new BasicParser(identifierRepository);

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

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

	public void ShouldParseEmptyThenBody()
	{
		// Arrange
		string statement = "IF a = b THEN  ELSE PRINT \"spoon\"";

		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var identifierRepository = new IdentifierRepository();

		var sut = new BasicParser(identifierRepository);

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<IfStatement>();

		var ifResult = (IfStatement)result;

		ifResult.ConditionExpression.Should().BeOfType<BinaryExpression>()
			.Which.Operator.Should().Be(Operator.Equals);
		ifResult.ThenBody.Should().HaveCount(0);
		ifResult.ElseBody.Should().HaveCount(1);

		ifResult.ElseBody[0].Should().BeOfType<PrintStatement>();
	}

	public void ShouldParseEmptyThenBodyAndEmptyElseBody()
	{
		// Arrange
		string statement = "IF a = b THEN  ELSE";

		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var identifierRepository = new IdentifierRepository();

		var sut = new BasicParser(identifierRepository);

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<IfStatement>();

		var ifResult = (IfStatement)result;

		ifResult.ConditionExpression.Should().BeOfType<BinaryExpression>()
			.Which.Operator.Should().Be(Operator.Equals);
		ifResult.ThenBody.Should().HaveCount(0);
		ifResult.ElseBody.Should().HaveCount(0);
	}

	[Test]
	public void ShouldParseEmptyElseBody()
	{
		// Arrange
		string statement = "IF condition THEN PRINT 1 ELSE";

		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var identifierRepository = new IdentifierRepository();

		var sut = new BasicParser(identifierRepository);

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<IfStatement>();

		var ifStatement = (IfStatement)result;

		ifStatement.ConditionExpression.Should().BeOfType<IdentifierExpression>()
			.Which.Identifier.Value.Should().Be("condition");

		ifStatement.ThenBody.Should().HaveCount(1);

		var bodyStatement = ifStatement.ThenBody[0];

		bodyStatement.Should().BeOfType<PrintStatement>();

		ifStatement.ElseBody.Should().NotBeNull().And.HaveCount(0);
	}

	[Test]
	public void ShouldParseNestedSingleLineWithElseClauses()
	{
		// Arrange
		string statement = "IF condition THEN IF othercondition THEN 10 ELSE 20 ELSE 30";

		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var identifierRepository = new IdentifierRepository();

		var sut = new BasicParser(identifierRepository);

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<IfStatement>();

		var outerIf = (IfStatement)result;

		outerIf.ConditionExpression.Should().BeOfType<IdentifierExpression>()
			.Which.Identifier.Value.Should().Be("condition");

		outerIf.ThenBody.Should().HaveCount(1);

		var bodyStatement = outerIf.ThenBody[0];

		bodyStatement.Should().BeOfType<IfStatement>();

		var innerIf = (IfStatement)bodyStatement;

		innerIf.ConditionExpression.Should().BeOfType<IdentifierExpression>()
			.Which.Identifier.Value.Should().Be("othercondition");

		innerIf.ThenBody.Should().HaveCount(1);
		innerIf.ThenBody[0].Should().BeOfType<BareLineNumberGoToStatement>()
			.Which.TargetLineNumber.Should().BeOfType<Identifier>()
			.Which.Value.Should().Be("10");

		innerIf.ElseBody.Should().HaveCount(1);
		innerIf.ElseBody[0].Should().BeOfType<BareLineNumberGoToStatement>()
			.Which.TargetLineNumber.Should().BeOfType<Identifier>()
			.Which.Value.Should().Be("20");

		outerIf.ElseBody.Should().HaveCount(1);
		outerIf.ElseBody[0].Should().BeOfType<BareLineNumberGoToStatement>()
			.Which.TargetLineNumber.Should().BeOfType<Identifier>()
			.Which.Value.Should().Be("30");
	}

	[Test]
	public void ShouldParseNestedSingleLineNotFirstStatementWithElseClauseOnInner()
	{
		// Arrange
		string statement = "IF condition THEN PRINT \"hello\": IF othercondition THEN 10 ELSE 20";

		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var identifierRepository = new IdentifierRepository();

		var sut = new BasicParser(identifierRepository);

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<IfStatement>();

		var outerIf = (IfStatement)result;

		outerIf.ConditionExpression.Should().BeOfType<IdentifierExpression>()
			.Which.Identifier.Value.Should().Be("condition");

		outerIf.ThenBody.Should().HaveCount(2);

		var bodyStatement = outerIf.ThenBody[0];

		bodyStatement.Should().BeOfType<PrintStatement>();

		bodyStatement = outerIf.ThenBody[1];

		bodyStatement.Should().BeOfType<IfStatement>();

		var innerIf = (IfStatement)bodyStatement;

		innerIf.ConditionExpression.Should().BeOfType<IdentifierExpression>()
			.Which.Identifier.Value.Should().Be("othercondition");

		innerIf.ThenBody.Should().HaveCount(1);
		innerIf.ThenBody[0].Should().BeOfType<BareLineNumberGoToStatement>()
			.Which.TargetLineNumber.Should().BeOfType<Identifier>()
			.Which.Value.Should().Be("10");

		innerIf.ElseBody.Should().HaveCount(1);
		innerIf.ElseBody[0].Should().BeOfType<BareLineNumberGoToStatement>()
			.Which.TargetLineNumber.Should().BeOfType<Identifier>()
			.Which.Value.Should().Be("20");

		outerIf.ElseBody.Should().BeNull();
	}
}
