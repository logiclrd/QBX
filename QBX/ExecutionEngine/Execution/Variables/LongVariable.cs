using System;
using System.Runtime.InteropServices;

using QBX.ExecutionEngine.Compiled;
using QBX.Hardware;
using QBX.Numbers;
using QBX.Utility;

namespace QBX.ExecutionEngine.Execution.Variables;

public class LongVariable : Variable
{
	public int Value;

	public LongVariable()
		: base(DataType.Long)
	{
	}

	public LongVariable(int value)
		: this()
	{
		Value = value;
	}

	public LongVariable(uint value)
		: this()
	{
		Value = unchecked((int)value);
	}

	public override object GetData() => Value;
	public override void SetData(object value) => Value = NumberConverter.ToLong(value);

	public override int CoerceToInt(Evaluable? context) => NumberConverter.ToLong(Value, context?.Source?.Token);
	public override string ToString() => NumberFormatter.Format(Value);

	public override int Serialize(Span<byte> buffer)
		=> BitConverterEx.WriteBytesThatFit(buffer, Value);
	public override int Deserialize(ReadOnlySpan<byte> buffer)
	{
		Value = BitConverterEx.ReadAvailableBytesLong(buffer);
		return Math.Min(4, buffer.Length);
	}

	public override void Reset()
	{
		Value = 0;
	}

	public override bool IsZero => (Value == 0);
	public override bool IsPositive => (Value > 0);
	public override bool IsNegative => (Value < 0);
}

public class PinnedLongVariable : LongVariable
{
	Machine _machine;

	public Span<int> ValueSpan => MemoryMarshal.Cast<byte, int>(_machine.SystemMemory.AsSpan().Slice(PinnedMemoryAddress, 4));

	public PinnedLongVariable(Machine machine, int memoryAddress)
	{
		_machine = machine;

		PinnedMemoryAddress = memoryAddress;

		Value = ValueSpan[0];
	}

	public override void ReadPinnedData() => Value = ValueSpan[0];
	public override void WritePinnedData() => ValueSpan[0] = Value;

	public override object GetData() => Value = ValueSpan[0];
	public override void SetData(object value) => ValueSpan[0] = NumberConverter.ToLong(value);

	public override int Serialize(Span<byte> buffer)
		=> BitConverterEx.WriteBytesThatFit(buffer, ValueSpan[0]);
	public override int Deserialize(ReadOnlySpan<byte> buffer)
	{
		ValueSpan[0] = BitConverterEx.ReadAvailableBytesLong(buffer);
		return Math.Min(4, buffer.Length);
	}
}
