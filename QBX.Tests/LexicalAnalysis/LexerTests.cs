using System.Reflection;
using NUnit.Framework.Constraints;
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

	[TestCase("i", DataType.Unspecified)]
	[TestCase("bob", DataType.Unspecified)]
	[TestCase("M34", DataType.Unspecified)]
	[TestCase("HelloWorld", DataType.Unspecified)]
	[TestCase("Hello9World", DataType.Unspecified)]
	public void Identifiers(
		[Values("i", "bob", "M34", "HelloWorld", "Hello9World")]
		string name,
		[Values]
		DataType dataType)
	{
		// Arrange
		string content = name + new TypeCharacter(dataType).Character;

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
				Expected((1, 1), (1, 6), (1, 7), (1, 21))
			];

		yield return//1         2         3         4         5          6
			[//1234567890123456789012345678901234567890123456789012345678 901234567890
				"newX# = COS(angle#) * oldX# + SIN(angle#) * oldY# ' rotate\nPRINT newX#",
				Expected(
					(1, 1), // "newX#"
					(1, 6), // " "
					(1, 7), // "="
					(1, 8), // " "
					(1, 9), // "COS"
					(1, 12), // "("
					(1, 13), // "angle#"
					(1, 19), // ")"
					(1, 20), // " "
					(1, 21), // "*"
					(1, 22), // " "
					(1, 23), // "oldX#"
					(1, 28), // " "
					(1, 29), // "+"
					(1, 30), // " "
					(1, 31), // "SIN"
					(1, 34), // "("
					(1, 35), // "angle#"
					(1, 41), // ")"
					(1, 42), // " "
					(1, 43), // "*"
					(1, 44), // " "
					(1, 45), // "oldY#"
					(1, 50), // " "
					(1, 51), // "' rotate"
					(1, 59), // "\n"
					(2, 1), // "PRINT"
					(2, 6), // " "
					(2, 7) // "newX#"
				)
			];

		yield return
			[
				"IF a& >= 1 AND a& <= 10 THEN",
				Expected(
					(1, 1), // "IF"
					(1, 3), // " "
					(1, 4), // "a&"
					(1, 6), // " "
					(1, 7), // ">="
					(1, 9), // " "
					(1, 10), // "1"
					(1, 11), // " "
					(1, 12), // "AND"
					(1, 15), // " "
					(1, 16), // "a&"
					(1, 18), // " "
					(1, 19), // "<="
					(1, 21), // " "
					(1, 22), // "10"
					(1, 24), // " "
					(1, 25) // "THEN"
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
}
