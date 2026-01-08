using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class DimStatementTests
{
	[TestCase("DIM SHARED arena(1 TO 50, 1 TO 80) AS arenaType",
		"arena",
		new int[] { 1, 1 },
		new int[] { 50, 80 },
		"arenaType",
		null,
		new int[0])]
	[TestCase("DIM curLevel, colorTable(10)",
		"curLevel",
		new int[0],
		new int[0],
		null,
		"colorTable",
		new int[] { 10 })]
	public void ShouldParse(string declaration, string variable1Name, int[] lowerBounds, int[] upperBounds, string? variable1Type, string? variable2Name, int[] upperBounds2)
	{
		// Arrange
		var tokens = new Lexer(declaration).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<DimStatement>();

		var dimResult = (DimStatement)result;

		int expectedCount = (variable2Name == null) ? 1 : 2;

		dimResult.Declarations.Should().HaveCount(expectedCount);

		dimResult.Declarations[0].Name.Should().Be(variable1Name);

		if (lowerBounds.Length == 0)
			dimResult.Declarations[0].Subscripts.Should().BeNull();
		else
		{
			dimResult.Declarations[0].Subscripts.Should().NotBeNull();
			dimResult.Declarations[0].Subscripts!.Subscripts.Should().HaveCount(lowerBounds.Length);

			for (int i = 0; i < lowerBounds.Length; i++)
			{
				var subscript = dimResult.Declarations[0].Subscripts!.Subscripts[i];

				if (subscript == null)
					subscript.Should().NotBeNull();

				subscript.Bound1.Should().NotBeNull();
				subscript.Bound2.Should().NotBeNull();

				subscript.Bound1!.Should().BeOfType<LiteralExpression>()
					.Which.Token!.Value.Should().Be(lowerBounds[i].ToString());
				subscript.Bound2!.Should().BeOfType<LiteralExpression>()
					.Which.Token!.Value.Should().Be(upperBounds[i].ToString());
			}
		}

		if (variable1Type != null)
			dimResult.Declarations[0].UserType.Should().Be(variable1Type);

		if (variable2Name != null)
		{
			dimResult.Declarations[1].Name.Should().Be(variable2Name);

			if (upperBounds2.Length > 0)
			{
				dimResult.Declarations[1].Subscripts.Should().NotBeNull();
				dimResult.Declarations[1].Subscripts!.Subscripts.Should().HaveCount(upperBounds2.Length);

				for (int i = 0; i < upperBounds2.Length; i++)
				{
					var subscript = dimResult.Declarations[1].Subscripts!.Subscripts[i];

					if (subscript == null)
						subscript.Should().NotBeNull();

					subscript.Bound1.Should().NotBeNull();
					subscript.Bound1!.Should().BeOfType<LiteralExpression>()
						.Which.Token!.Value.Should().Be(upperBounds2[i].ToString());
				}
			}
		}
	}
}
