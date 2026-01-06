using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.CodeModel;

public class DataTypeConverter
{
	static Dictionary<TokenType, DataType> s_dataTypeByTokenType =
		typeof(DataType).GetFields(BindingFlags.Public | BindingFlags.Static)
		.Where(field => field.Name != nameof(DataType.Unspecified))
		.Where(field => field.Name != nameof(DataType.UserDataType))
		.Select(field =>
			(
				DataType: (DataType)field.GetValue(null)!,
				TokenType: field.GetCustomAttribute<DataTypeTokenAttribute>()!.TokenType
			))
		.ToDictionary(key => key.TokenType, value => value.DataType);

	public static bool TryFromToken(Token token, [MaybeNullWhen(false)] out DataType dataType)
		=> s_dataTypeByTokenType.TryGetValue(token.Type, out dataType);

	public static DataType FromToken(Token token)
	{
		if (TryFromToken(token, out var dataType))
			return dataType;
		else
			throw new SyntaxErrorException(token, "Expected data type, found ");
	}
}
