using QBX.ExecutionEngine.Compiled;
using QBX.LexicalAnalysis;

namespace QBX.ExecutionEngine;

public class TypeRepository
{
	Dictionary<string, DataType> _typeByName = new Dictionary<string, DataType>(StringComparer.OrdinalIgnoreCase);

	public void RegisterType(UserDataType userType)
	{
		if (_typeByName.ContainsKey(userType.Name))
		{
			throw new RuntimeException(
				userType.Statement?.NameToken,
				"Duplicate definition");
		}

		_typeByName[userType.Name] = new DataType(userType);
	}

	public DataType ResolveType(CodeModel.DataType primitiveType, string? userTypeName, Token? context)
	{
		if (userTypeName == null)
			return DataType.FromCodeModelDataType(primitiveType);

		if (_typeByName.TryGetValue(userTypeName, out var type))
			return type;

		throw new RuntimeException(context, "Type not defined");
	}

	public DataType ResolveType(CodeModel.ParameterDefinition param)
	{
		if (param.AnyType)
			throw new Exception("Internal error: Cannot resolve ANY to a DataType");

		return ResolveType(param.Type, param.UserType, param.TypeToken);
	}
}
