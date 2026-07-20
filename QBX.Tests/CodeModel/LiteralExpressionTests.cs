using QBX.CodeModel;
using QBX.CodeModel.Expressions;
using QBX.ExecutionEngine.Compiled.Expressions;
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

	[TestCase("&H0", DataType.Unspecified, "&H0")]
	[TestCase("&H0%", DataType.INTEGER, "&H0")]
	[TestCase("&H0&", DataType.LONG, "&H0&")]
	[TestCase("&H7FFF", DataType.Unspecified, "&H7FFF")]
	[TestCase("&H7FFF%", DataType.INTEGER, "&H7FFF")]
	[TestCase("&H7FFF&", DataType.LONG, "&H7FFF&")]
	[TestCase("&H8000", DataType.Unspecified, "&H8000")]
	[TestCase("&H8000%", DataType.INTEGER, "&H8000")]
	[TestCase("&H8000&", DataType.LONG, "&H8000&")]
	[TestCase("&HFFFF", DataType.Unspecified, "&HFFFF")]
	[TestCase("&HFFFF%", DataType.INTEGER, "&HFFFF")]
	[TestCase("&HFFFF&", DataType.LONG, "&HFFFF&")]
	[TestCase("&H10000", DataType.Unspecified, "&H10000")]
	[TestCase("&H10000&", DataType.LONG, "&H10000")]
	[TestCase("&H80000000", DataType.Unspecified, "&H80000000")]
	[TestCase("&H80000000&", DataType.LONG, "&H80000000")]
	public void HexadecimalValueQualification(string valueChars, DataType inputQualification, string expectedOutput)
	{
		// Arrange
		var token = new Token(new MutableBox<int>(1), 1, TokenType.Number, valueChars, inputQualification);

		var sut = new LiteralExpression(token);

		var buffer = new StringWriter();

		// Act
		sut.Render(buffer);

		// Assert
		string output = buffer.ToString();

		output.Should().Be(expectedOutput);
	}

	[TestCase("&O0", DataType.Unspecified, "&O0")]
	[TestCase("&O0%", DataType.INTEGER, "&O0")]
	[TestCase("&O0&", DataType.LONG, "&O0&")]
	[TestCase("&O77777", DataType.Unspecified, "&O77777")]
	[TestCase("&O77777%", DataType.INTEGER, "&O77777")]
	[TestCase("&O77777&", DataType.LONG, "&O77777&")]
	[TestCase("&O100000", DataType.Unspecified, "&O100000")]
	[TestCase("&O100000%", DataType.INTEGER, "&O100000")]
	[TestCase("&O100000&", DataType.LONG, "&O100000&")]
	[TestCase("&O177777", DataType.Unspecified, "&O177777")]
	[TestCase("&O177777%", DataType.INTEGER, "&O177777")]
	[TestCase("&O177777&", DataType.LONG, "&O177777&")]
	[TestCase("&O200000", DataType.Unspecified, "&O200000")]
	[TestCase("&O200000&", DataType.LONG, "&O200000")]
	[TestCase("&O20000000000", DataType.Unspecified, "&O20000000000")]
	[TestCase("&O20000000000&", DataType.LONG, "&O20000000000")]
	public void OctalValueQualification(string valueChars, DataType inputQualification, string expectedOutput)
	{
		// Arrange
		var token = new Token(new MutableBox<int>(1), 1, TokenType.Number, valueChars, inputQualification);

		var sut = new LiteralExpression(token);

		var buffer = new StringWriter();

		// Act
		sut.Render(buffer);

		// Assert
		string output = buffer.ToString();

		output.Should().Be(expectedOutput);
	}
}
