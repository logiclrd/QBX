using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine.Execution;

public class UserDataTypeValue
{
	public Variable[] Fields;

	public UserDataTypeValue(UserDataType dataType)
	{
		Fields = new Variable[dataType.Members.Count];

		for (int i = 0; i < dataType.Members.Count; i++)
			Fields[i] = new Variable(dataType.Members[i].Type);
	}
}
