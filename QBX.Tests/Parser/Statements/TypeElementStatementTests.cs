using QBX.CodeModel;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class TypeElementStatementTests
{
	[TestCase("head AS INTEGER", "head", DataType.INTEGER, null)]
	[TestCase("length AS LONG", "length", DataType.LONG, null)]
	[TestCase("row AS SINGLE", "row", DataType.SINGLE, null)]
	[TestCase("col AS DOUBLE", "col", DataType.DOUBLE, null)]
	[TestCase("direction AS STRING", "direction", DataType.STRING, null)]
	[TestCase("lives AS CURRENCY", "lives", DataType.CURRENCY, null)]
	[TestCase("score AS othertypename", "score", DataType.Unspecified, "othertypename")]
	public void ShouldParse(string definition, string name, DataType type, string? userType)
	{
		// Arrange
		var tokens = new Lexer(definition).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = true; // important context

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ref inType);

		// Assert
		result.Should().BeOfType<TypeElementStatement>();

		var typeElementResult = (TypeElementStatement)result;

		typeElementResult.Name.Should().Be(name);

		typeElementResult.ElementType.Should().Be(type);
		typeElementResult.ElementUserType.Should().Be(userType);
	}
}
