using System;
using System.Runtime.InteropServices;

using QBX.ExecutionEngine.Compiled;
using QBX.Hardware;
using QBX.Numbers;
using QBX.Utility;

namespace QBX.ExecutionEngine.Execution.Variables;

public class DoubleVariable : Variable
{
	public double Value;

	public DoubleVariable()
		: base(DataType.Double)
	{
	}

	public DoubleVariable(double value)
		: this()
	{
		Value = value;
	}

	public override object GetData() => Value;
	public override void SetData(object value) => Value = NumberConverter.ToDouble(value);

	public override void SwapValueWith(Variable other)
	{
		if (other is not DoubleVariable otherDouble)
			throw RuntimeException.TypeMismatch();

		var tmp = Value;
		Value = otherDouble.Value;
		otherDouble.Value = tmp;
	}

	public override int CoerceToInt(Evaluable? context) => NumberConverter.ToLong(Value, context?.Source?.Token);
	public override string ToString() => NumberFormatter.Format(Value);

	public override int Serialize(Span<byte> buffer)
		=> BitConverterEx.WriteBytesThatFit(buffer, Value);
	public override int Deserialize(ReadOnlySpan<byte> buffer)
	{
		Value = BitConverterEx.ReadAvailableBytesDouble(buffer);
		return Math.Min(8, buffer.Length);
	}

	public override void Reset()
	{
		Value = 0;
	}

	public override bool IsZero => (Value == 0);
	public override bool IsPositive => (Value > 0);
	public override bool IsNegative => (Value < 0);
}

public class PinnedDoubleVariable : DoubleVariable
{
	Machine _machine;

	public Span<double> ValueSpan => MemoryMarshal.Cast<byte, double>(_machine.SystemMemory.AsSpan().Slice(PinnedMemoryAddress, 8));

	public PinnedDoubleVariable(Machine machine, int memoryAddress)
	{
		_machine = machine;

		PinnedMemoryAddress = memoryAddress;

		Value = ValueSpan[0];
	}

	public override void ReadPinnedData() => Value = ValueSpan[0];
	public override void WritePinnedData() => ValueSpan[0] = Value;

	public override object GetData() => Value = ValueSpan[0];
	public override void SetData(object value) => ValueSpan[0] = NumberConverter.ToDouble(value);

	public override int Serialize(Span<byte> buffer)
		=> BitConverterEx.WriteBytesThatFit(buffer, ValueSpan[0]);
	public override int Deserialize(ReadOnlySpan<byte> buffer)
	{
		ValueSpan[0] = BitConverterEx.ReadAvailableBytesDouble(buffer);
		return Math.Min(8, buffer.Length);
	}
}
