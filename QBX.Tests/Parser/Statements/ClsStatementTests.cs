using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class ClsStatementTests
{
	[TestCase("CLS", false)]
	[TestCase("CLS 0", true)]
	[TestCase("CLS a% + b%", true)]
	public void ShouldParse(string statement, bool expectModeExpression)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, colonAfter: false, ref inType);

		// Assert
		result.Should().BeOfType<ClsStatement>();

		var clsResult = (ClsStatement)result;

		if (expectModeExpression)
			clsResult.Mode.Should().NotBeNull();
		else
			clsResult.Mode.Should().BeNull();
	}
}
