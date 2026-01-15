using System;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Parser;

namespace QBX.ExecutionEngine.Execution;

public class Array
{
	public DataType ElementType;
	public ArraySubscripts Subscripts;
	public Variable?[] Elements;

	public ReadOnlyMemory<byte>? PackedData => _packedData;
	public int PackedSize => Elements.Length * ElementType.ByteSize;

	byte[]? _packedData;
	bool _packedDataDirty;

	public static readonly Array Uninitialized = new Array(DataType.Integer, new ArraySubscripts());

	public bool IsUninitialized => ReferenceEquals(this, Uninitialized);

	public Array(DataType elementType, ArraySubscripts subscripts)
	{
		ElementType = elementType;
		Subscripts = subscripts;

		Elements = new Variable?[subscripts.ElementCount];
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

	void EnsureUnpacked()
	{
		if (_packedData != null)
			Unpack();
	}

	public Variable GetElement(int index)
	{
		EnsureUnpacked();

		return Elements[index] ??= Variable.Construct(ElementType);
	}

	public Variable GetElement(int[] subscripts) => GetElement(Subscripts.GetElementIndex(subscripts));
	public Variable GetElement(Variable[] subscripts) => GetElement(Subscripts.GetElementIndex(subscripts));

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
