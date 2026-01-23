using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Parser;

namespace QBX.ExecutionEngine.Execution;

public class Array
{
	public DataType ElementType;
	public ArraySubscripts Subscripts;
	public Variable?[] Elements;

	public Span<byte> PackedData => _packedData;
	public int PackedSize => Elements.Length * ElementType.ByteSize;

	byte[]? _packedData;
	bool _packedDataDirty;

	public static readonly Array Uninitialized = new Array(DataType.Integer, new ArraySubscripts());

	public bool IsUninitialized => ReferenceEquals(this, Uninitialized);

	public bool IsPacked => (_packedData != null);

	public void EnsurePacked()
	{
		if (_packedData == null)
			Pack();
	}

	public void EnsureUnpacked()
	{
		if (_packedData != null)
			Unpack();
	}

	public Array(DataType elementType, ArraySubscripts subscripts)
	{
		ElementType = elementType;
		Subscripts = subscripts;

		Elements = new Variable?[subscripts.ElementCount];
	}

	public void RedimensionPreservingData(ArraySubscripts newSubscripts)
	{
		// Data is laid out in the array so that each successive dimension can be interpreted
		// as an array of the array described by the preceding dimensions.
		//
		// (1 TO 10) = 10 consecutive integers
		// (1 TO 10, 1 TO 5) = 5 consecutive copies of (1 TO 10)
		// (1 TO 10, 1 TO 5, 1 TO 8) = 8 consecutive copies of (1 TO 10, 1 TO 5)
		//
		// This allows for REDIM PRESERVE, with its restriction that only the upper bound of
		// the last dimension can be changed, to be implemented as a raw copy of the elements
		// without having to reorganize the array.
		//
		// This copy requires knowing the size of that second-to-last subarray, e.g.
		// (1 TO 10, 1 TO 5) in the example above.

		if (newSubscripts.Dimensions != Subscripts.Dimensions)
			throw new Exception("Internal error: RedimensionPreservingData called with an ArraySubscripts with a different number of dimensions");

		for (int i = 0; i < Subscripts.Dimensions - 1; i++)
		{
			var oldSubscript = Subscripts[i];
			var newSubscript = newSubscripts[i];

			if ((oldSubscript.LowerBound != newSubscript.LowerBound)
			 || (oldSubscript.UpperBound != newSubscript.UpperBound))
				throw RuntimeException.SubscriptOutOfRange();
		}

		var oldLastSubscript = Subscripts[Subscripts.Dimensions - 1];
		var newLastSubscript = newSubscripts[newSubscripts.Dimensions - 1];

		if (oldLastSubscript.LowerBound != newLastSubscript.LowerBound)
			throw RuntimeException.SubscriptOutOfRange();

		int subarraySize = Subscripts.GetSubarraySize();

		int oldSubarrayCount = oldLastSubscript.ElementCount;
		int newSubarrayCount = newLastSubscript.ElementCount;

		int numPreservedSubarrays = Math.Min(oldSubarrayCount, newSubarrayCount);

		int newElementCount = newSubarrayCount * subarraySize;

		if (newElementCount != newSubscripts.ElementCount)
			throw new Exception("Sanity check failed");

		var newElements = new Variable?[newElementCount];

		Elements.AsSpan().Slice(0, numPreservedSubarrays * subarraySize).CopyTo(newElements);

		Elements = newElements;
		Subscripts = newSubscripts;
	}

	public void Pack()
	{
		if (_packedData != null)
			throw new Exception("Array.Pack called when already packed");

		_packedData = new byte[PackedSize];

		Pack(_packedData);

		_packedDataDirty = false;
	}

	public void Unpack()
	{
		if (_packedData == null)
			throw new Exception("Array.Unpack called when not packed");

		if (_packedDataDirty)
			Unpack(_packedData);

		_packedData = null;
	}

	void Pack(Span<byte> buffer)
	{
		int elementSize = ElementType.ByteSize;

		for (int i = 0; i < Elements.Length; i++)
		{
			if (Elements[i] is Variable element)
				element.Serialize(buffer);
			else
			{
				if (buffer.Length <= elementSize)
				{
					buffer.Clear();
					break;
				}

				buffer.Slice(0, elementSize).Clear();
			}

			if (buffer.Length <= elementSize)
				break;

			buffer = buffer.Slice(elementSize);
		}
	}

	[ThreadStatic]
	static byte[]? s_zeroes;

	void Unpack(ReadOnlySpan<byte> buffer)
	{
		int elementSize = ElementType.ByteSize;

		if ((s_zeroes == null) || (s_zeroes.Length < elementSize))
			s_zeroes = new byte[elementSize * 2];

		var zeroElement = s_zeroes.AsSpan().Slice(0, elementSize);

		for (int i = 0; i < Elements.Length; i++)
		{
			if ((buffer.Length >= elementSize)
			 && MemoryExtensions.SequenceEqual(buffer.Slice(0, elementSize), zeroElement))
				Elements[i] = null;
			else
			{
				var element =
					Elements[i] ??= Variable.Construct(ElementType);

				element.Deserialize(buffer);
			}

			if (buffer.Length <= elementSize)
				break;

			buffer = buffer.Slice(elementSize);
		}
	}

	public Variable GetElement(int index)
	{
		EnsureUnpacked();

		return Elements[index] ??= Variable.Construct(ElementType);
	}

	public Variable GetElement(Variable[] subscripts, IList<Evaluable> expressions) => GetElement(Subscripts.GetElementIndex(subscripts, expressions));

	public void Serialize(Span<byte> buffer)
	{
		if (_packedData != null)
		{
			var source = _packedData.AsSpan();

			if (source.Length > buffer.Length)
				source = source.Slice(0, buffer.Length);

			source.CopyTo(buffer);
		}
		else
			Pack(buffer);
	}

	public void Deserialize(ReadOnlySpan<byte> buffer)
	{
		Elements.AsSpan().Clear();

		var packedSize = PackedSize;

		if (_packedData == null)
			_packedData = buffer.Slice(0, packedSize).ToArray();
		else
		{
			if (buffer.Length >= packedSize)
				buffer.Slice(0, packedSize).CopyTo(_packedData);
			else
			{
				buffer.CopyTo(_packedData);
				_packedData.Slice(packedSize).Clear();
			}
		}

		_packedDataDirty = true;
	}
}
