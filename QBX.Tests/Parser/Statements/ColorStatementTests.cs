using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class ColorStatementTests
{
	[TestCase("COLOR", 0, false, false, false)]
	[TestCase("COLOR 1", 1, true, false, false)]
	[TestCase("COLOR , 2", 2, false, true, false)]
	[TestCase("COLOR , , 3", 3, false, false, true)]
	[TestCase("COLOR 1, 2", 2, true, true, false)]
	[TestCase("COLOR 1, , 3", 3, true, false, true)]
	[TestCase("COLOR , 2, 3", 3, false, true, true)]
	[TestCase("COLOR 1, 2, 3", 3, true, true, true)]
	public void ShouldParse(string statement, int argCount, bool arg1, bool arg2, bool arg3)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, colonAfter: false, ref inType);

		// Assert
		result.Should().BeOfType<ColorStatement>();

		var colorResult = (ColorStatement)result;

		colorResult.Arguments.Should().HaveCount(argCount);

		if (arg1)
			colorResult.Arguments[0].Should().NotBeNull();
		else if (argCount > 0)
			colorResult.Arguments[0].Should().BeNull();

		if (arg2)
			colorResult.Arguments[1].Should().NotBeNull();
		else if (argCount > 1)
			colorResult.Arguments[1].Should().BeNull();

		if (arg3)
			colorResult.Arguments[2].Should().NotBeNull();
		else if (argCount > 2)
			colorResult.Arguments[2].Should().BeNull();
	}
}

/*
using QBX.CodeModel.Expressions;

namespace QBX.Tests.Parser.Statements;

public class ColorStatement
{
	public override StatementType Type => StatementType.Color;

	public ExpressionList? Arguments { get; set; }

	public ColorStatement()
	{
	}

	public ColorStatement(ExpressionList arguments)
	{
		Arguments = arguments;
	}

	public override void Render(TextWriter writer)
	{
		writer.Write("COLOR");

		if (Arguments != null)
		{
			writer.Write(' ');
			Arguments.Render(writer);
		}
	}
}

*/
