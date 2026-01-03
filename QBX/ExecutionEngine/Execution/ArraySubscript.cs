namespace QBX.ExecutionEngine.Execution;

public class ArraySubscript
{
	public int LowerBound;
	public int UpperBound;

	public int ElementCount => UpperBound - LowerBound + 1;
}
