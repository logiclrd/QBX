using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class SoundStatementTests
{
	[TestCase("SOUND 100, 5",
		typeof(LiteralExpression), "100",
		typeof(LiteralExpression), "5")]
	[TestCase("SOUND freqtable(note), 60 / tempo#",
		typeof(CallOrIndexExpression), "(",
		typeof(BinaryExpression), "/")]
	public void ShouldParse(
		string statement,
		Type expectedFrequencyExpressionType, string expectedFrequencyExpressionValue,
		Type expectedDurationExpressionType, string expectedDurationExpressionValue)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<SoundStatement>();

		var soundResult = (SoundStatement)result;

		soundResult.FrequencyExpression.Should().BeOfType(expectedFrequencyExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedFrequencyExpressionValue);
		soundResult.DurationExpression.Should().BeOfType(expectedDurationExpressionType)
			.And.BeAssignableTo<Expression>().Which.Token!.Value.Should().Be(expectedDurationExpressionValue);
	}
}
