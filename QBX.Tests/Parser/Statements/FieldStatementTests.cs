using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class FieldStatementTests
{
	[TestCase("FIELD #1, 24 AS playername$",
		1, // count
		typeof(LiteralExpression), "1", // file number
		typeof(LiteralExpression), "24", // field 1 width
		typeof(IdentifierExpression), "playername$")] // field 1 target
	[TestCase("FIELD #fn%, f1len% AS arr(i%).field1, f2len% AS arr(i%).field2",
		2, // count
		typeof(IdentifierExpression), "fn%", // file number
		typeof(IdentifierExpression), "f1len%", // field 1 width
		typeof(BinaryExpression), "")] // field 1 target
	[TestCase("FIELD #a% + b%, 12 AS filename$, 7 AS filetime$, 10 AS filedate$, 1 AS filereadonly$",
		4, // count
		typeof(BinaryExpression), "", // file number
		typeof(LiteralExpression), "12", // field 1 width
		typeof(IdentifierExpression), "filename$")] // field 1 target
	public void ShouldParse(string statement,
		int expectedDefinitionCount,
		Type expectedFileNumberExpressionType, string expectedFileNumberExpressionTokenValue,
		Type expectedField1WidthExpressionType, string expectedField1WidthExpressionTokenValue,
		Type expectedField1TargetExpressionType, string expectedField1TargetExpressionTokenValue)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ref inType);

		// Assert
		result.Should().BeOfType<FieldStatement>();

		var fieldResult = (FieldStatement)result;

		fieldResult.FieldDefinitions.Should().HaveCount(expectedDefinitionCount);

		fieldResult.FileNumberExpression.Should().BeOfType(expectedFileNumberExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedFileNumberExpressionTokenValue);

		fieldResult.FieldDefinitions[0].FieldWidthExpression.Should().BeOfType(expectedField1WidthExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedField1WidthExpressionTokenValue);
		fieldResult.FieldDefinitions[0].TargetExpression.Should().BeOfType(expectedField1TargetExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedField1TargetExpressionTokenValue);
	}
}
