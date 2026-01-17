using System.Text;

using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class LocateStatementTests
{
	[Test]
	public void ShouldParse(
		[Values] bool includeRow,
		[Values] bool includeColumn,
		[Values] bool includeCursor,
		[Values] bool includeStart,
		[Values] bool includeEnd)
	{
		// Arrange
		bool expectException = !(includeRow || includeColumn || includeCursor || includeStart || includeEnd);

		var statement = new StringBuilder();

		statement.Append("LOCATE");

		if (includeRow || includeColumn || includeCursor || includeStart || includeEnd)
		{
			statement.Append(' ');

			if (includeRow)
				statement.Append("1");

			if (includeColumn || includeCursor || includeStart || includeEnd)
			{
				statement.Append(", ");

				if (includeColumn)
					statement.Append("2");

				if (includeCursor || includeStart || includeEnd)
				{
					statement.Append(", ");

					if (includeCursor)
						statement.Append("3");

					if (includeStart || includeEnd)
					{
						statement.Append(", 4");

						if (includeEnd)
							statement.Append(", 5");
					}
				}
			}
		}

		var tokens = new Lexer(statement.ToString()).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		var sut = new BasicParser();

		// Act
		Statement? result = null;

		Action action =
			() =>
			{
				result = sut.ParseStatement(tokens, ignoreErrors: false);
			};

		// Assert
		if (expectException)
			action.Should().Throw<SyntaxErrorException>();
		else
		{
			action.Should().NotThrow();

			result.Should().BeOfType<LocateStatement>();

			var locateResult = (LocateStatement)result;

			if (includeRow)
				locateResult.RowExpression.Should().BeOfType<LiteralExpression>().Which.Token!.Value.Should().Be("1");
			else
				locateResult.RowExpression.Should().BeNull();

			if (includeColumn)
				locateResult.ColumnExpression.Should().BeOfType<LiteralExpression>().Which.Token!.Value.Should().Be("2");
			else
				locateResult.ColumnExpression.Should().BeNull();

			if (includeCursor)
				locateResult.CursorVisibilityExpression.Should().BeOfType<LiteralExpression>().Which.Token!.Value.Should().Be("3");
			else
				locateResult.CursorVisibilityExpression.Should().BeNull();

			if (includeStart || includeEnd)
				locateResult.CursorStartExpression.Should().BeOfType<LiteralExpression>().Which.Token!.Value.Should().Be("4");
			else
				locateResult.CursorStartExpression.Should().BeNull();

			if (includeEnd)
				locateResult.CursorEndExpression.Should().BeOfType<LiteralExpression>().Which.Token!.Value.Should().Be("5");
			else
				locateResult.CursorEndExpression.Should().BeNull();
		}
	}
}
