using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Execution;

public class Array
{
	public DataType ElementType;
	public ArraySubscripts Subscripts;
	public Variable[] Elements;

	public static readonly Array Uninitialized = new Array(DataType.Integer, new ArraySubscripts());

	public bool IsUninitialized => ReferenceEquals(this, Uninitialized);

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
