using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QBX.LexicalAnalysis;

public static class TokenTypeExtensions
{
	static Dictionary<TokenType, FieldInfo> s_tokenTypeFieldByTokenType = typeof(TokenType)
		.GetFields(BindingFlags.Static | BindingFlags.Public)
		.Select(fieldInfo => (FieldInfo: fieldInfo, TokenType: (TokenType)fieldInfo.GetValue(default)!))
		.DistinctBy(pair => pair.TokenType)
		.ToDictionary(key => key.TokenType, element => element.FieldInfo);

	public static string GetString(this TokenType tokenType)
	{
		if (tokenType == TokenType.Empty)
			return "";

		if (s_tokenTypeFieldByTokenType.TryGetValue(tokenType, out var fieldInfo))
		{
			if (fieldInfo.GetCustomAttribute<ValueTokenAttribute>() is not null)
				return "";

			if (fieldInfo.GetCustomAttribute<TokenCharacterAttribute>() is TokenCharacterAttribute tokenCharacter)
				return tokenCharacter.Character.ToString();

			if (fieldInfo.GetCustomAttribute<TokenValueAttribute>() is TokenValueAttribute tokenValueAttribute)
				return tokenValueAttribute.Value;

			if ((fieldInfo.GetCustomAttribute<KeywordTokenAttribute>() is KeywordTokenAttribute keywordTokenAttribute)
			 && (keywordTokenAttribute.Keyword is string keyword))
				return keyword;
		}

		return tokenType.ToString();
	}
}
