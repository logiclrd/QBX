using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class PutStatementTests
{
	[TestCase("PUT #1", typeof(LiteralExpression), "1", false, false)]
	[TestCase("PUT filenumber%, 37&", typeof(IdentifierExpression), "filenumber%", true, false)]
	[TestCase("PUT #fn%, , mystr$", typeof(IdentifierExpression), "fn%", false, true)]
	[TestCase("PUT #3, baserecord& + rn&, array(i%).fieldname", typeof(LiteralExpression), "3", true, true)]
	public void ShouldParse(string statement, Type expectedFileNumberExpressionType, string expectedFileNumberExpressionTokenValue, bool expectRecordNumberExpression, bool expectTargetExpression)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens);

		// Assert
		result.Should().BeOfType<PutStatement>();

		var putResult = (PutStatement)result;

		putResult.FileNumberExpression.Should().BeOfType(expectedFileNumberExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedFileNumberExpressionTokenValue);

		if (expectRecordNumberExpression)
			putResult.RecordNumberExpression.Should().NotBeNull();
		else
			putResult.RecordNumberExpression.Should().BeNull();

		if (expectTargetExpression)
			putResult.TargetExpression.Should().BeAssignableTo<Expression>()
				.Which.IsValidAssignmentTarget().Should().BeTrue();
		else
			putResult.TargetExpression.Should().BeNull();
	}
}
