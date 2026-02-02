using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine.Execution.Variables;

public class ArrayVariable(DataType type, int fixedStringLength = -1) : Variable(type)
{
	public override bool IsZero => false;
	public override bool IsPositive => false;
	public override bool IsNegative => false;

	public override int CoerceToInt(Evaluable? context) => throw CompilerException.TypeMismatch(context?.Source);

	public DataType ElementType { get; } = type.MakeElementType();

	public Array Array = Array.Uninitialized;

	public override object GetData() => Array;
	public override void SetData(object value) => Array = (Array)value;

	public override void Serialize(System.Span<byte> buffer)
		=> Array.Serialize(buffer);
	public override void Deserialize(System.ReadOnlySpan<byte> buffer)
		=> Array.Deserialize(buffer);

	internal void InitializeArray(ArraySubscripts subscripts)
	{
		Array = new Array(ElementType, subscripts, fixedStringLength);
	}
}
