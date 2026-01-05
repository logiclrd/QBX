using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class LegacyOpenStatementTests
{
	[TestCase(
		"OPEN \"I\", #1, \"file\"",
		typeof(LiteralExpression), "\"I\"",
		typeof(LiteralExpression), "1",
		typeof(LiteralExpression), "\"file\"",
		null, "")]
	[TestCase(
		"OPEN \"O\", #fn%, \"filename\"",
		typeof(LiteralExpression), "\"O\"",
		typeof(IdentifierExpression), "fn%",
		typeof(LiteralExpression), "\"filename\"",
		null, "")]
	[TestCase(
		"OPEN mode$, #fn%, basename$ + \".TXT\"",
		typeof(IdentifierExpression), "mode$",
		typeof(IdentifierExpression), "fn%",
		typeof(BinaryExpression), "",
		null, "")]
	[TestCase(
		"OPEN mode$, #fn%, basename$ + \".TXT\", 128",
		typeof(IdentifierExpression), "mode$",
		typeof(IdentifierExpression), "fn%",
		typeof(BinaryExpression), "",
		typeof(LiteralExpression), "128")]
	public void ShouldParse(
		string statement,
		Type expectedModeExpressionType, string expectedModeExpressionValue,
		Type expectedFileNumberExpressionType, string expectedFileNumberExpressionValue,
		Type expectedFileNameExpressionType, string expectedFileNameExpressionValue,
		Type? expectedRecordLengthExpressionType, string expectedRecordLengthExpressionValue)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens);

		// Assert
		result.Should().BeOfType<LegacyOpenStatement>();

		var openResult = (LegacyOpenStatement)result;

		openResult.ModeExpression.Should().BeOfType(expectedModeExpressionType);
		openResult.ModeExpression.Token!.Value.Should().Be(expectedModeExpressionValue);

		openResult.FileNumberExpression.Should().BeOfType(expectedFileNumberExpressionType);
		openResult.FileNumberExpression.Token!.Value.Should().Be(expectedFileNumberExpressionValue);

		openResult.FileNameExpression.Should().BeOfType(expectedFileNameExpressionType);
		openResult.FileNameExpression.Token!.Value.Should().Be(expectedFileNameExpressionValue);

		if (expectedRecordLengthExpressionType == null)
			openResult.RecordLengthExpression.Should().BeNull();
		else
		{
			openResult.RecordLengthExpression.Should().BeOfType(expectedRecordLengthExpressionType);
			openResult.RecordLengthExpression.Token!.Value.Should().Be(expectedRecordLengthExpressionValue);
		}
	}
}
