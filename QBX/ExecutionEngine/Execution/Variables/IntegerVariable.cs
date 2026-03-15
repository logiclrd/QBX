using System;
using System.Runtime.InteropServices;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.Hardware;
using QBX.Numbers;
using QBX.Utility;

namespace QBX.ExecutionEngine.Execution.Variables;

public class IntegerVariable : Variable
{
	public short Value;

	public IntegerVariable()
		: base(DataType.Integer)
	{
	}

	public IntegerVariable(short value)
		: this()
	{
		Value = value;
	}

	public IntegerVariable(ushort value)
		: this()
	{
		Value = unchecked((short)value);
	}

	public IntegerVariable(bool value)
		: this(value ? IntegerLiteralValue.True : IntegerLiteralValue.False)
	{
	}

	public override object GetData() => Value;
	public override void SetData(object value) => Value = NumberConverter.ToInteger(value);

	public override int CoerceToInt(Evaluable? context) => NumberConverter.ToLong(Value, context?.Source?.Token);
	public override string ToString() => NumberFormatter.Format(Value);

	public override int Serialize(Span<byte> buffer)
		=> BitConverterEx.WriteBytesThatFit(buffer, Value);
	public override int Deserialize(ReadOnlySpan<byte> buffer)
	{
		Value = BitConverterEx.ReadAvailableBytesInteger(buffer);
		return Math.Min(2, buffer.Length);
	}

	public override void Reset()
	{
		Value = 0;
	}

	public override bool IsZero => (Value == 0);
	public override bool IsPositive => (Value > 0);
	public override bool IsNegative => (Value < 0);
}

public class PinnedIntegerVariable : IntegerVariable
{
	Machine _machine;

	public Span<short> ValueSpan => MemoryMarshal.Cast<byte, short>(_machine.SystemMemory.AsSpan().Slice(PinnedMemoryAddress, 2));

	public PinnedIntegerVariable(Machine machine, int memoryAddress)
	{
		_machine = machine;

		PinnedMemoryAddress = memoryAddress;

		Value = ValueSpan[0];
	}

	public override void ReadPinnedData() => Value = ValueSpan[0];
	public override void WritePinnedData() => ValueSpan[0] = Value;

	public override object GetData() => Value = ValueSpan[0];
	public override void SetData(object value) => ValueSpan[0] = NumberConverter.ToInteger(value);

	public override int Serialize(Span<byte> buffer)
		=> BitConverterEx.WriteBytesThatFit(buffer, ValueSpan[0]);
	public override int Deserialize(ReadOnlySpan<byte> buffer)
	{
		ValueSpan[0] = BitConverterEx.ReadAvailableBytesInteger(buffer);
		return Math.Min(2, buffer.Length);
	}
}
