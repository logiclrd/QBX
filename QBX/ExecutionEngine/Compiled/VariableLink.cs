namespace QBX.ExecutionEngine.Compiled;

public class VariableLink
{
	public int LocalIndex;
	public int RemoteIndex;

	public override string ToString()
	{
		return "local[" + LocalIndex + "] => remote[" + RemoteIndex + "]";
	}
}
