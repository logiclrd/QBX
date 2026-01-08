using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class GetStatementTests
{
	[TestCase("GET #1", typeof(LiteralExpression), "1", false, false)]
	[TestCase("GET filenumber%, 37&", typeof(IdentifierExpression), "filenumber%", true, false)]
	[TestCase("GET #fn%, , mystr$", typeof(IdentifierExpression), "fn%", false, true)]
	[TestCase("GET #3, baserecord& + rn&, array(i%).fieldname", typeof(LiteralExpression), "3", true, true)]
	public void ShouldParse(string statement, Type expectedFileNumberExpressionType, string expectedFileNumberExpressionTokenValue, bool expectRecordNumberExpression, bool expectTargetExpression)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<GetStatement>();

		var getResult = (GetStatement)result;

		getResult.FileNumberExpression.Should().BeOfType(expectedFileNumberExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedFileNumberExpressionTokenValue);

		if (expectRecordNumberExpression)
			getResult.RecordNumberExpression.Should().NotBeNull();
		else
			getResult.RecordNumberExpression.Should().BeNull();

		if (expectTargetExpression)
			getResult.TargetExpression.Should().BeAssignableTo<Expression>()
				.Which.IsValidAssignmentTarget().Should().BeTrue();
		else
			getResult.TargetExpression.Should().BeNull();
	}
}
