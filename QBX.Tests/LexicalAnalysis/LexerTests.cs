using System.Reflection;

using QBX.CodeModel;
using QBX.LexicalAnalysis;

namespace QBX.Tests.LexicalAnalysis;

public class LexerTests
{
	[Test]
	public void EmptyInput()
	{
		// Arrange
		var sut = new Lexer("");

		// Act
		var result = sut.ToList();

		// Assert
		result.Should().BeEmpty();
	}

	[Test]
	public void Newlines(
		[Values("R", "L", "N", "RN", "LN", "RRR", "LLL", "NNN", "NNNR", "NNNL")]
		string template)
	{
		// Arrange
		var expected = template.Select(ch =>
			ch switch
			{
				'R' => "\r",
				'L' => "\n",
				'N' => "\r\n",

				_ => throw new Exception()
			}).ToArray();

		string input = string.Concat(expected);

		var sut = new Lexer(input);

		// Act
		var result = sut.ToList();

		// Assert
		result.Should().HaveCount(expected.Length);

		for (int i = 0; i < expected.Length; i++)
		{
			result[i].Type.Should().Be(TokenType.NewLine);
			result[i].Value.Should().Be(expected[i]);
		}
	}

	static IEnumerable<object[]> AllTokens()
	{
		foreach (var field in typeof(TokenType).GetFields(BindingFlags.Static | BindingFlags.Public))
		{
			var tokenType = (TokenType)field.GetValue(null)!;

			if (field.GetCustomAttribute<KeywordTokenAttribute>() is KeywordTokenAttribute keywordToken)
				yield return [keywordToken.Keyword ?? tokenType.ToString(), tokenType];
			else if (field.GetCustomAttribute<TokenCharacterAttribute>() is TokenCharacterAttribute characterToken)
				yield return [characterToken.Character.ToString(), tokenType];
		}

		yield return ["<=", TokenType.LessThanOrEquals];
		yield return [">=", TokenType.GreaterThanOrEquals];
		yield return ["<>", TokenType.NotEquals];
	}

	[TestCaseSource(nameof(AllTokens))]
	public void SingleToken(string input, TokenType expectedType)
	{
		// Arrange
		var sut = new Lexer(input);

		// Act
		var result = sut.ToList();

		// Assert
		result.Should().HaveCount(1);
		result[0].Type.Should().Be(expectedType);
	}

	[Test]
	public void Comments(
		[Values("'", "REM ")]
		string style,
		[Values("", "a", "testing", "multiple words", "multiple words ' including apostrophe")]
		string content)
	{
		// Arrange
		var input = (style + content).Trim();

		var sut = new Lexer(input);

		// Act
		var result = sut.ToList();

		// Assert
		result.Should().HaveCount(1);
		result[0].Type.Should().Be(TokenType.Comment);
		result[0].Value.Should().Be(input);
	}

	[Test]
	public void Strings(
		[Values("", "a", "testing testing")]
		string content,
		[Values(false, true)]
		bool propertyTerminated)
	{
		// Arrange
		string input = propertyTerminated
			? '"' + content + '"'
			: '"' + content;

		var sut = new Lexer(input);

		// Act
		var result = sut.ToList();

		// Assert
		result.Should().HaveCount(1);
		result[0].Type.Should().Be(TokenType.String);
		result[0].Value.Should().Be(input);
	}

	[TestCase("1", DataType.Unspecified)]
	[TestCase("10", DataType.Unspecified)]
	[TestCase("1.1", DataType.Unspecified)]
	[TestCase("1.", DataType.Unspecified)]
	[TestCase(".5", DataType.Unspecified)]
	[TestCase("2%", DataType.INTEGER)]
	[TestCase("2&", DataType.LONG)]
	[TestCase("2!", DataType.SINGLE)]
	[TestCase("2#", DataType.DOUBLE)]
	[TestCase("2@", DataType.CURRENCY)]
	public void Numbers(string serialized, DataType expectedType)
	{
		// Arrange
		var sut = new Lexer(serialized);

		// Act
		var result = sut.ToList();

		// Assert
		result.Should().HaveCount(1);
		result[0].Type.Should().Be(TokenType.Number);
		result[0].Value.Should().Be(serialized);
		result[0].DataType.Should().Be(expectedType);
	}

	[TestCase("&H0", DataType.Unspecified)]
	[TestCase("&H0%", DataType.INTEGER)]
	[TestCase("&H0&", DataType.LONG)]
	[TestCase("&HFEDCBA98", DataType.Unspecified)]
	[TestCase("&H76543210", DataType.Unspecified)]
	public void HexNumbers(string serialized, DataType expectedType)
	{
		// Arrange
		var sut = new Lexer(serialized);

		// Act
		var result = sut.ToList();

		// Assert
		result.Should().HaveCount(1);
		result[0].Type.Should().Be(TokenType.Number);
		result[0].Value.Should().Be(serialized);
		result[0].DataType.Should().Be(expectedType);
	}

	[TestCase("&O0", DataType.Unspecified)]
	[TestCase("&O0%", DataType.INTEGER)]
	[TestCase("&O0&", DataType.LONG)]
	[TestCase("&O76543210", DataType.Unspecified)]
	public void OctalNumbers(string serialized, DataType expectedType)
	{
		// Arrange
		var sut = new Lexer(serialized);

		// Act
		var result = sut.ToList();

		// Assert
		result.Should().HaveCount(1);
		result[0].Type.Should().Be(TokenType.Number);
		result[0].Value.Should().Be(serialized);
		result[0].DataType.Should().Be(expectedType);
	}

	public void Identifiers(
		[Values("i", "bob", "M34", "HelloWorld", "Hello9World")]
		string name,
		[Values]
		DataType dataType)
	{
		// Arrange
		string content =
			dataType == DataType.Unspecified
			? name
			: name + new TypeCharacter(dataType).Character;

		var sut = new Lexer(content);

		// Act
		var result = sut.ToList();

		// Assert
		result.Should().HaveCount(1);
		result[0].Type.Should().Be(TokenType.Identifier);
		result[0].Value.Should().Be(content);
		result[0].DataType.Should().Be(dataType);
	}

	[TestCase(" ")]
	[TestCase("\t")]
	[TestCase("     ")]
	[TestCase("\t\t\t")]
	[TestCase("\t \t \t")]
	[TestCase(" \t \t ")]
	public void Whitespace(string content)
	{
		// Arrange
		var sut = new Lexer(content);

		// Act
		var result = sut.ToList();

		// Assert
		result.Should().HaveCount(1);
		result[0].Type.Should().Be(TokenType.Whitespace);
		result[0].Value.Should().Be(content);
	}

	static IEnumerable<object[]> LineAndColumnTestSource()
	{
		object Expected(params (int Line, int Column)[] data) => data;

		yield return
			[
				"PRINT \"Hello, world\"\n",
				Expected((0, 0), (0, 5), (0, 6), (0, 20))
			];

		yield return//1         2         3         4         5          6
			[//1234567890123456789012345678901234567890123456789012345678 901234567890
				"newX# = COS(angle#) * oldX# + SIN(angle#) * oldY# ' rotate\nPRINT newX#",
				Expected(
					(0, 0), // "newX#"
					(0, 5), // " "
					(0, 6), // "="
					(0, 7), // " "
					(0, 8), // "COS"
					(0, 11), // "("
					(0, 12), // "angle#"
					(0, 18), // ")"
					(0, 19), // " "
					(0, 20), // "*"
					(0, 21), // " "
					(0, 22), // "oldX#"
					(0, 27), // " "
					(0, 28), // "+"
					(0, 29), // " "
					(0, 30), // "SIN"
					(0, 33), // "("
					(0, 34), // "angle#"
					(0, 40), // ")"
					(0, 41), // " "
					(0, 42), // "*"
					(0, 43), // " "
					(0, 44), // "oldY#"
					(0, 49), // " "
					(0, 50), // "' rotate"
					(0, 58), // "\n"
					(1, 0), // "PRINT"
					(1, 5), // " "
					(1, 6) // "newX#"
				)
			];

		yield return
			[
				"IF a& >= 1 AND a& <= 10 THEN",
				Expected(
					(0, 0), // "IF"
					(0, 2), // " "
					(0, 3), // "a&"
					(0, 5), // " "
					(0, 6), // ">="
					(0, 8), // " "
					(0, 9), // "1"
					(0, 10), // " "
					(0, 11), // "AND"
					(0, 14), // " "
					(0, 15), // "a&"
					(0, 17), // " "
					(0, 18), // "<="
					(0, 20), // " "
					(0, 21), // "10"
					(0, 23), // " "
					(0, 24) // "THEN"
				)
			];
	}

	[TestCaseSource(nameof(LineAndColumnTestSource))]
	public void LineAndColumn(string input, (int Line, int Column)[] expected)
	{
		// Arrange
		var sut = new Lexer(input);

		// Act
		var result = sut.ToList();

		// Assert
		result.Should().HaveCount(expected.Length);

		for (int i = 0; i < result.Count; i++)
		{
			result[i].Line.Should().Be(expected[i].Line);
			result[i].Column.Should().Be(expected[i].Column);
		}
	}

	[Test]
	public void NewLineAfterWhitespace()
	{
		// Arrange
		var input = "   \r\n   \r\n";

		var sut = new Lexer(input);

		// Act
		var result = sut.ToList();

		// Assert
		result.Should().HaveCount(4);
		result[0].Type.Should().Be(TokenType.Whitespace);
		result[0].Value.Should().Be("   ");
		result[1].Type.Should().Be(TokenType.NewLine);
		result[2].Type.Should().Be(TokenType.Whitespace);
		result[2].Value.Should().Be("   ");
		result[3].Type.Should().Be(TokenType.NewLine);
	}

	[TestCase("1")]
	[TestCase(".2")]
	[TestCase("0.7")]
	[TestCase("&H35")]
	[TestCase("&O35")]
	public void NumberFollowedImmediatelyByLetter(string number)
	{
		// Arrange
		string input = number + "TO";

		var sut = new Lexer(input);

		// Act
		var result = sut.ToList();

		// Assert
		result.Should().HaveCount(2);
		result[0].Type.Should().Be(TokenType.Number);
		result[0].Value.Should().Be(number);
		result[1].Type.Should().Be(TokenType.TO);
	}

	[TestCase("-0")]
	[TestCase("-1")]
	[TestCase("-1.0")]
	[TestCase("-1.1")]
	[TestCase("-0.1")]
	[TestCase("-.1")]
	[TestCase("-1.123")]
	[TestCase("-0.123")]
	[TestCase("-.123")]
	[TestCase("-&HD")]
	[TestCase("-&HDD")]
	[TestCase("-&O12")]
	public void NegativeNumber(string input)
	{
		// Arrange
		var sut = new Lexer(input);

		// Act
		var result = sut.ToList();

		// Assert
		result.Should().HaveCount(1);
		result[0].Type.Should().Be(TokenType.Number);
		result[0].Value.Should().Be(input);
	}

	[TestCase("-0")]
	[TestCase("-1")]
	[TestCase("-1.0")]
	[TestCase("-1.1")]
	[TestCase("-0.1")]
	[TestCase("-.1")]
	[TestCase("-1.123")]
	[TestCase("-0.123")]
	[TestCase("-.123")]
	[TestCase("-&HD")]
	[TestCase("-&HDD")]
	[TestCase("-&O12")]
	public void NegativeNumberFollowedByNegativeNumber(string input)
	{
		// Arrange
		var sut = new Lexer(input + input);

		// Act
		var result = sut.ToList();

		// Assert
		result.Should().HaveCount(2);
		result[0].Type.Should().Be(TokenType.Number);
		result[0].Value.Should().Be(input);
		result[1].Type.Should().Be(TokenType.Number);
		result[1].Value.Should().Be(input);
	}
}
