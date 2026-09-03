using System;

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

	public ArrayAllocationType AllocationType => Array.AllocationType;

	public bool IsCommonArray;

	public override void Reset()
	{
		switch (AllocationType)
		{
			case ArrayAllocationType.Static:
				Array.Reset();
				break;
			case ArrayAllocationType.Dynamic:
				Array = Array.Uninitialized;
				break;
		}
	}

	public override Variable Clone() => throw RuntimeException.IllegalFunctionCall();

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

	public override void SwapValueWith(Variable other)
		=> throw new NotImplementedException();

	public override int Serialize(System.Span<byte> buffer)
		=> Array.Serialize(buffer);
	public override int Deserialize(System.ReadOnlySpan<byte> buffer)
		=> Array.Deserialize(buffer);

	internal void InitializeArray(ArraySubscripts subscripts, ArrayAllocationType allocationType)
	{
		Array = new Array(ElementType, subscripts, allocationType, fixedStringLength);
		Array.PinnedMemoryOwner = this;
	}

	internal void InitializePinnedArray(ArraySubscripts subscripts, ArrayAllocationType allocationType, ExecutionContext context, int memoryAddress)
	{
		Array = Array.Pinned(ElementType, subscripts, allocationType, fixedStringLength, context, memoryAddress);
		Array.PinnedMemoryOwner = this;
	}

	public override bool SelfAllocateAndPin => true;

	public override void AllocateAndPin(ExecutionContext context)
	{
		Array.Pin(context);
	}
}
