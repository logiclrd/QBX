using System;
using System.Collections.Generic;

using QBX.Parser;

namespace QBX.ExecutionEngine.Compiled;

public class UserDataTypeFacade
{
	public UserDataType UnderlyingType { get; }

	public Identifier Name { get; }
	public List<Identifier> FieldNames { get; }

	public CodeModel.Statements.TypeStatement? Statement { get; }

	public UserDataTypeFacade(UserDataType underlyingType, Identifier name, List<Identifier> fieldNames, CodeModel.Statements.TypeStatement typeStatement)
	{
		UnderlyingType = underlyingType;

		Name = name;
		FieldNames = fieldNames;

		Statement = typeStatement;
	}

	public override string ToString()
	{
		if (Statement?.CodeLine?.CompilationElement is CodeModel.CompilationElement containingElement)
			return containingElement.Name?.ToString() + ":TYPE " + Name;
		else
			return "TYPE " + Name;
	}
}
