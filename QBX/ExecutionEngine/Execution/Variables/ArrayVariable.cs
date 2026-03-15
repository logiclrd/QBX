using System.Linq;

using QBX.ExecutionEngine.Compiled;
using QBX.Hardware;

namespace QBX.ExecutionEngine.Execution.Variables;

public class ArrayVariable(DataType type, int fixedStringLength = -1) : Variable(type)
{
	public override bool IsZero => false;
	public override bool IsPositive => false;
	public override bool IsNegative => false;

	public override int CoerceToInt(Evaluable? context) => throw CompilerException.TypeMismatch(context?.Source);

	public DataType ElementType { get; } = type.MakeElementType();

	public Array Array = Array.Uninitialized;

	public bool IsDynamic;

	public override void Reset()
	{
		if (IsDynamic)
			Array = Array.Uninitialized;
		else
			Array.Reset();
	}

	public override object GetData() => Array;

	public override void SetData(object value)
	{
		if ((value is not ArrayVariable arrayValue)
		 || !arrayValue.ElementType.Equals(this.ElementType)
		 || (arrayValue.Array.Elements.Length != this.Array.Elements.Length))
			throw RuntimeException.TypeMismatch();

		Array.EnsureUnpacked();
		arrayValue.Array.EnsureUnpacked();

		for (int i = 0; i < Array.Elements.Length; i++)
		{
			var thisElement = Array.Elements[i];
			var otherElement = arrayValue.Array.Elements[i];

			if (thisElement == null)
				Array.Elements[i] = otherElement;
			else if (otherElement == null)
				Array.Elements[i] = null;
			else
				thisElement.SetData(otherElement.GetData());
		}
	}

	public override int Serialize(System.Span<byte> buffer)
		=> Array.Serialize(buffer);
	public override int Deserialize(System.ReadOnlySpan<byte> buffer)
		=> Array.Deserialize(buffer);

	internal void InitializeArray(ArraySubscripts subscripts, bool isDynamic = true)
	{
		Array = new Array(ElementType, subscripts, fixedStringLength);
		Array.IsDynamic = IsDynamic;
		Array.PinnedMemoryOwner = this;
	}

	internal void InitializePinnedArray(ArraySubscripts subscripts, ExecutionContext context, int memoryAddress)
	{
		Array = Array.Pinned(ElementType, subscripts, fixedStringLength, context, memoryAddress);
		Array.PinnedMemoryOwner = this;
	}
}
