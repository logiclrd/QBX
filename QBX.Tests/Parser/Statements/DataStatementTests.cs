using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;
using QBX.Tests.Utility;

namespace QBX.Tests.Parser.Statements;

public class DataStatementTests
{
	[TestCase("DATA", "")]
	[TestCase("DATA 1, 2, 3", " 1, 2, 3")]
	[TestCase("DATA \"hello\", donkey, 42", " \"hello\", donkey, 42")]
	[TestCase("DATA          \"hello\"     ,      donkey    shrek\t\t\t, 42", "          \"hello\"     ,      donkey    shrek\t\t\t, 42")]
	[TestCase("DATA(", "(")]
	public void ShouldParse(string definition, string expectedRawString)
	{
		// Arrange
		expectedRawString = ("DATA" + expectedRawString).ExpandTabs().Substring(4);

		var tokens = new Lexer(definition).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var identifierRepository = new IdentifierRepository();

		var sut = new BasicParser(identifierRepository);

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<DataStatement>();

		var dataResult = (DataStatement)result;

		dataResult.RawString.Should().Be(expectedRawString);
	}
}
