using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class RedimStatementTests
{
	[TestCase("REDIM SHARED arena(1 TO 50, 1 TO 80) AS arenaType",
		true, false,
		"arena",
		new int[] { 1, 1 },
		new int[] { 50, 80 },
		"arenaType",
		null,
		new int[0])]
	[TestCase("REDIM array%(5)",
		false, false,
		"array%",
		new int[] { 5 },
		new int[] { -1 },
		null,
		null,
		new int[0])]
	[TestCase("REDIM PRESERVE SHARED array%(6)",
		true, true,
		"array%",
		new int[] { 6 },
		new int[] { -1 },
		null,
		null,
		new int[0])]
	[TestCase("REDIM PRESERVE array%(7)",
		false, true,
		"array%",
		new int[] { 7 },
		new int[] { -1 },
		null,
		null,
		new int[0])]
	public void ShouldParse(string declaration, bool expectShared, bool expectPreserve, string variable1Name, int[] lowerBounds, int[] upperBounds, string? variable1Type, string? variable2Name, int[] upperBounds2)
	{
		// Arrange
		var tokens = new Lexer(declaration).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<RedimStatement>();

		var redimResult = (RedimStatement)result;

		redimResult.Shared.Should().Be(expectShared);
		redimResult.Preserve.Should().Be(expectPreserve);

		int expectedCount = (variable2Name == null) ? 1 : 2;

		redimResult.Declarations.Should().HaveCount(expectedCount);

		redimResult.Declarations[0].Name.Should().Be(variable1Name);

		if (lowerBounds.Length == 0)
			redimResult.Declarations[0].Subscripts.Should().BeNull();
		else
		{
			redimResult.Declarations[0].Subscripts.Should().NotBeNull();
			redimResult.Declarations[0].Subscripts!.Subscripts.Should().HaveCount(lowerBounds.Length);

			for (int i = 0; i < lowerBounds.Length; i++)
			{
				var subscript = redimResult.Declarations[0].Subscripts!.Subscripts[i];

				if (subscript == null)
					subscript.Should().NotBeNull();

				subscript.Bound1.Should().NotBeNull();
				subscript.Bound1!.Should().BeOfType<LiteralExpression>()
					.Which.Token!.Value.Should().Be(lowerBounds[i].ToString());

				if (upperBounds[i] < 0)
					subscript.Bound2.Should().BeNull();
				else
				{
					subscript.Bound2.Should().NotBeNull();
					subscript.Bound2!.Should().BeOfType<LiteralExpression>()
						.Which.Token!.Value.Should().Be(upperBounds[i].ToString());
				}
			}
		}

		if (variable1Type != null)
			redimResult.Declarations[0].UserType.Should().Be(variable1Type);

		if (variable2Name != null)
		{
			redimResult.Declarations[1].Name.Should().Be(variable2Name);

			if (upperBounds2.Length > 0)
			{
				redimResult.Declarations[1].Subscripts.Should().NotBeNull();
				redimResult.Declarations[1].Subscripts!.Subscripts.Should().HaveCount(upperBounds2.Length);

				for (int i = 0; i < upperBounds2.Length; i++)
				{
					var subscript = redimResult.Declarations[1].Subscripts!.Subscripts[i];

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
