using System.Collections.Generic;
using System.Linq;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine;

public class CommonBlockStorage
{
	public readonly Variable[] Variables;

	public CommonBlockStorage(IEnumerable<DataType> variableTypes)
	{
		Variables = variableTypes.Select(Construct).ToArray();
	}

	private CommonBlockStorage(Variable[] variables)
	{
		Variables = variables;
	}

	public CommonBlockStorage ExtendIfNecessary(IEnumerable<DataType> variableTypes)
	{
		var variableTypeList = variableTypes.ToList();

		if (variableTypeList.Count <= Variables.Length)
			return this;
		else
		{
			var extendedVariables = new Variable[variableTypeList.Count];

			for (int i = Variables.Length; i < extendedVariables.Length; i++)
				extendedVariables[i] = Construct(variableTypeList[i]);

			return new CommonBlockStorage(extendedVariables);
		}
	}

	static Variable Construct(DataType dataType)
	{
		if (!dataType.IsArray)
			return Variable.Construct(dataType);
		else
		{
			var arrayVariable = Variable.ConstructArray(dataType);

			arrayVariable.IsCommonArray = true;

			return arrayVariable;
		}
	}
}
