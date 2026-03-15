using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine;

public class CommonBlock(string name)
{
	public const string DefaultBlockName = "COMMON$";

	public string Name => name;
	public List<DataType> VariableTypes = new List<DataType>();

	public void MapVariables(IEnumerable<DataType> declaredTypes)
	{
		int index = 0;

		foreach (var declaredType in declaredTypes)
		{
			if (index >= VariableTypes.Count)
				VariableTypes.Add(declaredType);
			else if (!declaredType.Equals(VariableTypes[index]))
				throw CompilerException.TypeMismatch();

			index++;
		}
	}

	public CommonBlockStorage CreateStorage()
		=> new CommonBlockStorage(VariableTypes);
}
