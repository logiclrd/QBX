using QBX.CodeModel;
using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class DefFnStatementTests
{
	[Test]
	public void ShouldParseBlockStart()
	{
		// Arrange
		string statement = "DEF FNspoon";

		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<DefFnStatement>();

		var defFnResult = (DefFnStatement)result;

		defFnResult.Parameters.Should().BeNull();
		defFnResult.ExpressionBody.Should().BeNull();
	}

	[Test]
	public void ShouldParseBlockStartWithParameters()
	{
		// Arrange
		string statement = "DEF FNfork(TineLength!, TineCount AS INTEGER)";

		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<DefFnStatement>();

		var defFnResult = (DefFnStatement)result;

		defFnResult.ExpressionBody.Should().BeNull();

		defFnResult.Parameters.Should().NotBeNull();
		defFnResult.Parameters.Parameters.Should().HaveCount(2);
		defFnResult.Parameters.Parameters[0].Name.Should().Be("TineLength!");
		defFnResult.Parameters.Parameters[0].Type.Should().Be(DataType.Unspecified);
		defFnResult.Parameters.Parameters[0].IsArray.Should().BeFalse();
		defFnResult.Parameters.Parameters[1].Name.Should().Be("TineCount");
		defFnResult.Parameters.Parameters[1].Type.Should().Be(DataType.INTEGER);
		defFnResult.Parameters.Parameters[1].IsArray.Should().BeFalse();
	}

	[Test]
	public void ShouldParseInline()
	{
		// Arrange
		string statement = "DEF FnRan = INT(RND(1) * 100) + 1";

		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<DefFnStatement>();

		var defFnResult = (DefFnStatement)result;

		defFnResult.Parameters.Should().BeNull();

		defFnResult.ExpressionBody.Should().BeOfType<BinaryExpression>()
			.Which.Left.Should().BeOfType<KeywordFunctionExpression>()
			.Which.Function.Should().Be(TokenType.INT);
	}

	[Test]
	public void ShouldParseInlineWithParameters()
	{
		// Arrange
		string statement = "DEF FnRan (x) = INT(RND(1) * x) + 1";

		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<DefFnStatement>();

		var defFnResult = (DefFnStatement)result;

		defFnResult.Parameters.Should().NotBeNull();
		defFnResult.Parameters.Parameters.Should().HaveCount(1);
		defFnResult.Parameters.Parameters[0].Name.Should().Be("x");
		defFnResult.Parameters.Parameters[0].Type.Should().Be(DataType.Unspecified);
		defFnResult.Parameters.Parameters[0].IsArray.Should().BeFalse();

		defFnResult.ExpressionBody.Should().BeOfType<BinaryExpression>()
			.Which.Left.Should().BeOfType<KeywordFunctionExpression>()
			.Which.Function.Should().Be(TokenType.INT);
	}
}
