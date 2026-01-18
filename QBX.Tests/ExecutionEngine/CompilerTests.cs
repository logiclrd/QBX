using QBX.CodeModel.Expressions;
using QBX.ExecutionEngine;
using QBX.ExecutionEngine.Compiled;
using QBX.LexicalAnalysis;

namespace QBX.Tests.ExecutionEngine;

public class CompilerTests
{
	static IEnumerable<object?[]> GenerateCollapseDottedIdentifierExpressionTestCases()
	{
		IdentifierExpression Identifier(string identifier)
			=> new IdentifierExpression(new Token(0, 0, TokenType.Identifier, identifier));
		Token CharacterToken(char ch)
			=> Token.ForCharacter(0, 0, ch);

		yield return
			new object?[]
			{
				null,
				new BinaryExpression(
					Identifier("a"),
					CharacterToken('*'),
					Identifier("b")),
				null // "a*b"
			};

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
				null // "a.b+c"
			};

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
				null // "a.b.c" but down the wrong side
			};

		yield return
			new object?[]
			{
				null,
				new BinaryExpression(
					Identifier("a"),
					CharacterToken('.'),
					Identifier("b")),
				"a.b"
			};

		yield return
			new object?[]
			{
				"b",
				new BinaryExpression(
					Identifier("a"),
					CharacterToken('.'),
					Identifier("b")),
				"a.b"
			};

		yield return
			new object?[]
			{
				"a", // disallowed
				new BinaryExpression(
					Identifier("a"),
					CharacterToken('.'),
					Identifier("b")),
				null
			};

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
				null // "a+c.b"
			};

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
				null // "a.c.b"
			};

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
				"a.c.b"
			};

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
				"a.c.b"
			};

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
				null
			};

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
				null // "a.c.d.b", but "c.d" is in the wrong place
			};

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
				"a.c.d.b"
			};

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
				"a.c.d.b"
			};

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
				null
			};
	}

	[TestCaseSource(nameof(GenerateCollapseDottedIdentifierExpressionTestCases))]
	public void CollapseDottedIdentifierExpression(string? disallowedSlug, BinaryExpression expression, string? expectedIdentifier)
	{
		// Arrange
		var dummyRoutine = new Routine(new Module(), new QBX.CodeModel.CompilationElement(new QBX.CodeModel.CompilationUnit()));

		var mapper = new Mapper(dummyRoutine);

		if (disallowedSlug != null)
			mapper.AddDisallowedSlug(disallowedSlug);

		var sut = new Compiler();

		// Act
		var actual = sut.CollapseDottedIdentifierExpression(expression, mapper);

		// Assert
		actual.Should().Be(expectedIdentifier);
	}
}
