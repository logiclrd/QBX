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
}
