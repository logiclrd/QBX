using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class DoStatementTests
{
	[TestCase("DO", DoConditionType.None)]
	[TestCase("DO WHILE 1", DoConditionType.While)]
	[TestCase("DO UNTIL 0", DoConditionType.Until)]
	public void ShouldParse(string definition, DoConditionType conditionType)
	{
		// Arrange
		var tokens = new Lexer(definition).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<DoStatement>();

		var doResult = (DoStatement)result;

		doResult.ConditionType.Should().Be(conditionType);

		if (conditionType == DoConditionType.None)
			doResult.Expression.Should().BeNull();
		else
			doResult.Expression.Should().NotBeNull();
	}
}
