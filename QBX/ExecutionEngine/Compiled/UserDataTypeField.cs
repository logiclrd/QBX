using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled;

public class UserDataTypeField(DataType type, ArraySubscripts? subscripts)
{
	public DataType Type => type;
	public ArraySubscripts? ArraySubscripts => subscripts;

	public override int GetHashCode()
	{
		if (subscripts != null)
			return type.GetHashCode() ^ subscripts.GetHashCode();
		else
			return type.GetHashCode();
	}
}
