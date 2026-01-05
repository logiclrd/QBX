using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Execution;

public class UserDataTypeValue
{
	public Variable[] Fields;

	public UserDataTypeValue(UserDataType dataType)
	{
		Fields = new Variable[dataType.Members.Count];

		for (int i = 0; i < dataType.Members.Count; i++)
			Fields[i] = Variable.Construct(dataType.Members[i].Type);
	}
}
