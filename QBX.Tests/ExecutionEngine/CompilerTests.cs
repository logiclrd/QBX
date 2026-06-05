using QBX.CodeModel;
using QBX.CodeModel.Expressions;
using QBX.ExecutionEngine;
using QBX.ExecutionEngine.Compiled;
using QBX.LexicalAnalysis;
using QBX.Parser;

using static QBX.Tests.Utility.IdentifierHelpers;

namespace QBX.Tests.ExecutionEngine;

public class CompilerTests
{
	static IEnumerable<object?[]> GenerateCollapseDottedIdentifierExpressionTestCases()
	{
		int column = 0;

		var identifierRepository = new IdentifierRepository();

		IdentifierExpression Identifier(string identifier)
		{
			var ret = new IdentifierExpression(
				new Token(Token.CreateDummyLine(), column, TokenType.Identifier, identifier),
				identifierRepository.GetOrAddCanonicalIdentifier(identifier));

			column += ret.Token!.Length;

			return ret;
		}

		Token CharacterToken(char ch)
		{
			var ret = Token.ForCharacter(Token.CreateDummyLine(), column, ch);

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
		var dummyRoutine = new Routine(new Module(new Compilation()), moduleMapper: null, source: new QBX.CodeModel.CompilationElement(new QBX.CodeModel.CompilationUnit()));

		var mapper = dummyRoutine.Mapper;

		if (disallowedSlug != null)
			mapper.AddDisallowedSlug(disallowedSlug);

		var identifierRepository = new IdentifierRepository();

		var sut = new Compiler(identifierRepository);

		// Act
		var actual = sut.CollapseDottedIdentifierExpression(expression, mapper, out int column);

		// Assert
		expression.Token!.Column.Should().NotBe(expectedColumn);
		actual.Should().Be(ID(expectedIdentifier));
		column.Should().Be(expectedColumn);
	}

	const string LineNumberWithComment = "10 'hello";

	[TestCase(
@"FOR i = 1 TO 5
@@
NEXT i", typeof(QBX.ExecutionEngine.Compiled.Statements.ForStatement))]
	[TestCase(
@"IF condition THEN
@@
END IF", typeof(QBX.ExecutionEngine.Compiled.Statements.IfStatement))]
	[TestCase(
@"IF condition THEN
PRINT 5 * 5
ELSE
@@
END IF", typeof(QBX.ExecutionEngine.Compiled.Statements.IfStatement))]
	[TestCase(
@"WHILE condition
@@
WEND", typeof(QBX.ExecutionEngine.Compiled.Statements.LoopStatement))]
	[TestCase(
@"DO
@@
LOOP UNTIL 3 > 5", typeof(QBX.ExecutionEngine.Compiled.Statements.LoopStatement))]
	[TestCase(
@"SELECT CASE value
CASE ELSE:
@@
END SELECT", typeof(QBX.ExecutionEngine.Compiled.Statements.SelectCaseStatement))]
	public void ShouldParseLineNumberWithCommentWithinBlock(string template, Type expectedStatementType)
	{
		// Arrange
		var code = template.Replace("@@", LineNumberWithComment);

		var tokens = new Lexer(code);

		var identifierRepository = new IdentifierRepository();

		var parser = new BasicParser(identifierRepository);

		var parsedCode = parser.ParseCodeLines(tokens, ignoreErrors: false);

		var unit = new CompilationUnit();

		var element = new CompilationElement(unit);

		element.Type = CompilationElementType.Main;

		unit.AddElement(element);

		element.AddLines(parsedCode);

		var compiler = new Compiler(identifierRepository);

		var compilation = new Compilation();

		// Act
		var module = compiler.Compile(unit, compilation);

		module.MainRoutine.Should().NotBeNull();
		module.MainRoutine.Statements.Should().HaveCount(1);
		module.MainRoutine.Statements[0].Should().BeAssignableTo(expectedStatementType);
	}
}
