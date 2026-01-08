namespace QBX.ExecutionEngine.Execution;

public class ArraySubscript
{
	public short LowerBound;
	public short UpperBound;

	public int ElementCount => UpperBound - LowerBound + 1;
}
