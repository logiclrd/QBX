using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class FunctionStatementTests
{
	[TestCase("FUNCTION Center (row AS INTEGER, text$)", "Center", new string[] { "row", "text$" }, false)]
	[TestCase("FUNCTION Center (row AS INTEGER, text$) STATIC", "Center", new string[] { "row", "text$" }, true)]
	[TestCase("FUNCTION DrawScreen%", "DrawScreen%", null, false)]
	[TestCase("FUNCTION DrawScreen STATIC", "DrawScreen", null, true)]
	public void ShouldParse(string definition, string functionName, string[]? arguments, bool expectIsStatic)
	{
		// Arrange
		var tokens = new Lexer(definition).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ignoreErrors: false);

		// Assert
		result.Should().BeOfType<FunctionStatement>();

		var functionResult = (FunctionStatement)result;

		functionResult.Name.Should().Be(functionName);

		if (arguments == null)
			functionResult.Parameters.Should().BeNull();
		else
		{
			functionResult.Parameters.Should().NotBeNull();
			functionResult.Parameters!.Parameters.Should().HaveCount(arguments.Length);

			for (int i = 0; i < arguments.Length; i++)
				functionResult.Parameters!.Parameters[i].Name.Should().Be(arguments[i]);
		}

		functionResult.IsStatic.Should().Be(expectIsStatic);
	}
}
