using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class InputStatementTests
{
	[TestCase("INPUT a%", true, false, false, 1)]
	[TestCase("INPUT ; b%", false, false, false, 1)]
	[TestCase("INPUT \"Prompt\"; b$", true, true, true, 1)]
	[TestCase("INPUT \"Prompt\", b$", true, true, false, 1)]
	[TestCase("INPUT c%, d%", true, false, false, 2)]
	[TestCase("INPUT c%(e%), d.f", true, false, false, 2)]
	[TestCase("INPUT c(e%).g, d.f(e%)", true, false, false, 2)]
	public void ShouldParse(string statement, bool expectEchoNewLine, bool expectPrompt, bool expectPromptQuestionMark, int expectedNumTargets)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, colonAfter: false, ref inType);

		// Assert
		result.Should().BeOfType<InputStatement>();

		var inputResult = (InputStatement)result;

		inputResult.EchoNewLine.Should().Be(expectEchoNewLine);

		if (expectPrompt)
		{
			inputResult.PromptString.Should().NotBeNull();
			inputResult.PromptQuestionMark.Should().Be(expectPromptQuestionMark);
		}
		else
			inputResult.PromptString.Should().BeNull();

		inputResult.Targets.Should().HaveCount(expectedNumTargets);
	}
}
