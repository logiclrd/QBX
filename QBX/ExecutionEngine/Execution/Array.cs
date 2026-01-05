using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Execution;

public class Array
{
	public DataType ElementType;
	public ArraySubscripts Subscripts;
	public Variable[] Elements;

	public Array(DataType elementType, ArraySubscripts subscripts)
	{
		ElementType = elementType;
		Subscripts = subscripts;

		Elements = new Variable[subscripts.ElementCount];
	}

	public Variable GetElement(int index)
	{
		return Elements[index] ??= Variable.Construct(ElementType);
	}

	public Variable GetElement(int[] subscripts) => GetElement(Subscripts.GetElementIndex(subscripts));
	public Variable GetElement(Variable[] subscripts) => GetElement(Subscripts.GetElementIndex(subscripts));
}
