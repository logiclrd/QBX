using System;
using System.Collections.Generic;
using System.Linq;

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

	public DataType ResolveType(string userType, Token? context = null)
		=> ResolveType(CodeModel.DataType.UserDataType, userType, fixedStringLength: 0, isArray: false, context);

	public DataType ResolveType(CodeModel.DataType primitiveType, string? userTypeName, int fixedStringLength, bool isArray, Token? context)
	{
		if (isArray)
		{
			var scalarType = ResolveType(primitiveType, userTypeName, fixedStringLength, isArray: false, context);

			return scalarType.MakeArrayType();
		}

		if (userTypeName == null)
			return DataType.FromCodeModelDataType(primitiveType, fixedStringLength);

		if (_typeByName.TryGetValue(userTypeName, out var type))
			return type;

		throw new RuntimeException(context, "Type not defined");
	}

	public DataType ResolveType(CodeModel.ParameterDefinition param, Mapper mapper)
	{
		if (param.AnyType)
			throw new Exception("Internal error: Cannot resolve ANY to a DataType");

		if (CodeModel.TypeCharacter.TryParse(param.Name.Last(), out var typeCharacter))
			return ResolveType(typeCharacter.Type, null, 0, param.IsArray, param.NameToken);
		else if ((param.Type != CodeModel.DataType.Unspecified) || (param.UserType != null))
			return ResolveType(param.Type, param.UserType, 0, param.IsArray, param.TypeToken);
		else
			return DataType.ForPrimitiveDataType(mapper.GetTypeForIdentifier(param.Name));
	}
}
