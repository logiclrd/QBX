using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class CaseStatementTests
{
	public enum ExpressionType
	{
		Plain,
		Relation,
		Range,
	}

	[TestCase("CASE 1", new ExpressionType[] { ExpressionType.Plain })]
	[TestCase("CASE \"test\"", new ExpressionType[] { ExpressionType.Plain })]
	[TestCase("CASE NEXTLEVEL", new ExpressionType[] { ExpressionType.Plain })]
	[TestCase("CASE 2, a%, b% + c%", new ExpressionType[] { ExpressionType.Plain, ExpressionType.Plain, ExpressionType.Plain })]
	[TestCase("CASE 2 TO 4, IS > 9, -1", new ExpressionType[] { ExpressionType.Range, ExpressionType.Relation, ExpressionType.Plain })]
	public void ShouldParse(string statement, ExpressionType[] expectedExpressionTypes)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens);

		// Assert
		result.Should().BeOfType<CaseStatement>();

		var caseResult = (CaseStatement)result;

		caseResult.Expressions.Should().NotBeNull();
		caseResult.Expressions.Expressions.Should().HaveCount(expectedExpressionTypes.Length);
		caseResult.MatchElse.Should().Be(false);

		for (int i = 0; i < expectedExpressionTypes.Length; i++)
		{
			var expression = caseResult.Expressions.Expressions[i];

			switch (expectedExpressionTypes[i])
			{
				case ExpressionType.Plain:
					expression.RelationToExpression.Should().BeNull();
					expression.RangeEndExpression.Should().BeNull();
					break;
				case ExpressionType.Relation:
					expression.RelationToExpression.Should().NotBeNull();
					expression.RangeEndExpression.Should().BeNull();
					break;
				case ExpressionType.Range:
					expression.RelationToExpression.Should().BeNull();
					expression.RangeEndExpression.Should().NotBeNull();
					break;
			}
		}
	}

	public void ShouldParseElse()
	{
		// Arrange
		var tokens = new Lexer("CASE ELSE").ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens);

		// Assert
		result.Should().BeOfType<CaseStatement>();

		var caseResult = (CaseStatement)result;

		caseResult.Expressions.Should().BeNull();
		caseResult.MatchElse.Should().Be(true);
	}
}
