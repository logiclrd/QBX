using System.Text;

using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class ScreenStatementTests
{
	[Test]
	public void ShouldParse(
		[Values] bool includeColourSwitch,
		[Values] bool includeActivePage,
		[Values] bool includeVisiblePage)
	{
		// Arrange
		var statement = new StringBuilder();

		statement.Append("SCREEN 0");

		if (includeColourSwitch || includeActivePage || includeVisiblePage)
		{
			statement.Append(", ");

			if (includeColourSwitch)
				statement.Append("1");

			if (includeActivePage || includeVisiblePage)
			{
				statement.Append(", ");

				if (includeActivePage)
					statement.Append("2");

				if (includeVisiblePage)
					statement.Append(", 3");
			}
		}

		var tokens = new Lexer(statement.ToString()).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ref inType);

		// Assert
		result.Should().BeOfType<ScreenStatement>();

		var screenResult = (ScreenStatement)result;

		screenResult.ModeExpression.Should().BeOfType<LiteralExpression>().Which.Token!.Value.Should().Be("0");

		if (includeColourSwitch)
			screenResult.ColourSwitchExpression.Should().BeOfType<LiteralExpression>().Which.Token!.Value.Should().Be("1");
		else
			screenResult.ColourSwitchExpression.Should().BeNull();

		if (includeActivePage)
			screenResult.ActivePageExpression.Should().BeOfType<LiteralExpression>().Which.Token!.Value.Should().Be("2");
		else
			screenResult.ActivePageExpression.Should().BeNull();

		if (includeVisiblePage)
			screenResult.VisiblePageExpression.Should().BeOfType<LiteralExpression>().Which.Token!.Value.Should().Be("3");
		else
			screenResult.VisiblePageExpression.Should().BeNull();
	}
}
