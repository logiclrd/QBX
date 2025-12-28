using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class ForStatementTests
{
	[TestCase("FOR a = 1 TO 6", "1", "6", null)]
	[TestCase("FOR b& = 3 TO 9 STEP 2", "3", "9", "2")]
	public void ShouldParse(string definition, string from, string to, string? step)
	{
		// Arrange
		var tokens = new Lexer(definition).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ref inType);

		// Assert
		result.Should().BeOfType<ForStatement>();

		var forResult = (ForStatement)result;

		forResult.StartExpression.Should().BeOfType<LiteralExpression>()
			.Which.Token!.Value.Should().Be(from);
		forResult.EndExpression.Should().BeOfType<LiteralExpression>()
			.Which.Token!.Value.Should().Be(to);

		if (step == null)
			forResult.StepExpression.Should().BeNull();
		else
		{
			forResult.StepExpression.Should().BeOfType<LiteralExpression>()
				.Which.Token!.Value.Should().Be(step);
		}
	}
}
