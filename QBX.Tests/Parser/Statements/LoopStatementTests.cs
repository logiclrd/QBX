using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class LoopStatementTests
{
	[TestCase("LOOP", DoConditionType.None)]
	[TestCase("LOOP WHILE 1", DoConditionType.While)]
	[TestCase("LOOP UNTIL 0", DoConditionType.Until)]
	public void ShouldParse(string definition, DoConditionType conditionType)
	{
		// Arrange
		var tokens = new Lexer(definition).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ref inType);

		// Assert
		result.Should().BeOfType<LoopStatement>();

		var loopResult = (LoopStatement)result;

		loopResult.ConditionType.Should().Be(conditionType);

		if (conditionType == DoConditionType.None)
			loopResult.Expression.Should().BeNull();
		else
			loopResult.Expression.Should().NotBeNull();
	}
}
