using QBX.CodeModel.Expressions;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser;

public class ExpressionTests
{
	ListRange<Token> Tokens(params (TokenType Type, string Value)[] tokens)
	{
		var list = new List<Token>();

		int line = 1;
		int column = 1;

		foreach (var tokenData in tokens)
		{
			Assume.That(tokenData.Value != "");

			var token = new Token(line, column, tokenData.Type, tokenData.Value);

			list.Add(token);

			column += token.Length;

			if (token.Type == TokenType.NewLine)
			{
				line++;
				column = 1;
			}
		}

		return list;
	}

	Token MakeEndToken(IEnumerable<Token> tokens)
		=> new Token(1, tokens.Max(tok => tok.Column + tok.Length), TokenType.Empty, "");

	[Test]
	public void NumericLiteral()
	{
		// Arrange
		ListRange<Token> tokens = Tokens(
			(TokenType.Number, "1"));

		var endToken = MakeEndToken(tokens);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseExpression(tokens, endToken);

		// Assert
		result.Should().BeOfType<LiteralExpression>()
			.Which.Token.Should().Be(tokens[0]);
	}

	[Test]
	public void StringLiteral()
	{
		// Arrange
		ListRange<Token> tokens = Tokens(
			(TokenType.String, "\"1\""));

		var endToken = MakeEndToken(tokens);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseExpression(tokens, endToken);

		// Assert
		result.Should().BeOfType<LiteralExpression>()
			.Which.Token.Should().Be(tokens[0]);
	}

	[Test]
	public void NegativeNumericLiteral()
	{
		// Arrange
		ListRange<Token> tokens = Tokens(
			(TokenType.Minus, "-"),
			(TokenType.Number, "1"));

		var endToken = MakeEndToken(tokens);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseExpression(tokens, endToken);

		// Assert
		result.Should().BeOfType<LiteralExpression>();

		var literalExpression = (LiteralExpression)result;

		literalExpression.Token.Should().NotBeNull();
		literalExpression.Token!.Value.Should().Be("-" + tokens[1].Value);
	}

	[Test]
	public void KeywordFunction()
	{
		// Arrange
		var tokens = Tokens(
			(TokenType.TIMER, "TIMER"));

		var endToken = MakeEndToken(tokens);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseExpression(tokens, endToken);

		// Assert
		result.Should().BeOfType<KeywordFunctionExpression>();

		var keywordFunction = (KeywordFunctionExpression)result;

		keywordFunction.Function.Should().Be(TokenType.TIMER);
		keywordFunction.Arguments.Should().BeNull();
	}

	[Test]
	public void KeywordFunctionWithParameters()
	{
		// Arrange
		var tokens = Tokens(
			(TokenType.SIN, "SIN"),
			(TokenType.OpenParenthesis, "("),
			(TokenType.Number, "0"),
			(TokenType.CloseParenthesis, ")"));

		var endToken = MakeEndToken(tokens);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseExpression(tokens, endToken);

		// Assert
		result.Should().BeOfType<KeywordFunctionExpression>();

		var keywordFunction = (KeywordFunctionExpression)result;

		keywordFunction.Function.Should().Be(TokenType.SIN);
		keywordFunction.Arguments.Should().NotBeNull();
		keywordFunction.Arguments.Expressions.Should().HaveCount(1);
	}

	[Test]
	public void CallOrIndex()
	{
		// Arrange
		const string TargetName = "foo";
		const string Argument1 = "1";
		const string Argument2 = "2";

		var tokens = Tokens(
			(TokenType.Identifier, TargetName),
			(TokenType.OpenParenthesis, "("),
			(TokenType.Number, Argument1),
			(TokenType.Comma, ","),
			(TokenType.Number, Argument2),
			(TokenType.CloseParenthesis, ")"));

		var endToken = MakeEndToken(tokens);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseExpression(tokens, endToken);

		// Assert
		result.Should().BeOfType<CallOrIndexExpression>();

		var callOrIndex = (CallOrIndexExpression)result;

		callOrIndex.Subject.Should().BeOfType<IdentifierExpression>()
			.Which.Token!.Value.Should().Be(TargetName);
		callOrIndex.Arguments.Expressions.Should().HaveCount(2);
		callOrIndex.Arguments.Expressions[0].Should().BeOfType<LiteralExpression>()
			.Which.Token!.Value.Should().Be(Argument1);
		callOrIndex.Arguments.Expressions[1].Should().BeOfType<LiteralExpression>()
			.Which.Token!.Value.Should().Be(Argument2);
	}

	[TestCase(TokenType.Plus, "+", Operator.Add)]
	[TestCase(TokenType.Minus, "-", Operator.Subtract)]
	[TestCase(TokenType.Asterisk, "*", Operator.Multiply)]
	[TestCase(TokenType.Slash, "/", Operator.Divide)]
	[TestCase(TokenType.Caret, "^", Operator.Exponentiate)]
	[TestCase(TokenType.Backslash, "\\", Operator.IntegerDivide)]
	[TestCase(TokenType.MOD, "MOD", Operator.Modulo)]
	[TestCase(TokenType.Equals, "=", Operator.Equals)]
	[TestCase(TokenType.NotEquals, "<>", Operator.NotEquals)]
	[TestCase(TokenType.LessThan, "<", Operator.LessThan)]
	[TestCase(TokenType.LessThanOrEquals, "<=", Operator.LessThanOrEquals)]
	[TestCase(TokenType.GreaterThan, ">", Operator.GreaterThan)]
	[TestCase(TokenType.GreaterThanOrEquals, ">=", Operator.GreaterThanOrEquals)]
	[TestCase(TokenType.AND, "AND", Operator.And)]
	[TestCase(TokenType.OR, "OR", Operator.Or)]
	[TestCase(TokenType.XOR, "XOR", Operator.ExclusiveOr)]
	[TestCase(TokenType.EQV, "EQV", Operator.Equivalent)]
	[TestCase(TokenType.IMP, "IMP", Operator.Implies)]
	public void BinaryExpressions(TokenType operatorTokenType, string operatorSourceText, Operator expectedOperator)
	{
		// Arrange
		var tokens = Tokens(
			(TokenType.Number, "0"),
			(operatorTokenType, operatorSourceText),
			(TokenType.Number, "1"));

		var endToken = MakeEndToken(tokens);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseExpression(tokens, endToken);

		// Assert
		result.Should().BeOfType<BinaryExpression>()
			.Which.Operator.Should().Be(expectedOperator);
	}

	[TestCase(TokenType.Minus, "-", Operator.Negate)]
	[TestCase(TokenType.NOT, "NOT", Operator.Not)]
	public void UnaryExpressions(TokenType operatorTokenType, string operatorSourceText, Operator expectedOperator)
	{
		// Arrange
		var tokens = Tokens(
			(operatorTokenType, operatorSourceText),
			(TokenType.Identifier, "a"));

		var endToken = MakeEndToken(tokens);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseExpression(tokens, endToken);

		// Assert
		result.Should().BeOfType<UnaryExpression>()
			.Which.Operator.Should().Be(expectedOperator);
	}

	[Test]
	public void ElidePositiveUnaryExpressions()
	{
		// Arrange
		var tokens = Tokens(
			(TokenType.Plus, "+"),
			(TokenType.Identifier, "a"));

		var endToken = MakeEndToken(tokens);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseExpression(tokens, endToken);

		// Assert
		result.Should().BeOfType<IdentifierExpression>()
			.Which.Token!.Value.Should().Be("a");
	}

	[Test]
	public void Parenthesized()
	{
		// Arrange
		var tokens = Tokens(
			(TokenType.OpenParenthesis, "("),
			(TokenType.Number, "0"),
			(TokenType.CloseParenthesis, ")"));

		var endToken = MakeEndToken(tokens);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseExpression(tokens, endToken);

		// Assert
		result.Should().BeOfType<ParenthesizedExpression>()
			.Which.Child.Should().BeOfType<LiteralExpression>();
	}

	[Test]
	public void OrderOfRepeatedOperations()
	{
		// Arrange
		var tokens = Tokens(
			(TokenType.Number, "1"),
			(TokenType.Minus, "-"),
			(TokenType.Number, "2"),
			(TokenType.Minus, "-"),
			(TokenType.Number, "3"));

		var endToken = MakeEndToken(tokens);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseExpression(tokens, endToken);

		// Assert
		result.Should().BeOfType<BinaryExpression>()
			.Which.Left.Should().BeOfType<BinaryExpression>();

		var second = (BinaryExpression)result;
		var first = (BinaryExpression)second.Left;

		first.Left.Should().BeOfType<LiteralExpression>()
			.Which.Token!.Value.Should().Be("1");
		first.Right.Should().BeOfType<LiteralExpression>()
			.Which.Token!.Value.Should().Be("2");
		second.Right.Should().BeOfType<LiteralExpression>()
			.Which.Token!.Value.Should().Be("3");
	}

	[TestCase(TokenType.Asterisk, "*", TokenType.Plus, "+", Operator.Multiply, Operator.Add)]
	[TestCase(TokenType.Plus, "+", TokenType.Asterisk, "*", Operator.Multiply, Operator.Add)]
	[TestCase(TokenType.Asterisk, "*", TokenType.Minus, "-", Operator.Multiply, Operator.Subtract)]
	[TestCase(TokenType.Minus, "-", TokenType.Asterisk, "*", Operator.Multiply, Operator.Subtract)]
	[TestCase(TokenType.Slash, "/", TokenType.Minus, "-", Operator.Divide, Operator.Subtract)]
	[TestCase(TokenType.Minus, "-", TokenType.Slash, "/", Operator.Divide, Operator.Subtract)]
	[TestCase(TokenType.Caret, "^", TokenType.Minus, "-", Operator.Exponentiate, Operator.Subtract)]
	[TestCase(TokenType.Minus, "-", TokenType.Caret, "^", Operator.Exponentiate, Operator.Subtract)]
	[TestCase(TokenType.Caret, "^", TokenType.Slash, "/", Operator.Exponentiate, Operator.Divide)]
	[TestCase(TokenType.Slash, "/", TokenType.Caret, "^", Operator.Exponentiate, Operator.Divide)]
	public void Precedence(TokenType leftOperator, string leftOperatorSourceText, TokenType rightOperator, string rightOperatorSourceText,Operator expectedFirstOperator, Operator expectedSecondOperator)
	{
		// Arrange
		var tokens = Tokens(
			(TokenType.Number, "1"),
			(leftOperator, leftOperatorSourceText),
			(TokenType.Number, "2"),
			(rightOperator, rightOperatorSourceText),
			(TokenType.Number, "3"));

		var endToken = MakeEndToken(tokens);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseExpression(tokens, endToken);

		// Assert
		result.Should().BeOfType<BinaryExpression>();

		var second = (BinaryExpression)result;

		var first =
			(second.Left as BinaryExpression) ??
			(second.Right as BinaryExpression);

		first.Should().NotBeNull();

		first.Operator.Should().Be(expectedFirstOperator);
		second.Operator.Should().Be(expectedSecondOperator);
	}

	[Test]
	public void Member()
	{
		// Arrange
		var tokens = Tokens(
			(TokenType.Identifier, "struct"),
			(TokenType.Period, "."),
			(TokenType.Identifier, "field"));

		var endToken = MakeEndToken(tokens);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseExpression(tokens, endToken);

		// Assert
		result.Should().BeOfType<BinaryExpression>();

		var binaryResult = (BinaryExpression)result;

		binaryResult.Operator.Should().Be(Operator.Field);

		binaryResult.Left.Should().BeOfType<IdentifierExpression>()
			.Which.Identifier.Should().Be("struct");
		binaryResult.Right.Should().BeOfType<IdentifierExpression>()
			.Which.Identifier.Should().Be("field");
	}

	[Test]
	public void MemberOfMember()
	{
		// Arrange
		var tokens = Tokens(
			(TokenType.Identifier, "struct"),
			(TokenType.Period, "."),
			(TokenType.Identifier, "field"),
			(TokenType.Period, "."),
			(TokenType.Identifier, "subfield"));

		var endToken = MakeEndToken(tokens);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseExpression(tokens, endToken);

		// Assert
		result.Should().BeOfType<BinaryExpression>();

		var binaryResult = (BinaryExpression)result;

		binaryResult.Operator.Should().Be(Operator.Field);

		binaryResult.Left.Should().BeOfType<BinaryExpression>();

		var subBinaryResult = (BinaryExpression)binaryResult.Left;

		subBinaryResult.Operator.Should().Be(Operator.Field);

		subBinaryResult.Left.Should().BeOfType<IdentifierExpression>()
			.Which.Identifier.Should().Be("struct");
		subBinaryResult.Right.Should().BeOfType<IdentifierExpression>()
			.Which.Identifier.Should().Be("field");
		binaryResult.Right.Should().BeOfType<IdentifierExpression>()
			.Which.Identifier.Should().Be("subfield");
	}

	[Test]
	public void IndexMember()
	{
		// Arrange
		var tokens = Tokens(
			(TokenType.Identifier, "struct"),
			(TokenType.Period, "."),
			(TokenType.Identifier, "field"),
			(TokenType.OpenParenthesis, "("),
			(TokenType.Number, "1"),
			(TokenType.CloseParenthesis, ")"));

		var endToken = MakeEndToken(tokens);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseExpression(tokens, endToken);

		// Assert
		result.Should().BeOfType<CallOrIndexExpression>();

		var indexResult = (CallOrIndexExpression)result;

		indexResult.Arguments.Expressions.Should().HaveCount(1);
		indexResult.Subject.Should().BeOfType<BinaryExpression>();

		var binaryResult = (BinaryExpression)indexResult.Subject;

		binaryResult.Operator.Should().Be(Operator.Field);
	}

	[Test]
	public void MemberOfArrayElement()
	{
		// Arrange
		var tokens = Tokens(
			(TokenType.Identifier, "array"),
			(TokenType.OpenParenthesis, "("),
			(TokenType.Number, "1"),
			(TokenType.CloseParenthesis, ")"),
			(TokenType.Period, "."),
			(TokenType.Identifier, "field"));

		var endToken = MakeEndToken(tokens);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseExpression(tokens, endToken);

		// Assert
		result.Should().BeOfType<BinaryExpression>();

		var binaryResult = (BinaryExpression)result;

		binaryResult.Operator.Should().Be(Operator.Field);

		binaryResult.Left.Should().BeOfType<CallOrIndexExpression>();

		var indexResult = (CallOrIndexExpression)binaryResult.Left;

		indexResult.Arguments.Expressions.Should().HaveCount(1);
		indexResult.Subject.Should().BeOfType<IdentifierExpression>()
			.Which.Token!.Value.Should().Be("array");

		binaryResult.Right.Should().BeOfType<IdentifierExpression>()
			.Which.Token!.Value.Should().Be("field");
	}

	[Test]
	public void MemberPrecedence()
	{
		// Arrange
		var tokens = Tokens(
			(TokenType.Identifier, "struct"),
			(TokenType.Period, "."),
			(TokenType.Identifier, "field"),
			(TokenType.Caret, "^"),
			(TokenType.Number, "2"));

		var endToken = MakeEndToken(tokens);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseExpression(tokens, endToken);

		// Assert
		result.Should().BeOfType<BinaryExpression>();

		var binaryResult = (BinaryExpression)result;

		binaryResult.Operator.Should().Be(Operator.Exponentiate);

		binaryResult.Left.Should().BeOfType<BinaryExpression>();

		var subBinaryResult = (BinaryExpression)binaryResult.Left;

		subBinaryResult.Operator.Should().Be(Operator.Field);

		subBinaryResult.Left.Should().BeOfType<IdentifierExpression>()
			.Which.Identifier.Should().Be("struct");
		subBinaryResult.Right.Should().BeOfType<IdentifierExpression>()
			.Which.Identifier.Should().Be("field");
		binaryResult.Right.Should().BeOfType<LiteralExpression>()
			.Which.Token!.Value.Should().Be("2");
	}
}
