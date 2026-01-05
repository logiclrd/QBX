using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class TypeStatementTests
{
	[Test]
	public void ShouldParse()
	{
		// Arrange
		const string TypeName = "foo";

		string text = $"TYPE {TypeName}";

		var tokens = new Lexer(text).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens);

		// Assert
		result.Should().BeOfType<TypeStatement>()
			.Which.Name.Should().Be(TypeName);
	}
}
