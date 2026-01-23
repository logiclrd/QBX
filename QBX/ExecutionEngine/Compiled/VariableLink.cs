namespace QBX.ExecutionEngine.Compiled;

public class VariableLink
{
	public int LocalIndex;
	public int RootIndex;

	public override string ToString()
	{
		return "local[" + LocalIndex + "] => root[" + RootIndex + "]";
	}
}
