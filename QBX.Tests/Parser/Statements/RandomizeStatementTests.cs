using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class RandomizeStatementTests
{
	[TestCase("RANDOMIZE", false)]
	[TestCase("RANDOMIZE 0", true)]
	[TestCase("RANDOMIZE TIMER", true)]
	public void ShouldParse(string definition, bool shouldHaveExpression)
	{
		// Arrange
		var tokens = new Lexer(definition).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<RandomizeStatement>();

		var randomizeResult = (RandomizeStatement)result;

		if (shouldHaveExpression)
			randomizeResult.ArgumentExpression.Should().NotBeNull();
		else
			randomizeResult.ArgumentExpression.Should().BeNull();
	}
}

/*
using QBX.CodeModel.Expressions;

namespace QBX.Tests.Parser.Statements;

public class RandomizeStatement
{
	public override StatementType Type => StatementType.Randomize;

	public Expression? Expression { get; set; }

	public override void Render(TextWriter writer)
	{
		writer.Write("RANDOMIZE");

		if (Expression != null)
		{
			writer.Write(' ');
			Expression.Render(writer);
		}
	}
}

*/
