using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Hardware;
using QBX.Parser;

namespace QBX.ExecutionEngine.Execution;

public class Array
{
	public static int MaximumSize = 65536;

	public DataType ElementType;
	public ArraySubscripts Subscripts;
	public Variable?[] Elements;

	public Variable? PinnedMemoryOwner;

	public int FixedStringLength = -1;
	public int ElementSize;

	public bool IsPinned = false;
	public Machine? PinnedToMachine;
	public int PinnedToMemoryAddress;

	public bool IsDynamic = true;

	public Span<byte> PackedData
	{
		get
		{
			if (IsPinned)
			{
				var context = PinnedMemoryOwner!.PinnedMemoryContext!;
				var address = PinnedMemoryOwner!.PinnedMemoryAddress;

				return context.Machine.SystemMemory.AsSpan().Slice(address, PackedSize);
			}
			else
				return _packedData;
		}
	}

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

	public Array(DataType elementType, ArraySubscripts subscripts, int fixedStringLength = -1)
	{
		ElementType = elementType;
		Subscripts = subscripts;
		FixedStringLength = fixedStringLength;

		ElementSize = fixedStringLength < 0 ? ElementType.ByteSize : fixedStringLength;

		if (subscripts.ElementCount * ElementSize > MaximumSize)
			throw RuntimeException.SubscriptOutOfRange();

		Elements = new Variable?[subscripts.ElementCount];
	}

	Array(DataType elementType, ArraySubscripts subscripts, int fixedStringLength, ExecutionContext context, int memoryAddress)
	{
		ElementType = elementType;
		Subscripts = subscripts;
		FixedStringLength = fixedStringLength;

		IsPinned = true;
		PinnedToMachine = context.Machine;
		PinnedToMemoryAddress = memoryAddress;

		Elements = System.Array.Empty<Variable?>();
	}

	public static Array Pinned(DataType elementType, ArraySubscripts subscripts, int fixedStringLength, ExecutionContext context, int memoryAddress)
		=> new Array(elementType, subscripts, fixedStringLength, context, memoryAddress);

	public void Pin(ExecutionContext context)
	{
		if (IsPinned)
			return;

		IsPinned = true;

		PinnedMemoryOwner!.AllocatePinnedMemory(context, PackedSize);

		PinnedToMachine = context.Machine;
		PinnedToMemoryAddress = PinnedMemoryOwner.PinnedMemoryAddress;

		Pack(PackedData);
	}

	public void Unpin()
	{
		if (!IsPinned)
			return;

		Unpack(PackedData);

		PinnedMemoryOwner!.ReleasePinnedMemory();
		IsPinned = false;
	}

	Variable ConstructElement()
	{
		Variable variable;

		if ((FixedStringLength >= 0) && ElementType.IsString)
			variable = new StringVariable(FixedStringLength);
		else
			variable = Variable.Construct(ElementType);

		variable.PinnedMemoryOwner = PinnedMemoryOwner;

		return variable;
	}

	Variable ConstructPinnedElement(int offset)
	{
		// Assumes IsPinned
		Variable variable;

		if ((FixedStringLength >= 0) && ElementType.IsString)
			variable = new PinnedStringVariable(PinnedToMachine!, PinnedToMemoryAddress + offset, FixedStringLength);
		else
			variable = Variable.ConstructPinned(ElementType, PinnedMemoryOwner!.PinnedMemoryContext!, PinnedToMemoryAddress + offset);

		variable.PinnedMemoryOwner = PinnedMemoryOwner;

		return variable;
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

		if (!IsDynamic)
			throw RuntimeException.IllegalFunctionCall();

		if (newSubscripts.Dimensions != Subscripts.Dimensions)
			throw new Exception("Internal error: RedimensionPreservingData called with an ArraySubscripts with a different number of dimensions");

		if (IsPinned)
			Unpin();

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

		if (newElementCount * ElementType.ByteSize > MaximumSize)
			throw RuntimeException.SubscriptOutOfRange();

		var newElements = new Variable?[newElementCount];

		Elements.AsSpan().Slice(0, numPreservedSubarrays * subarraySize).CopyTo(newElements);

		Elements = newElements;
		Subscripts = newSubscripts;
	}

	public void Reset()
	{
		Elements.AsSpan().Clear();
	}

	public void Pack()
	{
		if (_packedData != null)
			throw new Exception("Array.Pack called when already packed");

		if (!IsPinned)
		{
			_packedData = new byte[PackedSize];

			Pack(_packedData);

			_packedDataDirty = false;
		}
	}

	public void Unpack()
	{
		if (_packedData == null)
			throw new Exception("Array.Unpack called when not packed");

		if (!IsPinned)
		{
			if (_packedDataDirty)
				Unpack(_packedData);

			_packedData = null;
		}
	}

	int Pack(Span<byte> buffer)
	{
		int lengthAtStart = buffer.Length;

		for (int i = 0; i < Elements.Length; i++)
		{
			if (Elements[i] is Variable element)
				element.Serialize(buffer);
			else
			{
				if (buffer.Length <= ElementSize)
				{
					buffer.Clear();
					break;
				}

				buffer.Slice(0, ElementSize).Clear();
			}

			if (buffer.Length <= ElementSize)
				break;

			buffer = buffer.Slice(ElementSize);
		}

		return lengthAtStart - buffer.Length;
	}

	[ThreadStatic]
	static byte[]? s_zeroes;

	void Unpack(ReadOnlySpan<byte> buffer)
	{
		if ((s_zeroes == null) || (s_zeroes.Length < ElementSize))
			s_zeroes = new byte[ElementSize * 2];

		var zeroElement = s_zeroes.AsSpan().Slice(0, ElementSize);

		for (int i = 0; i < Elements.Length; i++)
		{
			if ((buffer.Length >= ElementSize)
			 && MemoryExtensions.SequenceEqual(buffer.Slice(0, ElementSize), zeroElement))
				Elements[i] = null;
			else
			{
				var element =
					Elements[i] ??= ConstructElement();

				element.Deserialize(buffer);
			}

			if (buffer.Length <= ElementSize)
				break;

			buffer = buffer.Slice(ElementSize);
		}
	}

	public Variable GetElement(int index)
	{
		if (IsPinned)
			return ConstructPinnedElement(offset: index * ElementSize);
		else
		{
			EnsureUnpacked();

			return Elements[index] ??= ConstructElement();
		}
	}

	public Variable GetElement(Variable[] subscripts, IList<Evaluable> expressions) => GetElement(Subscripts.GetElementIndex(subscripts, expressions));

	public int Serialize(Span<byte> buffer)
	{
		if ((_packedData != null) || IsPinned)
		{
			var source = PackedData;

			if (source.Length > buffer.Length)
				source = source.Slice(0, buffer.Length);

			source.CopyTo(buffer);

			return source.Length;
		}
		else
			return Pack(buffer);
	}

	public int Deserialize(ReadOnlySpan<byte> buffer)
	{
		Elements.AsSpan().Clear();

		var packedSize = PackedSize;

		if (IsPinned)
			buffer.CopyTo(PackedData);
		else if (_packedData == null)
		{
			if (buffer.Length >= packedSize)
				_packedData = buffer.Slice(0, packedSize).ToArray();
			else
			{
				_packedData = new byte[packedSize];
				buffer.CopyTo(_packedData);
			}
		}
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

		return Math.Min(packedSize, buffer.Length);
	}
}
