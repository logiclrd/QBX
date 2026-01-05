using QBX.CodeModel;
using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class TypeElementStatementTests
{
	[TestCase("head AS INTEGER", "head", null, DataType.INTEGER, null)]
	[TestCase("length AS LONG", "length", null, DataType.LONG, null)]
	[TestCase("row AS SINGLE", "row", null, DataType.SINGLE, null)]
	[TestCase("col AS DOUBLE", "col", null, DataType.DOUBLE, null)]
	[TestCase("direction AS STRING", "direction", null, DataType.STRING, null)]
	[TestCase("lives AS CURRENCY", "lives", null, DataType.CURRENCY, null)]
	[TestCase("score AS othertypename", "score", null, DataType.Unspecified, "othertypename")]
	[TestCase("array(2) AS INTEGER", "array", new int[] { 2, -1 }, DataType.INTEGER, null)]
	[TestCase("array(1 TO 5, 3 TO 7, 5) AS INTEGER", "array", new int[] { 1, 5, 3, 7, 5, -1 }, DataType.INTEGER, null)]
	public void ShouldParse(string definition, string expectedName, int[]? expectedSubscripts, DataType expectedType, string? expectedUserType)
	{
		// Arrange
		var tokens = new Lexer(definition).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens);

		// Assert
		result.Should().BeOfType<TypeElementStatement>();

		var typeElementResult = (TypeElementStatement)result;

		typeElementResult.Name.Should().Be(expectedName);

		if (expectedSubscripts == null)
			typeElementResult.Subscripts.Should().BeNull();
		else
		{
			typeElementResult.Subscripts.Should().NotBeNull();
			typeElementResult.Subscripts.Subscripts.Should().HaveCount(expectedSubscripts.Length / 2);

			for (int i = 0; i < expectedSubscripts.Length; i += 2)
			{
				var subscript = typeElementResult.Subscripts.Subscripts[i / 2];

				subscript.Bound1.Should().BeOfType<LiteralExpression>();
				subscript.Bound1.Token!.Value.Should().Be(expectedSubscripts[i].ToString());

				if (expectedSubscripts[i + 1] < 0)
					subscript.Bound2.Should().BeNull();
				else
				{
					subscript.Bound2.Should().BeOfType<LiteralExpression>();
					subscript.Bound2.Token!.Value.Should().Be(expectedSubscripts[i + 1].ToString());
				}
			}
		}

		typeElementResult.ElementType.Should().Be(expectedType);
		typeElementResult.ElementUserType.Should().Be(expectedUserType);
	}
}
