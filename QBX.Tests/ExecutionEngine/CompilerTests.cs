using QBX.CodeModel.Expressions;
using QBX.ExecutionEngine;
using QBX.ExecutionEngine.Compiled;
using QBX.LexicalAnalysis;

namespace QBX.Tests.ExecutionEngine;

public class CompilerTests
{
	static IEnumerable<object?[]> GenerateCollapseDottedIdentifierExpressionTestCases()
	{
		int column = 0;

		IdentifierExpression Identifier(string identifier)
		{
			var ret = new IdentifierExpression(new Token(0, column, TokenType.Identifier, identifier));

			column += ret.Token!.Length;

			return ret;
		}

		Token CharacterToken(char ch)
		{
			var ret = Token.ForCharacter(0, column, ch);

			column++;

			return ret;
		}

		column = 10;
		yield return
			new object?[]
			{
				null,
				new BinaryExpression(
					Identifier("a"),
					CharacterToken('*'),
					Identifier("b")),
				null, 0 // "a*b"
			};

		column = 10;
		yield return
			new object?[]
			{
				null,
				new BinaryExpression(
					Identifier("a"),
					CharacterToken('*'),
					new BinaryExpression(
						Identifier("b"),
						CharacterToken('+'),
						Identifier("c"))),
				null, 0 // "a.b+c"
			};

		column = 10;
		yield return
			new object?[]
			{
				null,
				new BinaryExpression(
					Identifier("a"),
					CharacterToken('*'),
					new BinaryExpression(
						Identifier("b"),
						CharacterToken('.'),
						Identifier("c"))),
				null, 0 // "a.b.c" but down the wrong side
			};

		column = 10;
		yield return
			new object?[]
			{
				null,
				new BinaryExpression(
					Identifier("a"),
					CharacterToken('.'),
					Identifier("b")),
				"a.b", 10
			};

		column = 10;
		yield return
			new object?[]
			{
				"b",
				new BinaryExpression(
					Identifier("a"),
					CharacterToken('.'),
					Identifier("b")),
				"a.b", 10
			};

		column = 10;
		yield return
			new object?[]
			{
				"a", // disallowed
				new BinaryExpression(
					Identifier("a"),
					CharacterToken('.'),
					Identifier("b")),
				null, 0
			};

		column = 10;
		yield return
			new object?[]
			{
				null,
				new BinaryExpression(
					new BinaryExpression(
						Identifier("a"),
						CharacterToken('+'),
						Identifier("c")),
					CharacterToken('.'),
					Identifier("b")),
				null, 0 // "a+c.b"
			};

		column = 10;
		yield return
			new object?[]
			{
				null,
				new BinaryExpression(
					new BinaryExpression(
						Identifier("a"),
						CharacterToken('+'),
						Identifier("c")),
					CharacterToken('.'),
					Identifier("b")),
				null, 0 // "a.c.b"
			};

		column = 10;
		yield return
			new object?[]
			{
				null,
				new BinaryExpression(
					new BinaryExpression(
						Identifier("a"),
						CharacterToken('.'),
						Identifier("c")),
					CharacterToken('.'),
					Identifier("b")),
				"a.c.b", 10
			};

		column = 10;
		yield return
			new object?[]
			{
				"b",
				new BinaryExpression(
					new BinaryExpression(
						Identifier("a"),
						CharacterToken('.'),
						Identifier("c")),
					CharacterToken('.'),
					Identifier("b")),
				"a.c.b", 10
			};

		column = 10;
		yield return
			new object?[]
			{
				"a", // disallowed
				new BinaryExpression(
					new BinaryExpression(
						Identifier("a"),
						CharacterToken('.'),
						Identifier("c")),
					CharacterToken('.'),
					Identifier("b")),
				null, 0
			};

		column = 10;
		yield return
			new object?[]
			{
				null,
				new BinaryExpression(
					new BinaryExpression(
						Identifier("a"),
						CharacterToken('.'),
						new BinaryExpression(
							Identifier("c"),
							CharacterToken('.'),
							Identifier("d"))),
					CharacterToken('.'),
					Identifier("b")),
				null, 0 // "a.c.d.b", but "c.d" is in the wrong place
			};

		column = 10;
		yield return
			new object?[]
			{
				null,
				new BinaryExpression(
					new BinaryExpression(
						new BinaryExpression(
							Identifier("a"),
							CharacterToken('.'),
							Identifier("c")),
						CharacterToken('.'),
						Identifier("d")),
					CharacterToken('.'),
					Identifier("b")),
				"a.c.d.b", 10
			};

		column = 10;
		yield return
			new object?[]
			{
				"c",
				new BinaryExpression(
					new BinaryExpression(
						new BinaryExpression(
							Identifier("a"),
							CharacterToken('.'),
							Identifier("c")),
						CharacterToken('.'),
						Identifier("d")),
					CharacterToken('.'),
					Identifier("b")),
				"a.c.d.b", 10
			};

		column = 10;
		yield return
			new object?[]
			{
				"a", // disallowed
				new BinaryExpression(
					new BinaryExpression(
						new BinaryExpression(
							Identifier("a"),
							CharacterToken('.'),
							Identifier("c")),
						CharacterToken('.'),
						Identifier("d")),
					CharacterToken('.'),
					Identifier("b")),
				null, 0
			};
	}

	[TestCaseSource(nameof(GenerateCollapseDottedIdentifierExpressionTestCases))]
	public void CollapseDottedIdentifierExpression(string? disallowedSlug, BinaryExpression expression, string? expectedIdentifier, int expectedColumn)
	{
		// Arrange
		var dummyRoutine = new Routine(new Module(), new QBX.CodeModel.CompilationElement(new QBX.CodeModel.CompilationUnit()));

		var mapper = new Mapper(dummyRoutine);

		if (disallowedSlug != null)
			mapper.AddDisallowedSlug(disallowedSlug);

		var sut = new Compiler();

		// Act
		var actual = sut.CollapseDottedIdentifierExpression(expression, mapper, out int column);

		// Assert
		expression.Token!.Column.Should().NotBe(expectedColumn);
		actual.Should().Be(expectedIdentifier);
		column.Should().Be(expectedColumn);
	}
}
