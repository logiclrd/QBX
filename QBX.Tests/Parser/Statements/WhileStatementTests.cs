using QBX.CodeModel.Statements;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.Tests.Parser.Statements;

public class WhileStatementTests
{
	[TestCase("WHILE state%")]
	[TestCase("WHILE INKEY$")]
	[TestCase("WHILE (WhatToDO)")]
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
		result.Should().BeOfType<WhileStatement>()
			.Which.Condition.Should().NotBeNull();
	}
}
