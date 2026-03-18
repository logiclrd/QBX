using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

using static QBX.Tests.Utility.IdentifierHelpers;

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

		var identifierRepository = new IdentifierRepository();

		var sut = new BasicParser(identifierRepository);

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<TypeStatement>()
			.Which.Name.Should().Be(ID(TypeName));
	}
}
