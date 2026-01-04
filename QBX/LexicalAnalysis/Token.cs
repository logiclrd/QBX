using QBX.CodeModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace QBX.LexicalAnalysis;

public class Token(int line, int column, TokenType type, string value, DataType dataType = default)
{
	public TokenType Type => type;
	public string? Value => value;
	public DataType DataType => dataType;

	public int Line => line;
	public int Column => column;

	public int Length
	{
		get
		{
			if (Value != null)
				return Value.Length;

			if (s_characterTokenTypes.Contains(Type))
				return 1;

			if (Type == TokenType.Empty)
				return 0;

			if (s_tokenValues.TryGetValue(Type, out var value))
				return value.Length;

			throw new Exception("Internal error");
		}
	}

	public bool IsDataType => DataTypeConverter.TryFromToken(this, out var _);

	public bool IsKeywordFunction => s_keywordFunctionsParameters.Contains(Type);
	public bool IsParameterlessKeywordFunction => s_keywordFunctionsNoParameters.Contains(Type);

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

			if (!value.StartsWith("\"") || !value.EndsWith("\""))
				throw new Exception("Internal error: String token that doesn't start and end with double quote character.");

			return value.Substring(1, value.Length - 2);
		}
	}

	Token Emplace(int newLine, int newColumn) => new Token(newLine, newColumn, type, value, dataType);

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

	static Dictionary<string, Token> s_keywordTokens =
		typeof(TokenType).GetFields(BindingFlags.Public | BindingFlags.Static)
		.Select(f => (TokenType: (TokenType)f.GetValue(null)!, Keyword: f.GetCustomAttribute<KeywordTokenAttribute>()))
		.Where(f => f.Keyword != null)
		.ToDictionary(key => key.Keyword!.Keyword ?? key.TokenType.ToString(), value => new Token(0, 0, value.TokenType, value.Keyword?.Keyword ?? value.TokenType.ToString()), StringComparer.OrdinalIgnoreCase);

	static Dictionary<TokenType, string> s_keywordByTokenType =
		typeof(TokenType).GetFields(BindingFlags.Public | BindingFlags.Static)
		.Select(f => (TokenType: (TokenType)f.GetValue(null)!, Keyword: f.GetCustomAttribute<KeywordTokenAttribute>()))
		.Where(f => f.Keyword != null)
		.ToDictionary(key => key.TokenType, value => value.Keyword?.Keyword ?? value.TokenType.ToString());

	static HashSet<TokenType> s_keywordFunctionsNoParameters =
		typeof(TokenType).GetFields(BindingFlags.Public | BindingFlags.Static)
		.Select(f => (TokenType: (TokenType)f.GetValue(null)!, Keyword: f.GetCustomAttribute<KeywordFunctionAttribute>()))
		.Where(f => (f.Keyword != null) && f.Keyword.TakesNoParameters)
		.Select(f => f.TokenType)
		.ToHashSet();

	static HashSet<TokenType> s_keywordFunctionsParameters =
		typeof(TokenType).GetFields(BindingFlags.Public | BindingFlags.Static)
		.Select(f => (TokenType: (TokenType)f.GetValue(null)!, Keyword: f.GetCustomAttribute<KeywordFunctionAttribute>()))
		.Where(f => (f.Keyword != null) && f.Keyword.TakesParameters)
		.Select(f => f.TokenType)
		.ToHashSet();

	static Dictionary<TokenType, string> s_tokenValues =
		typeof(TokenType).GetFields(BindingFlags.Public | BindingFlags.Static)
		.Select(f => (TokenType: (TokenType)f.GetValue(null)!, Keyword: f.GetCustomAttribute<KeywordTokenAttribute>()))
		.Where(f => f.Keyword != null)
		.Select(f => (TokenType: f.TokenType, Value: f.Keyword!.Keyword ?? f.TokenType.ToString()))
		.Concat(
			typeof(TokenType).GetFields(BindingFlags.Public | BindingFlags.Static)
			.Select(f => (TokenType: (TokenType)f.GetValue(null)!, ValueAttribute: f.GetCustomAttribute<TokenValueAttribute>()))
			.Where(f => f.ValueAttribute != null)
			.Select(f => (TokenType: f.TokenType, Value: f.ValueAttribute!.Value)))
		.ToDictionary(key => key.TokenType, value => value.Value);

	static Dictionary<char, Token> s_characterTokens =
		typeof(TokenType).GetFields(BindingFlags.Public | BindingFlags.Static)
		.Select(f => (TokenType: (TokenType)f.GetValue(null)!, TokenCharacter: f.GetCustomAttribute<TokenCharacterAttribute>()))
		.Where(f => f.TokenCharacter != null)
		.ToDictionary(key => key.TokenCharacter!.Character, value => new Token(0, 0, value.TokenType, ""));

	static HashSet<TokenType> s_characterTokenTypes =
		typeof(TokenType).GetFields(BindingFlags.Public | BindingFlags.Static)
		.Select(f => (TokenType: (TokenType)f.GetValue(null)!, TokenCharacter: f.GetCustomAttribute<TokenCharacterAttribute>()))
		.Where(f => f.TokenCharacter != null)
		.Select(value => value.TokenType)
		.ToHashSet();

	public static Token GetStatic(int line, int column, TokenType type) => new Token(line, column, type, "");

	public static Token ForKeyword(int line, int column, string keyword) => s_keywordTokens[keyword].Emplace(line, column);

	public static bool TryForKeyword(int line, int column, string keyword, [NotNullWhen(true)] out Token? token)
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

	public static Token ForCharacter(int line, int column, char ch) => s_characterTokens[ch].Emplace(line, column);

	public static bool TryForCharacter(int line, int column, char ch, [NotNullWhen(true)] out Token? token)
	{
		if (s_characterTokens.TryGetValue(ch, out token))
		{
			token = token.Emplace(line, column);
			return true;
		}

		return false;
	}
}
