using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class SubStatementTests
{
	[TestCase("SUB Center (row AS INTEGER, text$)", "Center", new string[] { "row", "text$" }, false)]
	[TestCase("SUB Center (row AS INTEGER, text$) STATIC", "Center", new string[] { "row", "text$" }, true)]
	[TestCase("SUB DrawScreen", "DrawScreen", null, false)]
	[TestCase("SUB DrawScreen STATIC", "DrawScreen", null, true)]
	public void ShouldParse(string definition, string subName, string[]? arguments, bool expectIsStatic)
	{
		// Arrange
		var tokens = new Lexer(definition).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ref inType);

		// Assert
		result.Should().BeOfType<SubStatement>();

		var subResult = (SubStatement)result;

		subResult.Name.Should().Be(subName);

		if (arguments == null)
			subResult.Parameters.Should().BeNull();
		else
		{
			subResult.Parameters.Should().NotBeNull();
			subResult.Parameters!.Parameters.Should().HaveCount(arguments.Length);

			for (int i = 0; i < arguments.Length; i++)
				subResult.Parameters!.Parameters[i].Name.Should().Be(arguments[i]);
		}

		subResult.IsStatic.Should().Be(expectIsStatic);
	}
}
