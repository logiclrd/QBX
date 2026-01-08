using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine.Execution.Variables;

public class ArrayVariable(DataType type) : Variable(type)
{
	public override bool IsZero => false;
	public override bool IsPositive => false;
	public override bool IsNegative => false;

	public override int CoerceToInt() => throw new System.InvalidOperationException();

	public DataType ElementType { get; } = type.MakeElementType();

	public Array Array = Array.Uninitialized;

	public override object GetData() => Array;
	public override void SetData(object value) => Array = (Array)value;

	internal void InitializeArray(ArraySubscripts subscripts)
	{
		Array = new Array(ElementType, subscripts);
	}
}
