using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

using QBX.CodeModel;
using QBX.Utility;

namespace QBX.LexicalAnalysis;

public class Token(MutableBox<int> line, int column, TokenType type, string value, DataType dataType = default)
{
	public TokenType Type => type;
	public string Value => value;
	public DataType DataType => dataType;

	public int Line => line.Value;
	public int Column => column;

	public MutableBox<int> LineNumberBox => line;

	public CodeModel.Statements.Statement? OwnerStatement;

	public int Length => value.Length;

	public bool IsDataType => DataTypeConverter.TryFromToken(this, out var _);

	public bool IsParameterlessKeywordFunction => KeywordFunctionAttribute?.TakesNoParameters ?? false;

	public KeywordFunctionAttribute? KeywordFunctionAttribute
	{
		get
		{
			s_keywordFunctions.TryGetValue(Type, out var attribute);

			return attribute;
		}
	}


	public static bool TryGetKeywordFunctionAttribute(TokenType tokenType, [NotNullWhen(true)] out KeywordFunctionAttribute? value)
		=> s_keywordFunctions.TryGetValue(tokenType, out value);

	// Captured only for AS tokens inside TYPE declarations
	// and tokens inside DATA statements.
	public string? PrecedingWhitespace { get; set; }

	public string StringLiteralValue
	{
		get
		{
			if (Type != TokenType.String)
				throw new Exception("Internal error: Cannot retrieve StringLiteralValue for TokenType." + Type);

			if (value == null)
				throw new Exception("Internal error: String token with no Value");

			if (!value.StartsWith("\""))
				throw new Exception("Internal error: String token that doesn't start with double quote character.");

			if (value.EndsWith("\""))
				return value.Substring(1, value.Length - 2);
			else
				return value.Substring(1, value.Length - 1);
		}
	}

	Token Emplace(MutableBox<int> newLine, int newColumn, string? newValue = null) => new Token(newLine, newColumn, type, newValue ?? value, dataType);

	public override string ToString()
	{
		if (Value == null)
			return Type.ToString();
		else
		{
			string type = (DataType != DataType.Unspecified)
				? "(" + DataType.ToString() + ")"
				: "";

			return Type + ": " + Value + type;
		}
	}

	public static MutableBox<int> CreateDummyLine() => new MutableBox<int>();

	static Dictionary<string, Token> s_keywordTokens =
		typeof(TokenType).GetFields(BindingFlags.Public | BindingFlags.Static)
		.Select(f => (TokenType: (TokenType)f.GetValue(null)!, Keyword: f.GetCustomAttribute<KeywordTokenAttribute>()))
		.Where(f => f.Keyword != null)
		.Select(f => (Keyword: f.Keyword!.Keyword ?? f.TokenType.ToString(), Field: f))
		.ToDictionary(key => key.Keyword, value => new Token(CreateDummyLine(), 0, value.Field.TokenType, value.Keyword), StringComparer.OrdinalIgnoreCase);

	static Dictionary<TokenType, string> s_keywordByTokenType =
		typeof(TokenType).GetFields(BindingFlags.Public | BindingFlags.Static)
		.Select(f => (TokenType: (TokenType)f.GetValue(null)!, Keyword: f.GetCustomAttribute<KeywordTokenAttribute>()))
		.Where(f => f.Keyword != null)
		.ToDictionary(key => key.TokenType, value => value.Keyword?.Keyword ?? value.TokenType.ToString());

	static Dictionary<TokenType, KeywordFunctionAttribute> s_keywordFunctions =
		typeof(TokenType).GetFields(BindingFlags.Public | BindingFlags.Static)
		.Select(f => (TokenType: (TokenType)f.GetValue(null)!, Keyword: f.GetCustomAttribute<KeywordFunctionAttribute>()))
		.Where(f => f.Keyword != null)
		.ToDictionary(key => key.TokenType, value => value.Keyword!);

	static Dictionary<char, Token> s_characterTokens =
		typeof(TokenType).GetFields(BindingFlags.Public | BindingFlags.Static)
		.Select(f => (TokenType: (TokenType)f.GetValue(null)!, TokenCharacter: f.GetCustomAttribute<TokenCharacterAttribute>()))
		.Where(f => f.TokenCharacter != null)
		.Select(f => (TokenCharacter: f.TokenCharacter!.Character, Field: f))
		.ToDictionary(key => key.TokenCharacter, value => new Token(CreateDummyLine(), 0, value.Field.TokenType, value.TokenCharacter.ToString()));

	public static Token GetStatic(MutableBox<int> line, int column, string sourceText, TokenType type) => new Token(line, column, type, value: sourceText);

	public static bool IsKeyword(string keyword) => s_keywordTokens.ContainsKey(keyword);

	public static Token ForKeyword(MutableBox<int> line, int column, string keyword) => s_keywordTokens[keyword].Emplace(line, column, keyword);

	public static bool TryForKeyword(MutableBox<int> line, int column, string keyword, [NotNullWhen(true)] out Token? token)
	{
		if (s_keywordTokens.TryGetValue(keyword, out token))
		{
			token = token.Emplace(line, column);
			return true;
		}

		return false;
	}

	public static void RenderKeyword(TokenType keywordType, TextWriter writer)
	{
		if (!s_keywordByTokenType.TryGetValue(keywordType, out var keyword))
			throw new Exception("Internal error: Keyword not found: " + keywordType);

		writer.Write(keyword);
	}

	public static Token ForCharacter(MutableBox<int> line, int column, char ch) => s_characterTokens[ch].Emplace(line, column);

	public static bool TryForCharacter(MutableBox<int> line, int column, char ch, [NotNullWhen(true)] out Token? token)
	{
		if (s_characterTokens.TryGetValue(ch, out token))
		{
			token = token.Emplace(line, column);
			return true;
		}

		return false;
	}

	public static Token ForStrayCharacter(MutableBox<int> line, int column, char ch)
		=> new Token(line, column, TokenType.StrayCharacter, ch.ToString());

	internal void SetLocation(MutableBox<int> newLine, int newColumn)
	{
		line = newLine;
		column = newColumn;
	}
}
