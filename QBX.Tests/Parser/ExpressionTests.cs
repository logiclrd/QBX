using QBX.CodeModel.Expressions;
using QBX.LexicalAnalysis;
using QBX.Parser;
using System;
using System.Collections.Generic;
using System.Text;

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
			(TokenType.Minus, ""),
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
			(TokenType.TIMER, ""));

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
			(TokenType.SIN, ""),
			(TokenType.OpenParenthesis, ""),
			(TokenType.Number, "0"),
			(TokenType.CloseParenthesis, ""));

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
			(TokenType.OpenParenthesis, ""),
			(TokenType.Number, Argument1),
			(TokenType.Comma, ""),
			(TokenType.Number, Argument2),
			(TokenType.CloseParenthesis, ""));

		var endToken = MakeEndToken(tokens);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseExpression(tokens, endToken);

		// Assert
		result.Should().BeOfType<CallOrIndexExpression>();

		var callOrIndex = (CallOrIndexExpression)result;

		callOrIndex.Name.Should().Be(TargetName);
		callOrIndex.Arguments.Expressions.Should().HaveCount(2);
		callOrIndex.Arguments.Expressions[0].Should().BeOfType<LiteralExpression>()
			.Which.Token!.Value.Should().Be(Argument1);
		callOrIndex.Arguments.Expressions[1].Should().BeOfType<LiteralExpression>()
			.Which.Token!.Value.Should().Be(Argument2);
	}

	[TestCase(TokenType.Plus, Operator.Add)]
	[TestCase(TokenType.Minus, Operator.Subtract)]
	[TestCase(TokenType.Asterisk, Operator.Multiply)]
	[TestCase(TokenType.Slash, Operator.Divide)]
	[TestCase(TokenType.Caret, Operator.Exponentiate)]
	[TestCase(TokenType.Backslash,  Operator.IntegerDivide)]
	[TestCase(TokenType.MOD, Operator.Modulo)]
	[TestCase(TokenType.Equals, Operator.Equals)]
	[TestCase(TokenType.NotEquals,  Operator.NotEquals)]
	[TestCase(TokenType.LessThan, Operator.LessThan)]
	[TestCase(TokenType.LessThanOrEquals, Operator.LessThanOrEquals)]
	[TestCase(TokenType.GreaterThan, Operator.GreaterThan)]
	[TestCase(TokenType.GreaterThanOrEquals, Operator.GreaterThanOrEquals)]
	[TestCase(TokenType.AND, Operator.And)]
	[TestCase(TokenType.OR, Operator.Or)]
	[TestCase(TokenType.XOR, Operator.ExclusiveOr)]
	[TestCase(TokenType.EQV, Operator.Equivalent)]
	[TestCase(TokenType.IMP, Operator.Implies)]
	public void BinaryExpressions(TokenType operatorTokenType, Operator expectedOperator)
	{
		// Arrange
		var tokens = Tokens(
			(TokenType.Number, "0"),
			(operatorTokenType, ""),
			(TokenType.Number, "1"));

		var endToken = MakeEndToken(tokens);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseExpression(tokens, endToken);

		// Assert
		result.Should().BeOfType<BinaryExpression>()
			.Which.Operator.Should().Be(expectedOperator);
	}

	public void UnaryExpressions(TokenType operatorTokenType, Operator expectedOperator)
	{
		// Arrange
		var tokens = Tokens(
			(operatorTokenType, ""),
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
	public void Parenthesized()
	{
		// Arrange
		var tokens = Tokens(
			(TokenType.OpenParenthesis, ""),
			(TokenType.Number, "0"),
			(TokenType.CloseParenthesis, ""));

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
			(TokenType.Minus, ""),
			(TokenType.Number, "2"),
			(TokenType.Minus, ""),
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

	[TestCase(TokenType.Asterisk, TokenType.Plus, Operator.Multiply, Operator.Add)]
	[TestCase(TokenType.Plus, TokenType.Asterisk, Operator.Multiply, Operator.Add)]
	[TestCase(TokenType.Asterisk, TokenType.Minus, Operator.Multiply, Operator.Subtract)]
	[TestCase(TokenType.Minus, TokenType.Asterisk, Operator.Multiply, Operator.Subtract)]
	[TestCase(TokenType.Slash, TokenType.Minus, Operator.Divide, Operator.Subtract)]
	[TestCase(TokenType.Minus, TokenType.Slash, Operator.Divide, Operator.Subtract)]
	[TestCase(TokenType.Caret, TokenType.Minus, Operator.Exponentiate, Operator.Subtract)]
	[TestCase(TokenType.Minus, TokenType.Caret, Operator.Exponentiate, Operator.Subtract)]
	[TestCase(TokenType.Caret, TokenType.Slash, Operator.Exponentiate, Operator.Divide)]
	[TestCase(TokenType.Slash, TokenType.Caret, Operator.Exponentiate, Operator.Divide)]
	public void Precedence(TokenType leftOperator, TokenType rightOperator, Operator expectedFirstOperator, Operator expectedSecondOperator)
	{
		// Arrange
		var tokens = Tokens(
			(TokenType.Number, "1"),
			(leftOperator, ""),
			(TokenType.Number, "2"),
			(rightOperator, ""),
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
}
