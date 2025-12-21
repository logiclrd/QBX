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

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, colonAfter: false, ref inType);

		// Assert
		result.Should().BeOfType<TypeStatement>()
			.Which.Name.Should().Be(TypeName);
	}
}
