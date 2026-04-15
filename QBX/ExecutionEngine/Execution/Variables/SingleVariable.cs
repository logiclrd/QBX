using System;
using System.Runtime.InteropServices;

using QBX.ExecutionEngine.Compiled;
using QBX.Hardware;
using QBX.Numbers;
using QBX.Utility;

namespace QBX.ExecutionEngine.Execution.Variables;

public class SingleVariable : Variable
{
	public float Value;

	public SingleVariable()
		: base(DataType.Single)
	{
	}

	public SingleVariable(float value)
		: this()
	{
		Value = value;
	}

	public override object GetData() => Value;
	public override void SetData(object value) => Value = NumberConverter.ToSingle(value);

	public override void SwapValueWith(Variable other)
	{
		if (other is not SingleVariable otherSingle)
			throw RuntimeException.TypeMismatch();

		var tmp = Value;
		Value = otherSingle.Value;
		otherSingle.Value = tmp;
	}

	public override Variable Clone()
		=> new SingleVariable(Value);

	public override int CoerceToInt(Evaluable? context) => NumberConverter.ToLong(Value, context?.Source?.Token);
	public override string ToString() => NumberFormatter.Format(Value);

	public override int Serialize(Span<byte> buffer)
		=> BitConverterEx.WriteBytesThatFit(buffer, Value);
	public override int Deserialize(ReadOnlySpan<byte> buffer)
	{
		Value = BitConverterEx.ReadAvailableBytesSingle(buffer);
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

public class PinnedSingleVariable : SingleVariable
{
	Machine _machine;

	public Span<float> ValueSpan => MemoryMarshal.Cast<byte, float>(_machine.SystemMemory.AsSpan().Slice(PinnedMemoryAddress, 4));

	public PinnedSingleVariable(Machine machine, int memoryAddress)
	{
		_machine = machine;

		PinnedMemoryAddress = memoryAddress;

		Value = ValueSpan[0];
	}

	public override void ReadPinnedData() => Value = ValueSpan[0];
	public override void WritePinnedData() => ValueSpan[0] = Value;

	public override object GetData() => Value = ValueSpan[0];
	public override void SetData(object value) => ValueSpan[0] = NumberConverter.ToSingle(value);

	public override int Serialize(Span<byte> buffer)
		=> BitConverterEx.WriteBytesThatFit(buffer, ValueSpan[0]);
	public override int Deserialize(ReadOnlySpan<byte> buffer)
	{
		ValueSpan[0] = BitConverterEx.ReadAvailableBytesSingle(buffer);
		return Math.Min(4, buffer.Length);
	}
}
