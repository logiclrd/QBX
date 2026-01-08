using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class LetStatementTests
{
	[TestCase("foo")]
	[TestCase("foo%")]
	[TestCase("foo&")]
	[TestCase("foo!")]
	[TestCase("foo#")]
	[TestCase("foo$")]
	[TestCase("foo@")]
	public void ShouldParse(string targetVariableName)
	{
		// Arrange
		var text = $"LET {targetVariableName} = 0";

		var tokens = new Lexer(text).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<LetStatement>();

		var letResult = (LetStatement)result;

		letResult.TargetExpression.Should().BeOfType<IdentifierExpression>()
			.Which.Identifier.Should().Be(targetVariableName);
	}

	[TestCase("3", 1)]
	[TestCase("3, 5", 2)]
	public void ShouldParseArrayElement(string subscripts, int subscriptCount)
	{
		// Arrange
		var tokens = new Lexer($"LET foo%({subscripts}) = 0").ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<LetStatement>();

		var letResult = (LetStatement)result;

		letResult.TargetExpression.Should().BeOfType<CallOrIndexExpression>();

		var indexResult = (CallOrIndexExpression)letResult.TargetExpression;

		indexResult.Subject.Should().BeOfType<IdentifierExpression>()
			.Which.Token!.Value.Should().Be("foo%");
		indexResult.Arguments.Expressions.Should().HaveCount(subscriptCount);
	}

	[Test]
	public void ShouldParseFieldOfArrayElement()
	{
		// Arrange
		var code = "LET arena(row, col).realRow = INT((row + 1) / 2)";

		var tokens = new Lexer(code).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<LetStatement>();

		var letResult = (LetStatement)result;

		letResult.TargetExpression.Should().BeOfType<BinaryExpression>();

		var binaryResult = (BinaryExpression)letResult.TargetExpression;

		binaryResult.Operator.Should().Be(Operator.Field);

		binaryResult.Left.Should().BeOfType<CallOrIndexExpression>();

		var indexResult = (CallOrIndexExpression)binaryResult.Left;

		indexResult.Subject.Should().BeOfType<IdentifierExpression>()
			.Which.Token!.Value.Should().Be("arena");
		indexResult.Arguments.Expressions.Should().HaveCount(2);
	}
}
