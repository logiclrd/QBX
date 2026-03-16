using System.Collections.Generic;
using System.Linq;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine;

public class CommonBlockStorage(IEnumerable<DataType> variableTypes)
{
	public readonly Variable[] Variables = variableTypes.Select(Construct).ToArray();

	static Variable Construct(DataType dataType)
	{
		if (dataType.IsArray)
			return Variable.ConstructArray(dataType);
		else
			return Variable.Construct(dataType);
	}
}
