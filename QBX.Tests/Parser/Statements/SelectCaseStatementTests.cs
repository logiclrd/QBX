using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class SelectCaseStatementTests
{
	[TestCase("SELECT CASE state%")]
	[TestCase("SELECT CASE INKEY$")]
	[TestCase("SELECT CASE (WhatToDO)")]
	public void ShouldParse(string statement)
	{
		// Arrange
		var tokens = new Lexer(statement).ToList();

		tokens.RemoveAll(token => token.Type == TokenType.Whitespace);

		bool inType = false;

		var sut = new BasicParser();

		// Act
		var result = sut.ParseStatement(tokens, ref inType);

		// Assert
		result.Should().BeOfType<SelectCaseStatement>()
			.Which.Expression.Should().NotBeNull();
	}
}
