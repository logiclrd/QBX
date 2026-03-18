using System.Collections.Generic;

using QBX.ExecutionEngine.Compiled;
using QBX.Parser;

namespace QBX.ExecutionEngine;

public class CommonBlock(Identifier name)
{
	public const string DefaultBlockNameValue = "COMMON$";

	public static readonly Identifier DefaultBlockName = Identifier.Standalone(DefaultBlockNameValue);

	public Identifier Name => name;
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
