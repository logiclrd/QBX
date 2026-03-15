using System;
using System.Collections.Generic;

namespace QBX.ExecutionEngine.Compiled;

public class UserDataTypeFacade
{
	public UserDataType UnderlyingType { get; }

	public string Name { get; }
	public List<string> FieldNames { get; } = new List<string>();

	public CodeModel.Statements.TypeStatement? Statement { get; }

	public UserDataTypeFacade(UserDataType underlyingType, string name, List<string> fieldNames, CodeModel.Statements.TypeStatement typeStatement)
	{
		UnderlyingType = underlyingType;

		Name = name;
		FieldNames = fieldNames;

		Statement = typeStatement;
	}

	public override string ToString()
	{
		if (Statement?.CodeLine?.CompilationElement is CodeModel.CompilationElement containingElement)
			return containingElement.Name + ":TYPE " + Name;
		else
			return "TYPE " + Name;
	}
}
