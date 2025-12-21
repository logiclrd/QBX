using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class AssignmentStatementTests
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
		var text = $"{targetVariableName} = 0";

		var tokens = new Lexer(text).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, colonAfter: false, ref inType);

		// Assert
		result.Should().BeOfType<AssignmentStatement>();

		var assignmentResult = (AssignmentStatement)result;

		assignmentResult.TargetExpression.Should().BeOfType<IdentifierExpression>()
			.Which.Identifier.Should().Be(targetVariableName);
	}

	[TestCase("3", 1)]
	[TestCase("3, 5", 2)]
	public void ShouldParseArrayElement(string subscripts, int subscriptCount)
	{
		// Arrange
		var tokens = new Lexer($"foo%({subscripts}) = 0").ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, colonAfter: false, ref inType);

		// Assert
		result.Should().BeOfType<AssignmentStatement>();

		var assignmentResult = (AssignmentStatement)result;

		assignmentResult.TargetExpression.Should().BeOfType<CallOrIndexExpression>();

		var indexResult = (CallOrIndexExpression)assignmentResult.TargetExpression;

		indexResult.Subject.Should().BeOfType<IdentifierExpression>()
			.Which.Token!.Value.Should().Be("foo%");
		indexResult.Arguments.Expressions.Should().HaveCount(subscriptCount);
	}

	[Test]
	public void ShouldParseFieldOfArrayElement()
	{
		// Arrange
		var code = "arena(row, col).realRow = INT((row + 1) / 2)";

		var tokens = new Lexer(code).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, colonAfter: false, ref inType);

		// Assert
		result.Should().BeOfType<AssignmentStatement>();

		var assignmentResult = (AssignmentStatement)result;

		assignmentResult.TargetExpression.Should().BeOfType<BinaryExpression>();

		var binaryResult = (BinaryExpression)assignmentResult.TargetExpression;

		binaryResult.Operator.Should().Be(Operator.Member);

		binaryResult.Left.Should().BeOfType<CallOrIndexExpression>();

		var indexResult = (CallOrIndexExpression)binaryResult.Left;

		indexResult.Subject.Should().BeOfType<IdentifierExpression>()
			.Which.Token!.Value.Should().Be("arena");
		indexResult.Arguments.Expressions.Should().HaveCount(2);
	}
}
