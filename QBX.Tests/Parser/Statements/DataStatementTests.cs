using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class DataStatementTests
{
	[TestCase("DATA", new string[0])]
	[TestCase("DATA 1, 2, 3", new string[] { "1", "2", "3" })]
	[TestCase("DATA \"hello\", donkey, 42", new string[] { "hello", "donkey", "42" })]
	[TestCase("DATA          \"hello\"     ,      donkey    shrek\t\t\t, 42", new string[] { "hello", "donkey    shrek", "42" })]
	[TestCase("DATA(", new string[] { "(" })]
	public void ShouldParse(string definition, string[] expectedDataItems)
	{
		// Arrange
		var tokens = new Lexer(definition).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<DataStatement>();

		var dataResult = (DataStatement)result;

		var dataItems = dataResult.ParseDataItems().ToList();

		dataItems.Should().Equal(expectedDataItems);
	}
}
