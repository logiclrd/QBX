using System;
using System.Runtime.InteropServices;

using QBX.ExecutionEngine.Compiled;
using QBX.Hardware;
using QBX.Numbers;
using QBX.Utility;

namespace QBX.ExecutionEngine.Execution.Variables;

public class CurrencyVariable : Variable
{
	public decimal Value;

	public CurrencyVariable()
		: base(DataType.Currency)
	{
	}

	public CurrencyVariable(decimal value)
		: this()
	{
		Value = value;
	}

	public override object GetData() => Value;
	public override void SetData(object value) => Value = NumberConverter.ToCurrency(value);

	public override int CoerceToInt(Evaluable? context) => NumberConverter.ToLong(Value, context?.Source?.Token);
	public override string ToString() => NumberFormatter.Format(Value);

	public override int Serialize(Span<byte> buffer)
	{
		BitConverterEx.WriteBytesThatFit(buffer, decimal.ToOACurrency(Value));
		return 8;
	}

	public override int Deserialize(ReadOnlySpan<byte> buffer)
	{
		Value = decimal.FromOACurrency(BitConverterEx.ReadAvailableBytesInt64(buffer));
		return 8;
	}

	public override void Reset()
	{
		Value = 0;
	}

	public override bool IsZero => (Value == 0);
	public override bool IsPositive => (Value > 0);
	public override bool IsNegative => (Value < 0);
}

public class PinnedCurrencyVariable : CurrencyVariable
{
	Machine _machine;

	public Span<long> ValueSpan => MemoryMarshal.Cast<byte, long>(_machine.SystemMemory.AsSpan().Slice(PinnedMemoryAddress, 8));

	public PinnedCurrencyVariable(Machine machine, int memoryAddress)
	{
		_machine = machine;

		PinnedMemoryAddress = memoryAddress;

		ReadPinnedData();
	}

	public override void ReadPinnedData() => Value = decimal.FromOACurrency(ValueSpan[0]);
	public override void WritePinnedData() => ValueSpan[0] = decimal.ToOACurrency(Value);

	public override object GetData() => Value = decimal.FromOACurrency(ValueSpan[0]);
	public override void SetData(object value) => ValueSpan[0] = decimal.ToOACurrency(NumberConverter.ToCurrency(value));

	public override int Serialize(Span<byte> buffer)
		=> BitConverterEx.WriteBytesThatFit(buffer, ValueSpan[0]);
	public override int Deserialize(ReadOnlySpan<byte> buffer)
	{
		ValueSpan[0] = BitConverterEx.ReadAvailableBytesInt64(buffer);
		return Math.Min(8, buffer.Length);
	}
}
