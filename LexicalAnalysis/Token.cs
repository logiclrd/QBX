using QBX.CodeModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace QBX.LexicalAnalysis;

public class Token(int line, int column, TokenType type, string value, decimal? numericValue = null)
{
	public TokenType Type => type;
	public string? Value => value;
	public decimal? NumericValue => numericValue;

	public int Line => line;
	public int Column => column;

	public bool IsDataType => DataTypeConverter.TryFromToken(this, out var _);

	Token Emplace(int newLine, int newColumn) => new Token(newLine, newColumn, type, value, numericValue);

	public override string ToString()
	{
		if (Value == null)
			return Type.ToString();
		else
			return Type + ": " + Value;
	}

	static Dictionary<string, Token> s_keywordTokens =
		typeof(TokenType).GetFields(BindingFlags.Public | BindingFlags.Static)
		.Select(f => (TokenType: (TokenType)f.GetValue(null)!, Keyword: f.GetCustomAttribute<KeywordTokenAttribute>()))
		.Where(f => f.Keyword != null)
		.ToDictionary(key => key.Keyword!.Keyword ?? key.TokenType.ToString(), value => new Token(0, 0, value.TokenType, value.Keyword?.Keyword ?? value.TokenType.ToString()), StringComparer.OrdinalIgnoreCase);

	static Dictionary<char, Token> s_characterTokens =
		typeof(TokenType).GetFields(BindingFlags.Public | BindingFlags.Static)
		.Select(f => (TokenType: (TokenType)f.GetValue(null)!, TokenCharacter: f.GetCustomAttribute<TokenCharacterAttribute>()))
		.Where(f => f.TokenCharacter != null)
		.ToDictionary(key => key.TokenCharacter!.Character, value => new Token(0, 0, value.TokenType, ""));

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
