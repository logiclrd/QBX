using System;
using System.Collections.Generic;
using System.Text;

using QBX.CodeModel.Expressions;
using QBX.LexicalAnalysis;
using QBX.Utility;

namespace QBX.Tests.CodeModel;

public class LiteralExpressionTests
{
	[Test]
	public void SingleValueWithExcessPrecision()
	{
		// Arrange
		var line = new MutableBox<int>();
		int column = 0;

		const string LiteralIn = ".12345678!"; // '!' means only 7 significant figures
		const string LiteralOut = ".1234568"; // '!' not needed in this form

		var token = new Token(line, column, TokenType.Number, LiteralIn, QBX.CodeModel.DataType.SINGLE);

		var sut = new LiteralExpression(token);

		var buffer = new StringWriter();

		// Act
		sut.Render(buffer);

		// Assert
		buffer.ToString().Should().Be(LiteralOut);
	}
}
